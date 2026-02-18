using InventoryAndSalesSystem.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InventoryAndSalesSystem.Services
{
    /// <summary>
    /// Records every user action (create, edit, delete, sell, restock, etc.) 
    /// to an AuditTrail.xlsx file for full accountability.
    /// </summary>
    public class AuditTrailService
    {
        private readonly string _auditFile;

        public AuditTrailService(string dataFolder)
        {
            _auditFile = Path.Combine(dataFolder, "AuditTrail.xlsx");
            InitializeFile();
        }

        private void InitializeFile()
        {
            if (File.Exists(_auditFile)) return;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("AuditTrail");
            ws.Cells[1, 1].Value = "ID";
            ws.Cells[1, 2].Value = "Timestamp";
            ws.Cells[1, 3].Value = "Action";
            ws.Cells[1, 4].Value = "Entity";
            ws.Cells[1, 5].Value = "EntityId";
            ws.Cells[1, 6].Value = "EntityName";
            ws.Cells[1, 7].Value = "Details";
            ws.Cells[1, 8].Value = "OldValue";
            ws.Cells[1, 9].Value = "NewValue";
            package.SaveAs(new FileInfo(_auditFile));
        }

        /// <summary>
        /// Logs a generic audit entry.
        /// </summary>
        public void Log(string action, string entity, int entityId, string entityName,
                        string details = "", string oldValue = "", string newValue = "")
        {
            using var package = new ExcelPackage(new FileInfo(_auditFile));
            var ws = package.Workbook.Worksheets["AuditTrail"];
            var rowCount = ws.Dimension?.Rows ?? 1;
            var newRow = rowCount + 1;
            var nextId = rowCount; // header = row 1, so data rows = rowCount

            ws.Cells[newRow, 1].Value = nextId;
            ws.Cells[newRow, 2].Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt");
            ws.Cells[newRow, 3].Value = action;
            ws.Cells[newRow, 4].Value = entity;
            ws.Cells[newRow, 5].Value = entityId;
            ws.Cells[newRow, 6].Value = entityName;
            ws.Cells[newRow, 7].Value = details;
            ws.Cells[newRow, 8].Value = oldValue;
            ws.Cells[newRow, 9].Value = newValue;
            package.Save();
        }

        // Convenience helpers ------------------------------------------------

        public void LogProductAdded(Product p) =>
            Log("Product Added", "Product", p.Id, p.Name,
                $"Category: {p.Category}, Price: {p.Price:C}, UnitCost: {p.UnitCost:C}, Stock: {p.Stock}, MinStock: {p.MinStock}");

        public void LogProductEdited(Product before, Product after) =>
            Log("Product Edited", "Product", after.Id, after.Name,
                BuildDiff(before, after),
                $"Name:{before.Name}|Cat:{before.Category}|Price:{before.Price}|Cost:{before.UnitCost}|Stock:{before.Stock}|MinStock:{before.MinStock}",
                $"Name:{after.Name}|Cat:{after.Category}|Price:{after.Price}|Cost:{after.UnitCost}|Stock:{after.Stock}|MinStock:{after.MinStock}");

        public void LogProductDeleted(Product p) =>
            Log("Product Deleted", "Product", p.Id, p.Name,
                $"Category: {p.Category}, Last Stock: {p.Stock}");

        public void LogSale(Sale s) =>
            Log("Sale", "Sale", s.Id, s.ProductName,
                $"Qty: {s.Quantity}, Price: {s.Price:C}, Total: {s.Total:C}");

        public void LogRestock(Product p, int qty) =>
            Log("Restock", "Product", p.Id, p.Name,
                $"Added: {qty}", $"Stock Before: {p.Stock - qty}", $"Stock After: {p.Stock}");

        public void LogSalesReturn(Product p, int qty, string reason) =>
            Log("Sales Return", "Product", p.Id, p.Name,
                $"Qty: {qty}, Reason: {reason}", $"Stock Before: {p.Stock - qty}", $"Stock After: {p.Stock}");

        public void LogPurchaseReturn(Product p, int qty, string reason) =>
            Log("Purchase Return", "Product", p.Id, p.Name,
                $"Qty: {qty}, Reason: {reason}", $"Stock Before: {p.Stock + qty}", $"Stock After: {p.Stock}");

        public void LogProductLoss(Product p, int qty, string reason) =>
            Log("Product Loss", "Product", p.Id, p.Name,
                $"Qty: {qty}, Reason: {reason}", $"Stock Before: {p.Stock + qty}", $"Stock After: {p.Stock}");

        public void LogManualBackup() =>
            Log("Manual Backup", "System", 0, "Backup", "User triggered manual backup");

        public void LogAutoBackup(string filePath) =>
            Log("Auto Backup", "System", 0, "Backup", $"Scheduled backup created: {Path.GetFileName(filePath)}");

        // Read all audit entries
        public List<AuditEntry> GetAllEntries()
        {
            var entries = new List<AuditEntry>();
            using var package = new ExcelPackage(new FileInfo(_auditFile));
            var ws = package.Workbook.Worksheets["AuditTrail"];
            var rowCount = ws.Dimension?.Rows ?? 1;

            for (int row = 2; row <= rowCount; row++)
            {
                if (ws.Cells[row, 1].Value == null) continue;
                entries.Add(new AuditEntry
                {
                    Id = int.Parse(ws.Cells[row, 1].Value.ToString()!),
                    Timestamp = ParseDate(ws.Cells[row, 2].Value),
                    Action = ws.Cells[row, 3].Value?.ToString() ?? "",
                    Entity = ws.Cells[row, 4].Value?.ToString() ?? "",
                    EntityId = int.TryParse(ws.Cells[row, 5].Value?.ToString(), out var eid) ? eid : 0,
                    EntityName = ws.Cells[row, 6].Value?.ToString() ?? "",
                    Details = ws.Cells[row, 7].Value?.ToString() ?? "",
                    OldValue = ws.Cells[row, 8].Value?.ToString() ?? "",
                    NewValue = ws.Cells[row, 9].Value?.ToString() ?? ""
                });
            }

            return entries.OrderByDescending(e => e.Timestamp).ToList();
        }

        private static DateTime ParseDate(object? val)
        {
            if (val is DateTime dt) return dt;
            if (val is double d) { try { return DateTime.FromOADate(d); } catch { } }
            if (DateTime.TryParse(val?.ToString(), out var parsed)) return parsed;
            return DateTime.Now;
        }

        private static string BuildDiff(Product before, Product after)
        {
            var changes = new List<string>();
            if (before.Name != after.Name) changes.Add($"Name: {before.Name}→{after.Name}");
            if (before.Category != after.Category) changes.Add($"Category: {before.Category}→{after.Category}");
            if (before.Price != after.Price) changes.Add($"Price: {before.Price:C}→{after.Price:C}");
            if (before.UnitCost != after.UnitCost) changes.Add($"UnitCost: {before.UnitCost:C}→{after.UnitCost:C}");
            if (before.Stock != after.Stock) changes.Add($"Stock: {before.Stock}→{after.Stock}");
            if (before.MinStock != after.MinStock) changes.Add($"MinStock: {before.MinStock}→{after.MinStock}");
            return changes.Count > 0 ? string.Join(", ", changes) : "No changes";
        }
    }

    public class AuditEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = "";
        public string Entity { get; set; } = "";
        public int EntityId { get; set; }
        public string EntityName { get; set; } = "";
        public string Details { get; set; } = "";
        public string OldValue { get; set; } = "";
        public string NewValue { get; set; } = "";
    }
}