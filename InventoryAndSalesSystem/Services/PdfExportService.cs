using InventoryAndSalesSystem.Models;
using InventoryAndSalesSystem.ViewModels;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InventoryAndSalesSystem.Services
{
    public class PdfExportService
    {
        public void ExportReportToPdf(
            string filePath,
            DateTime startDate,
            DateTime endDate,
            decimal totalRevenue,
            decimal totalCost,
            decimal totalProfit,
            decimal profitMargin,
            int totalSales,
            List<ProductProfitReport> profitReports,
            List<Sale> recentSales,
            List<StockLog> recentLogs)
        {
            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf))
            {
                // Header
                AddHeader(document);

                // Date Range
                AddDateRange(document, startDate, endDate);

                // Summary Statistics
                AddSummaryStatistics(document, totalRevenue, totalCost, totalProfit, profitMargin, totalSales);

                // Profit Analysis Table
                AddProfitAnalysisTable(document, profitReports);

                // Recent Sales
                AddRecentSalesTable(document, recentSales);

                // Recent Stock Activity
                AddRecentStockActivityTable(document, recentLogs);

                // Footer
                AddFooter(document);
            }
        }

        private void AddHeader(Document document)
        {
            var header = new Paragraph("INVENTORY MANAGEMENT SYSTEM")
                .SetFontSize(24)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(5);
            document.Add(header);

            var subheader = new Paragraph("Sales & Profit Analysis Report")
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(subheader);
        }

        private void AddDateRange(Document document, DateTime startDate, DateTime endDate)
        {
            var dateRange = new Paragraph($"Report Period: {startDate:MMM dd, yyyy} to {endDate:MMM dd, yyyy}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20)
                .SetItalic();
            document.Add(dateRange);
        }

        private void AddSummaryStatistics(Document document, decimal totalRevenue, decimal totalCost, 
            decimal totalProfit, decimal profitMargin, int totalSales)
        {
            var title = new Paragraph("Summary Statistics")
                .SetFontSize(18)
                .SetBold()
                .SetMarginBottom(10);
            document.Add(title);

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1, 1, 1 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(20);

            // Headers
            AddTableHeader(table, "Total Revenue");
            AddTableHeader(table, "Total Cost");
            AddTableHeader(table, "Net Profit");
            AddTableHeader(table, "Profit Margin");

            // Values
            AddTableCell(table, $"₱{totalRevenue:N2}", new DeviceRgb(76, 175, 80)); // Green
            AddTableCell(table, $"₱{totalCost:N2}", new DeviceRgb(255, 152, 0)); // Orange
            AddTableCell(table, $"₱{totalProfit:N2}", totalProfit >= 0 ? new DeviceRgb(33, 150, 243) : new DeviceRgb(244, 67, 54)); // Blue or Red
            AddTableCell(table, $"{profitMargin:F2}%", new DeviceRgb(156, 39, 176)); // Purple

            document.Add(table);

            var totalSalesInfo = new Paragraph($"Total Units Sold: {totalSales}")
                .SetFontSize(12)
                .SetBold()
                .SetMarginBottom(20);
            document.Add(totalSalesInfo);
        }

        private void AddProfitAnalysisTable(Document document, List<ProductProfitReport> profitReports)
        {
            var title = new Paragraph("Product Profit Analysis")
                .SetFontSize(18)
                .SetBold()
                .SetMarginBottom(10);
            document.Add(title);

            if (!profitReports.Any())
            {
                document.Add(new Paragraph("No data available for the selected period.")
                    .SetItalic()
                    .SetMarginBottom(20));
                return;
            }

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1.5f, 2, 2, 2, 1.5f }))
                .UseAllAvailableWidth()
                .SetMarginBottom(20);

            // Headers
            AddTableHeader(table, "Product Name");
            AddTableHeader(table, "Units Sold");
            AddTableHeader(table, "Revenue");
            AddTableHeader(table, "Cost");
            AddTableHeader(table, "Profit");
            AddTableHeader(table, "Margin %");

            // Data rows
            foreach (var report in profitReports.OrderByDescending(r => r.Profit).Take(20))
            {
                table.AddCell(CreateCell(report.ProductName));
                table.AddCell(CreateCell(report.UnitsSold.ToString()));
                table.AddCell(CreateCell($"₱{report.Revenue:N2}"));
                table.AddCell(CreateCell($"₱{report.Cost:N2}"));
                
                var profitCell = CreateCell($"₱{report.Profit:N2}");
                if (report.Profit > 0)
                    profitCell.SetFontColor(new DeviceRgb(76, 175, 80)).SetBold(); // Green
                else if (report.Profit < 0)
                    profitCell.SetFontColor(new DeviceRgb(244, 67, 54)).SetBold(); // Red
                table.AddCell(profitCell);
                
                table.AddCell(CreateCell($"{report.ProfitMargin:F2}%"));
            }

            document.Add(table);
        }

        private void AddRecentSalesTable(Document document, List<Sale> recentSales)
        {
            var title = new Paragraph("Recent Sales Transactions")
                .SetFontSize(18)
                .SetBold()
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(title);

            if (!recentSales.Any())
            {
                document.Add(new Paragraph("No sales data available.")
                    .SetItalic()
                    .SetMarginBottom(20));
                return;
            }

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 1.5f, 2, 2 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(20);

            // Headers
            AddTableHeader(table, "Product");
            AddTableHeader(table, "Quantity");
            AddTableHeader(table, "Total");
            AddTableHeader(table, "Date");

            // Data rows
            foreach (var sale in recentSales.Take(15))
            {
                table.AddCell(CreateCell(sale.ProductName));
                table.AddCell(CreateCell(sale.Quantity.ToString()));
                table.AddCell(CreateCell($"₱{sale.Total:N2}"));
                table.AddCell(CreateCell(sale.Date.ToString("MMM dd, yyyy hh:mm tt")));
            }

            document.Add(table);
        }

        private void AddRecentStockActivityTable(Document document, List<StockLog> recentLogs)
        {
            var title = new Paragraph("Recent Stock Activity")
                .SetFontSize(18)
                .SetBold()
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(title);

            if (!recentLogs.Any())
            {
                document.Add(new Paragraph("No stock activity data available.")
                    .SetItalic()
                    .SetMarginBottom(20));
                return;
            }

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 2.5f, 1.5f, 1, 2, 2 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(20);

            // Headers
            AddTableHeader(table, "Product");
            AddTableHeader(table, "Action");
            AddTableHeader(table, "Qty");
            AddTableHeader(table, "Reason");
            AddTableHeader(table, "Date");

            // Data rows
            foreach (var log in recentLogs.Take(15))
            {
                table.AddCell(CreateCell(log.ProductName));
                table.AddCell(CreateCell(log.Action));
                table.AddCell(CreateCell(log.Quantity.ToString()));
                table.AddCell(CreateCell(log.Reason ?? "-"));
                table.AddCell(CreateCell(log.Date.ToString("MMM dd, yyyy hh:mm tt")));
            }

            document.Add(table);
        }

        private void AddFooter(Document document)
        {
            var footer = new Paragraph($"Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(30)
                .SetItalic()
                .SetFontColor(ColorConstants.GRAY);
            document.Add(footer);

            var appFooter = new Paragraph("Inventory Management System © 2026")
                .SetFontSize(9)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.GRAY);
            document.Add(appFooter);
        }

        private void AddTableHeader(Table table, string text)
        {
            var cell = new Cell()
                .Add(new Paragraph(text).SetBold().SetFontSize(11))
                .SetBackgroundColor(new DeviceRgb(33, 150, 243))
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8)
                .SetBorder(new SolidBorder(ColorConstants.WHITE, 1));
            table.AddHeaderCell(cell);
        }

        private void AddTableCell(Table table, string text, DeviceRgb color)
        {
            var cell = new Cell()
                .Add(new Paragraph(text).SetBold().SetFontSize(13))
                .SetBackgroundColor(new DeviceRgb(color.GetColorValue()[0] * 0.2f, 
                                                  color.GetColorValue()[1] * 0.2f, 
                                                  color.GetColorValue()[2] * 0.2f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(10)
                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1));
            table.AddCell(cell);
        }

        private Cell CreateCell(string text)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFontSize(10))
                .SetPadding(5)
                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.5f));
        }
    }
}