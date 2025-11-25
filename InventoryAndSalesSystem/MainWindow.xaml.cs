using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventoryAndSalesSystem
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Product> Products { get; set; }
        private readonly string excelFilePath = "inventory.xlsx";
        private const int LOW_STOCK_THRESHOLD = 10;

        public MainWindow()
        {
            InitializeComponent();
            Products = new ObservableCollection<Product>();
            LoadDataFromExcel();
            ProductsDataGrid.ItemsSource = Products;
            CheckLowStock();
        }

        private void LoadDataFromExcel()
        {
            if (!File.Exists(excelFilePath))
            {
                CreateExcelFile();
                return;
            }

            try
            {
                using (var workbook = new XLWorkbook(excelFilePath))
                {
                    var worksheet = workbook.Worksheet("Products");
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        Products.Add(new Product
                        {
                            Id = row.Cell(1).GetValue<int>(),
                            Name = row.Cell(2).GetValue<string>(),
                            Category = row.Cell(3).GetValue<string>(),
                            Price = row.Cell(4).GetValue<decimal>(),
                            Stock = row.Cell(5).GetValue<int>(),
                            SoldUnits = row.Cell(6).GetValue<int>(),
                        });
                    }
                }

            }
            catch (Exception ex) {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateExcelFile()
        {
            using(var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Category";
                worksheet.Cell(1, 4).Value = "Price";
                worksheet.Cell(1, 5).Value = "Stock";
                worksheet.Cell(1, 6).Value = "Sold Units";
                workbook.SaveAs(excelFilePath);
            }
        }

        private void SaveDataToExcel()
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Products");
                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Name";
                    worksheet.Cell(1, 3).Value = "Category";
                    worksheet.Cell(1, 4).Value = "Price";
                    worksheet.Cell(1, 5).Value = "Stock";
                    worksheet.Cell(1, 6).Value = "Sold Units";

                    int row = 2;

                    foreach (var product in Products)
                    {
                        worksheet.Cell(row, 1).Value = product.Id;
                        worksheet.Cell(row, 2).Value = product.Name;
                        worksheet.Cell(row, 3).Value = product.Category;
                        worksheet.Cell(row, 4).Value = product.Price;
                        worksheet.Cell(row, 5).Value = product.Stock;
                        worksheet.Cell(row, 6).Value = product.SoldUnits;
                        row++;
                    }
                    workbook.SaveAs(excelFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductDialog();
            if(dialog.ShowDialog() == true)
            {
                var newProduct = dialog.Product;
                newProduct.Id = Products.Count > 0 ? Products.Max(p => p.Id) + 1 : 1;
                Products.Add(newProduct);
                SaveDataToExcel();
                CheckLowStock();
                MessageBox.Show("Product has been added sucesfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                var dialog = new ProductDialog(selectedProduct);
                if (dialog.ShowDialog() == true)
                {
                    var index = Products.IndexOf(selectedProduct);
                    Products[index] = dialog.Product;
                    SaveDataToExcel();
                    ProductsDataGrid.Items.Refresh();
                    CheckLowStock();
                    MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{selectedProduct.Name}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Products.Remove(selectedProduct);
                    SaveDataToExcel();
                    CheckLowStock();
                    MessageBox.Show("Product deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CheckLowStock()
        {
            var lowStockProducts = Products.Where(p => p.Stock < LOW_STOCK_THRESHOLD).ToList();

            if (lowStockProducts.Any())
            {
                string message = "Low Stock Alert!\n\nThe following products are running low:\n\n";
                foreach (var product in lowStockProducts)
                {
                    message += $"• {product.Name}: {product.Stock} units left\n";
                }
                MessageBox.Show(message, "Low Stock Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewReport_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new SalesReportWindow(Products.ToList());
            reportWindow.ShowDialog();
        }
    }
}