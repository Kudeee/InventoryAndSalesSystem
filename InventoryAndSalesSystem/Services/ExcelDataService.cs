using InventoryAndSalesSystem.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryAndSalesSystem.Services
{
    public class ExcelDataService
    {
        private readonly string _dataFolder;
        private readonly string _productsFile;
        private readonly string _salesFile;
        private readonly string _stockLogsFile;

        public ExcelDataService()
        {
            _dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _productsFile = Path.Combine(_dataFolder, "Products.xlsx");
            _salesFile = Path.Combine(_dataFolder, "Sales.xlsx");
            _stockLogsFile = Path.Combine(_dataFolder, "StockLogs.xlsx");

            InitializeFiles();
        }

        private void InitializeFiles()
        {
            if (!Directory.Exists(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            InitializeProductsFile();
            InitializeSalesFile();
            InitializeStockLogsFile();
        }

        private void InitializeProductsFile()
        {
            if (File.Exists(_productsFile)) return;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Category";
            worksheet.Cells[1, 4].Value = "UnitPrice";
            worksheet.Cells[1, 5].Value = "Price";
            worksheet.Cells[1, 6].Value = "Stock";
            worksheet.Cells[1, 7].Value = "MinStock";

            package.SaveAs(new FileInfo(_productsFile));
        }

        private void InitializeSalesFile()
        {
            if (File.Exists(_salesFile)) return;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Sales");
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "ProductId";
            worksheet.Cells[1, 3].Value = "ProductName";
            worksheet.Cells[1, 4].Value = "Quantity";
            worksheet.Cells[1, 5].Value = "Price";
            worksheet.Cells[1, 6].Value = "Total";
            worksheet.Cells[1, 7].Value = "Date";

            package.SaveAs(new FileInfo(_salesFile));
        }

        private void InitializeStockLogsFile()
        {
            if (File.Exists(_stockLogsFile)) return;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("StockLogs");
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "ProductId";
            worksheet.Cells[1, 3].Value = "ProductName";
            worksheet.Cells[1, 4].Value = "Action";
            worksheet.Cells[1, 5].Value = "Quantity";
            worksheet.Cells[1, 6].Value = "StockBefore";
            worksheet.Cells[1, 7].Value = "StockAfter";
            worksheet.Cells[1, 8].Value = "Reason";
            worksheet.Cells[1, 9].Value = "Date";

            package.SaveAs(new FileInfo(_stockLogsFile));
        }

        // Products Operations
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using var package = new ExcelPackage(new FileInfo(_productsFile));
            var worksheet = package.Workbook.Worksheets["Products"];

            var rowCount = worksheet.Dimension?.Rows ?? 1;
            for (int row = 2; row <= rowCount; row++)
            {
                if (worksheet.Cells[row, 1].Value == null) continue;

                products.Add(new Product
                {
                    Id = int.Parse(worksheet.Cells[row, 1].Value.ToString()!),
                    Name = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                    Category = worksheet.Cells[row, 3].Value?.ToString() ?? "",
                    UnitCost = decimal.Parse(worksheet.Cells[row, 4].Value?.ToString() ?? "0"), 
                    Price = decimal.Parse(worksheet.Cells[row, 5].Value?.ToString() ?? "0"),
                    Stock = int.Parse(worksheet.Cells[row, 6].Value?.ToString() ?? "0"),
                    MinStock = int.Parse(worksheet.Cells[row, 7].Value?.ToString() ?? "0")
                });
            }

            return products;
        }

        public void SaveProduct(Product product)
        {
            using var package = new ExcelPackage(new FileInfo(_productsFile));
            var worksheet = package.Workbook.Worksheets["Products"];

            var rowCount = worksheet.Dimension?.Rows ?? 1;
            var existingRow = -1;

            for (int row = 2; row <= rowCount; row++)
            {
                if (worksheet.Cells[row, 1].Value?.ToString() == product.Id.ToString())
                {
                    existingRow = row;
                    break;
                }
            }

            if (existingRow == -1)
            {
                existingRow = rowCount + 1;
                if (product.Id == 0)
                    product.Id = GetNextProductId();
            }

            worksheet.Cells[existingRow, 1].Value = product.Id;
            worksheet.Cells[existingRow, 2].Value = product.Name;
            worksheet.Cells[existingRow, 3].Value = product.Category;
            worksheet.Cells[existingRow, 4].Value = product.UnitCost;
            worksheet.Cells[existingRow, 5].Value = product.Price;
            worksheet.Cells[existingRow, 6].Value = product.Stock;
            worksheet.Cells[existingRow, 7].Value = product.MinStock;

            package.Save();
        }

        public void DeleteProduct(int productId)
        {
            using var package = new ExcelPackage(new FileInfo(_productsFile));
            var worksheet = package.Workbook.Worksheets["Products"];
            var rowCount = worksheet.Dimension?.Rows ?? 1;

            for (int row = 2; row <= rowCount; row++)
            {
                if (worksheet.Cells[row, 1].Value?.ToString() == productId.ToString())
                {
                    worksheet.DeleteRow(row);
                    break;
                }
            }

            package.Save();
        }

        private int GetNextProductId()
        {
            var products = GetAllProducts();
            return products.Any() ? products.Max(p => p.Id) + 1 : 1;
        }

        // Sales Operations
        public List<Sale> GetAllSales()
        {
            var sales = new List<Sale>();
            using var package = new ExcelPackage(new FileInfo(_salesFile));
            var worksheet = package.Workbook.Worksheets["Sales"];

            var rowCount = worksheet.Dimension?.Rows ?? 1;
            for (int row = 2; row <= rowCount; row++)
            {
                if (worksheet.Cells[row, 1].Value == null) continue;

                sales.Add(new Sale
                {
                    Id = int.Parse(worksheet.Cells[row, 1].Value.ToString()!),
                    ProductId = int.Parse(worksheet.Cells[row, 2].Value?.ToString() ?? "0"),
                    ProductName = worksheet.Cells[row, 3].Value?.ToString() ?? "",
                    Quantity = int.Parse(worksheet.Cells[row, 4].Value?.ToString() ?? "0"),
                    Price = decimal.Parse(worksheet.Cells[row, 5].Value?.ToString() ?? "0"),
                    Date = DateTime.Parse(worksheet.Cells[row, 7].Value?.ToString() ?? DateTime.Now.ToString())
                });
            }

            return sales;
        }

        public void SaveSale(Sale sale)
        {
            using var package = new ExcelPackage(new FileInfo(_salesFile));
            var worksheet = package.Workbook.Worksheets["Sales"];
            var rowCount = worksheet.Dimension?.Rows ?? 1;
            var newRow = rowCount + 1;

            sale.Id = GetNextSaleId();

            worksheet.Cells[newRow, 1].Value = sale.Id;
            worksheet.Cells[newRow, 2].Value = sale.ProductId;
            worksheet.Cells[newRow, 3].Value = sale.ProductName;
            worksheet.Cells[newRow, 4].Value = sale.Quantity;
            worksheet.Cells[newRow, 5].Value = sale.Price;
            worksheet.Cells[newRow, 6].Value = sale.Total;
            worksheet.Cells[newRow, 7].Value = sale.Date.ToString("yyyy-MM-dd hh:mm:ss");

            package.Save();
        }

        private int GetNextSaleId()
        {
            var sales = GetAllSales();
            return sales.Any() ? sales.Max(s => s.Id) + 1 : 1;
        }

        // Stock Logs Operations
        public List<StockLog> GetAllStockLogs()
        {
            var logs = new List<StockLog>();
            using var package = new ExcelPackage(new FileInfo(_stockLogsFile));
            var worksheet = package.Workbook.Worksheets["StockLogs"];

            var rowCount = worksheet.Dimension?.Rows ?? 1;
            for (int row = 2; row <= rowCount; row++)
            {
                if (worksheet.Cells[row, 1].Value == null) continue;

                logs.Add(new StockLog
                {
                    Id = int.Parse(worksheet.Cells[row, 1].Value.ToString()!),
                    ProductId = int.Parse(worksheet.Cells[row, 2].Value?.ToString() ?? "0"),
                    ProductName = worksheet.Cells[row, 3].Value?.ToString() ?? "",
                    Action = worksheet.Cells[row, 4].Value?.ToString() ?? "",
                    Quantity = int.Parse(worksheet.Cells[row, 5].Value?.ToString() ?? "0"),
                    StockBefore = int.Parse(worksheet.Cells[row, 6].Value?.ToString() ?? "0"),
                    StockAfter = int.Parse(worksheet.Cells[row, 7].Value?.ToString() ?? "0"),
                    Date = DateTime.Parse(worksheet.Cells[row, 8].Value?.ToString() ?? DateTime.Now.ToString())
                });
            }

            return logs;
        }

        public void SaveStockLog(StockLog log)
        {
            using var package = new ExcelPackage(new FileInfo(_stockLogsFile));
            var worksheet = package.Workbook.Worksheets["StockLogs"];
            var rowCount = worksheet.Dimension?.Rows ?? 1;
            var newRow = rowCount + 1;

            log.Id = GetNextStockLogId();

            worksheet.Cells[newRow, 1].Value = log.Id;
            worksheet.Cells[newRow, 2].Value = log.ProductId;
            worksheet.Cells[newRow, 3].Value = log.ProductName;
            worksheet.Cells[newRow, 4].Value = log.Action;
            worksheet.Cells[newRow, 5].Value = log.Quantity;
            worksheet.Cells[newRow, 6].Value = log.StockBefore;
            worksheet.Cells[newRow, 7].Value = log.StockAfter;
            worksheet.Cells[newRow, 8].Value = log.Reason;
            worksheet.Cells[newRow, 9].Value = log.Date.ToString("yyyy-MM-dd hh:mm:ss");

            package.Save();
        }

        private int GetNextStockLogId()
        {
            var logs = GetAllStockLogs();
            return logs.Any() ? logs.Max(l => l.Id) + 1 : 1;
        }
    }
}
