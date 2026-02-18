using InventoryAndSalesSystem.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InventoryAndSalesSystem.Services
{
    /// <summary>
    /// Manages cycle count sessions: snapshot expected stock, accept physical counts,
    /// compute variances, and persist results in CycleCounts.xlsx.
    /// </summary>
    public class CycleCountService
    {
        private readonly string _cycleCountFile;

        public CycleCountService(string dataFolder)
        {
            _cycleCountFile = Path.Combine(dataFolder, "CycleCounts.xlsx");
            InitializeFile();
        }

        private void InitializeFile()
        {
            if (File.Exists(_cycleCountFile)) return;

            using var package = new ExcelPackage();

            var sessions = package.Workbook.Worksheets.Add("Sessions");
            sessions.Cells[1, 1].Value = "SessionId";
            sessions.Cells[1, 2].Value = "StartDate";
            sessions.Cells[1, 3].Value = "CompletedDate";
            sessions.Cells[1, 4].Value = "Status";     // Open / Completed
            sessions.Cells[1, 5].Value = "Notes";

            var items = package.Workbook.Worksheets.Add("Items");
            items.Cells[1, 1].Value = "SessionId";
            items.Cells[1, 2].Value = "ProductId";
            items.Cells[1, 3].Value = "ProductName";
            items.Cells[1, 4].Value = "Category";
            items.Cells[1, 5].Value = "ExpectedQty";
            items.Cells[1, 6].Value = "CountedQty";
            items.Cells[1, 7].Value = "Variance";       // Counted - Expected
            items.Cells[1, 8].Value = "UnitCost";
            items.Cells[1, 9].Value = "VarianceValue";  // Variance * UnitCost
            items.Cells[1, 10].Value = "Notes";
            items.Cells[1, 11].Value = "Counted";       // bool

            package.SaveAs(new FileInfo(_cycleCountFile));
        }

        // ── Sessions ────────────────────────────────────────────────────────

        public int CreateSession(List<Product> products, string notes = "")
        {
            using var package = new ExcelPackage(new FileInfo(_cycleCountFile));

            var sessWs = package.Workbook.Worksheets["Sessions"];
            var sessionRows = sessWs.Dimension?.Rows ?? 1;
            var newSessionId = sessionRows; // header = 1, first data = row 2 → id 1

            var newRow = sessionRows + 1;
            sessWs.Cells[newRow, 1].Value = newSessionId;
            sessWs.Cells[newRow, 2].Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt");
            sessWs.Cells[newRow, 3].Value = "";
            sessWs.Cells[newRow, 4].Value = "Open";
            sessWs.Cells[newRow, 5].Value = notes;

            var itemWs = package.Workbook.Worksheets["Items"];
            var itemRows = itemWs.Dimension?.Rows ?? 1;
            var itemRow = itemRows + 1;

            foreach (var p in products)
            {
                itemWs.Cells[itemRow, 1].Value = newSessionId;
                itemWs.Cells[itemRow, 2].Value = p.Id;
                itemWs.Cells[itemRow, 3].Value = p.Name;
                itemWs.Cells[itemRow, 4].Value = p.Category;
                itemWs.Cells[itemRow, 5].Value = p.Stock;   // expected = current system stock
                itemWs.Cells[itemRow, 6].Value = -1;        // -1 = not yet counted
                itemWs.Cells[itemRow, 7].Value = 0;
                itemWs.Cells[itemRow, 8].Value = (double)p.UnitCost;
                itemWs.Cells[itemRow, 9].Value = 0;
                itemWs.Cells[itemRow, 10].Value = "";
                itemWs.Cells[itemRow, 11].Value = false;
                itemRow++;
            }

            package.Save();
            return newSessionId;
        }

        public List<CycleCountSession> GetAllSessions()
        {
            var sessions = new List<CycleCountSession>();
            using var package = new ExcelPackage(new FileInfo(_cycleCountFile));
            var ws = package.Workbook.Worksheets["Sessions"];
            var rows = ws.Dimension?.Rows ?? 1;

            for (int r = 2; r <= rows; r++)
            {
                if (ws.Cells[r, 1].Value == null) continue;
                sessions.Add(new CycleCountSession
                {
                    SessionId = int.Parse(ws.Cells[r, 1].Value.ToString()!),
                    StartDate = ParseDate(ws.Cells[r, 2].Value),
                    CompletedDate = ws.Cells[r, 3].Value?.ToString() is { Length: > 0 } s
                        ? ParseDate(ws.Cells[r, 3].Value) : (DateTime?)null,
                    Status = ws.Cells[r, 4].Value?.ToString() ?? "Open",
                    Notes = ws.Cells[r, 5].Value?.ToString() ?? ""
                });
            }

            return sessions.OrderByDescending(s => s.StartDate).ToList();
        }

        public List<CycleCountItem> GetSessionItems(int sessionId)
        {
            var items = new List<CycleCountItem>();
            using var package = new ExcelPackage(new FileInfo(_cycleCountFile));
            var ws = package.Workbook.Worksheets["Items"];
            var rows = ws.Dimension?.Rows ?? 1;

            for (int r = 2; r <= rows; r++)
            {
                if (ws.Cells[r, 1].Value == null) continue;
                if (int.Parse(ws.Cells[r, 1].Value.ToString()!) != sessionId) continue;

                items.Add(new CycleCountItem
                {
                    SessionId = sessionId,
                    ProductId = int.Parse(ws.Cells[r, 2].Value?.ToString() ?? "0"),
                    ProductName = ws.Cells[r, 3].Value?.ToString() ?? "",
                    Category = ws.Cells[r, 4].Value?.ToString() ?? "",
                    ExpectedQty = int.Parse(ws.Cells[r, 5].Value?.ToString() ?? "0"),
                    CountedQty = int.TryParse(ws.Cells[r, 6].Value?.ToString(), out var cq) ? cq : -1,
                    Variance = int.TryParse(ws.Cells[r, 7].Value?.ToString(), out var v) ? v : 0,
                    UnitCost = decimal.TryParse(ws.Cells[r, 8].Value?.ToString(), out var uc) ? uc : 0,
                    VarianceValue = decimal.TryParse(ws.Cells[r, 9].Value?.ToString(), out var vv) ? vv : 0,
                    Notes = ws.Cells[r, 10].Value?.ToString() ?? "",
                    Counted = ws.Cells[r, 11].Value?.ToString()?.ToLower() == "true"
                });
            }

            return items;
        }

        /// <summary>
        /// Save the physical count for a single item within a session.
        /// </summary>
        public void SaveItemCount(int sessionId, int productId, int countedQty, string notes = "")
        {
            using var package = new ExcelPackage(new FileInfo(_cycleCountFile));
            var ws = package.Workbook.Worksheets["Items"];
            var rows = ws.Dimension?.Rows ?? 1;

            for (int r = 2; r <= rows; r++)
            {
                if (ws.Cells[r, 1].Value == null) continue;
                if (int.Parse(ws.Cells[r, 1].Value.ToString()!) != sessionId) continue;
                if (int.Parse(ws.Cells[r, 2].Value?.ToString() ?? "0") != productId) continue;

                var expected = int.Parse(ws.Cells[r, 5].Value?.ToString() ?? "0");
                var unitCost = decimal.TryParse(ws.Cells[r, 8].Value?.ToString(), out var uc) ? uc : 0;
                var variance = countedQty - expected;

                ws.Cells[r, 6].Value = countedQty;
                ws.Cells[r, 7].Value = variance;
                ws.Cells[r, 9].Value = (double)(variance * unitCost);
                ws.Cells[r, 10].Value = notes;
                ws.Cells[r, 11].Value = true;
                break;
            }

            package.Save();
        }

        /// <summary>
        /// Mark a session as completed.
        /// </summary>
        public void CompleteSession(int sessionId)
        {
            using var package = new ExcelPackage(new FileInfo(_cycleCountFile));
            var ws = package.Workbook.Worksheets["Sessions"];
            var rows = ws.Dimension?.Rows ?? 1;

            for (int r = 2; r <= rows; r++)
            {
                if (ws.Cells[r, 1].Value == null) continue;
                if (int.Parse(ws.Cells[r, 1].Value.ToString()!) != sessionId) continue;
                ws.Cells[r, 3].Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt");
                ws.Cells[r, 4].Value = "Completed";
                break;
            }

            package.Save();
        }

        private static DateTime ParseDate(object? val)
        {
            if (val is DateTime dt) return dt;
            if (val is double d) { try { return DateTime.FromOADate(d); } catch { } }
            if (DateTime.TryParse(val?.ToString(), out var parsed)) return parsed;
            return DateTime.Now;
        }
    }

    public class CycleCountSession
    {
        public int SessionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; } = "Open";
        public string Notes { get; set; } = "";
    }

    public class CycleCountItem
    {
        public int SessionId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string Category { get; set; } = "";
        public int ExpectedQty { get; set; }
        public int CountedQty { get; set; }       // -1 = not yet counted
        public int Variance { get; set; }
        public decimal UnitCost { get; set; }
        public decimal VarianceValue { get; set; }
        public string Notes { get; set; } = "";
        public bool Counted { get; set; }

        public bool HasVariance => Counted && Variance != 0;
        public string VarianceStatus => !Counted ? "Pending" : Variance == 0 ? "OK" : Variance > 0 ? "Overage" : "Shortage";
    }
}