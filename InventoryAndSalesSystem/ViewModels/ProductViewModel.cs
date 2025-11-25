using InventoryAndSalesSystem.Helpers;
using InventoryAndSalesSystem.Models;
using InventoryAndSalesSystem.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InventoryAndSalesSystem.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private readonly ExcelDataService _dataService;
        private ObservableCollection<Product> _products;
        private Product? _selectedProduct;
        private string _name = string.Empty;
        private string _category = string.Empty;
        private decimal _price;
        private decimal _unitCost;
        private int _stock;
        private int _minStock = 10;
        private int _sellQuantity = 1;
        private int _restockQuantity = 1;

        public ProductViewModel(ExcelDataService dataService)
        {
            _dataService = dataService;
            _products = new ObservableCollection<Product>();

            SaveCommand = new RelayCommand(_ => SaveProduct());
            DeleteCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);
            SellCommand = new RelayCommand(_ => SellProduct(), _ => SelectedProduct != null && SellQuantity > 0);
            RestockCommand = new RelayCommand(_ => RestockProduct(), _ => SelectedProduct != null && RestockQuantity > 0);
            LoadCommand = new RelayCommand(_ => LoadProducts());
            ClearCommand = new RelayCommand(_ => ClearForm());

            LoadProducts();
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                if (value != null)
                {
                    Name = value.Name;
                    Category = value.Category;
                    Price = value.Price;
                    UnitCost = value.UnitCost;
                    Stock = value.Stock;
                    MinStock = value.MinStock;
                }
            }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public decimal UnitCost
        {
            get => _unitCost;
            set { _unitCost = value; OnPropertyChanged(); }
        }

        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); }
        }

        public int MinStock
        {
            get => _minStock;
            set { _minStock = value; OnPropertyChanged(); }
        }

        public int SellQuantity
        {
            get => _sellQuantity;
            set { _sellQuantity = value; OnPropertyChanged(); }
        }

        public int RestockQuantity
        {
            get => _restockQuantity;
            set { _restockQuantity = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SellCommand { get; }
        public ICommand RestockCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand ClearCommand { get; }

        private void LoadProducts()
        {
            Products.Clear();
            var products = _dataService.GetAllProducts();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            CheckLowStock();
        }

        private void SaveProduct()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please enter a product name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var product = new Product
            {
                Id = SelectedProduct?.Id ?? 0,
                Name = Name,
                Category = Category,
                Price = Price,
                UnitCost = UnitCost,
                Stock = Stock,
                MinStock = MinStock
            };

            _dataService.SaveProduct(product);
            LoadProducts();
            ClearForm();
            MessageBox.Show("Product saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show($"Delete '{SelectedProduct.Name}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _dataService.DeleteProduct(SelectedProduct.Id);
                LoadProducts();
                ClearForm();
                MessageBox.Show("Product deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SellProduct()
        {
            if (SelectedProduct == null) return;

            if (SellQuantity > SelectedProduct.Stock)
            {
                MessageBox.Show("Insufficient stock!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sale = new Sale
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Quantity = SellQuantity,
                Price = SelectedProduct.Price,
                Date = DateTime.Now
            };

            var stockLog = new StockLog
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Action = "Sale",
                Quantity = SellQuantity,
                StockBefore = SelectedProduct.Stock,
                StockAfter = SelectedProduct.Stock - SellQuantity,
                Date = DateTime.Now
            };

            SelectedProduct.Stock -= SellQuantity;
            _dataService.SaveProduct(SelectedProduct);
            _dataService.SaveSale(sale);
            _dataService.SaveStockLog(stockLog);

            LoadProducts();
            MessageBox.Show($"Sold {SellQuantity} units!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RestockProduct()
        {
            if (SelectedProduct == null) return;

            var stockLog = new StockLog
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Action = "Restock",
                Quantity = RestockQuantity,
                StockBefore = SelectedProduct.Stock,
                StockAfter = SelectedProduct.Stock + RestockQuantity,
                Date = DateTime.Now
            };

            SelectedProduct.Stock += RestockQuantity;
            _dataService.SaveProduct(SelectedProduct);
            _dataService.SaveStockLog(stockLog);

            LoadProducts();
            MessageBox.Show($"Added {RestockQuantity} units!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Category = string.Empty;
            Price = 0;
            UnitCost = 0; 
            Stock = 0;
            MinStock = 10;
            SelectedProduct = null;
        }

        private void CheckLowStock()
        {
            var lowStock = Products.Where(p => p.IsLowStock).ToList();
            if (lowStock.Any())
            {
                var message = "⚠️ Low Stock Alert!\n\n";
                foreach (var product in lowStock)
                {
                    message += $"• {product.Name}: {product.Stock} units (Min: {product.MinStock})\n";
                }
                MessageBox.Show(message, "Low Stock Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
