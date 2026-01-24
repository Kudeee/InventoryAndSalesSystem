using InventoryAndSalesSystem.Models;
using InventoryAndSalesSystem.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, startDate, endDate, 
                        totalRevenue, totalCost, totalProfit, profitMargin, totalSales, 
                        profitReports, recentSales, recentLogs));
                    page.Footer().Element(ComposeFooter);
                });
            })
            .GeneratePdf(filePath);
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("INVENTORY MANAGEMENT SYSTEM")
                    .FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                
                column.Item().AlignCenter().Text("Sales & Profit Analysis Report")
                    .FontSize(16).FontColor(Colors.Grey.Darken1);
                
                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
        }

        void ComposeContent(IContainer container, DateTime startDate, DateTime endDate,
            decimal totalRevenue, decimal totalCost, decimal totalProfit, decimal profitMargin, int totalSales,
            List<ProductProfitReport> profitReports, List<Sale> recentSales, List<StockLog> recentLogs)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // Date Range
                column.Item().PaddingBottom(10).AlignCenter()
                    .Text($"Report Period: {startDate:MMM dd, yyyy} to {endDate:MMM dd, yyyy}")
                    .Italic().FontSize(12);

                // Summary Statistics
                column.Item().PaddingBottom(20).Element(c => ComposeSummaryStats(c, totalRevenue, 
                    totalCost, totalProfit, profitMargin, totalSales));

                // Product Profit Analysis
                if (profitReports.Any())
                {
                    column.Item().PaddingBottom(20).Element(c => ComposeProfitTable(c, profitReports));
                }

                // Recent Sales
                if (recentSales.Any())
                {
                    column.Item().PaddingBottom(20).Element(c => ComposeRecentSales(c, recentSales));
                }

                // Recent Stock Activity
                if (recentLogs.Any())
                {
                    column.Item().Element(c => ComposeStockActivity(c, recentLogs));
                }
            });
        }

        void ComposeSummaryStats(IContainer container, decimal totalRevenue, decimal totalCost, 
            decimal totalProfit, decimal profitMargin, int totalSales)
        {
            container.Column(column =>
            {
                column.Item().Text("Summary Statistics").FontSize(16).Bold();
                
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Background(Colors.Green.Lighten3).Padding(10)
                        .Column(col =>
                        {
                            col.Item().Text("Total Revenue").FontSize(10).Bold();
                            col.Item().Text($"₱{totalRevenue:N2}").FontSize(16).Bold();
                        });
                    
                    row.Spacing(5);
                    
                    row.RelativeItem().Background(Colors.Orange.Lighten3).Padding(10)
                        .Column(col =>
                        {
                            col.Item().Text("Total Cost").FontSize(10).Bold();
                            col.Item().Text($"₱{totalCost:N2}").FontSize(16).Bold();
                        });
                    
                    row.Spacing(5);
                    
                    row.RelativeItem().Background(totalProfit >= 0 ? Colors.Blue.Lighten3 : Colors.Red.Lighten3)
                        .Padding(10).Column(col =>
                        {
                            col.Item().Text("Net Profit").FontSize(10).Bold();
                            col.Item().Text($"₱{totalProfit:N2}").FontSize(16).Bold();
                        });
                    
                    row.Spacing(5);
                    
                    row.RelativeItem().Background(Colors.Purple.Lighten3).Padding(10)
                        .Column(col =>
                        {
                            col.Item().Text("Profit Margin").FontSize(10).Bold();
                            col.Item().Text($"{profitMargin:F2}%").FontSize(16).Bold();
                        });
                });
                
                column.Item().PaddingTop(10).Text($"Total Units Sold: {totalSales}").Bold();
            });
        }

        void ComposeProfitTable(IContainer container, List<ProductProfitReport> reports)
        {
            container.Column(column =>
            {
                column.Item().Text("Product Profit Analysis").FontSize(16).Bold();
                
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Product Name").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Units Sold").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Revenue").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Cost").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Profit").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Margin %").FontColor(Colors.White).Bold();
                    });

                    // Data rows
                    foreach (var report in reports.OrderByDescending(r => r.Profit).Take(20))
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(report.ProductName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(report.UnitsSold.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"₱{report.Revenue:N2}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"₱{report.Cost:N2}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"₱{report.Profit:N2}")
                            .FontColor(report.Profit > 0 ? Colors.Green.Darken2 : Colors.Red.Darken2).Bold();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"{report.ProfitMargin:F2}%");
                    }
                });
            });
        }

        void ComposeRecentSales(IContainer container, List<Sale> sales)
        {
            container.Column(column =>
            {
                column.Item().Text("Recent Sales Transactions").FontSize(16).Bold();
                
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Product").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Quantity").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Total").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Date").FontColor(Colors.White).Bold();
                    });

                    foreach (var sale in sales.Take(15))
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(sale.ProductName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(sale.Quantity.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"₱{sale.Total:N2}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(sale.Date.ToString("MMM dd, yyyy hh:mm tt"));
                    }
                });
            });
        }

        void ComposeStockActivity(IContainer container, List<StockLog> logs)
        {
            container.Column(column =>
            {
                column.Item().Text("Recent Stock Activity").FontSize(16).Bold();
                
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Product").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Action").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Qty").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Reason").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Medium).Padding(5)
                            .Text("Date").FontColor(Colors.White).Bold();
                    });

                    foreach (var log in logs.Take(15))
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(log.ProductName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(log.Action);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(log.Quantity.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(log.Reason ?? "-");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(log.Date.ToString("MMM dd, yyyy hh:mm tt"));
                    }
                });
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                column.Item().PaddingTop(5).AlignCenter()
                    .Text($"Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
                    .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                column.Item().AlignCenter()
                    .Text("Inventory Management System © 2026")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }
    }
}