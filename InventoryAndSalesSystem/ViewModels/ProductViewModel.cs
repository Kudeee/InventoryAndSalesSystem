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
        private ObservableCollection<Product> _filteredProducts;
        private Product? _selectedProduct;
        private string _name = string.Empty;
        private string _category = string.Empty;
        private decimal _price;
        private decimal _unitCost;
        private int _stock;
        private int _minStock = 10;
        private int _sellQuantity = 1;
        private int _restockQuantity = 1;
        private int _returnQuantity = 1;
        private int _lossQuantity = 1;
        private string _returnReason = string.Empty;
        private string _lossReason = string.Empty;
        private string _searchText = string.Empty;
        private string _selectedCategoryFilter = "All Categories";
        private List<Product> _allProducts;

        public ProductViewModel(ExcelDataService dataService)
        {
            _dataService = dataService;
            _products = new ObservableCollection<Product>();
            _filteredProducts = new ObservableCollection<Product>();
            _allProducts = new List<Product>();

            // Initialize all commands
            SaveCommand = new RelayCommand(_ => SaveProduct());
            DeleteCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);
            SellCommand = new RelayCommand(_ => SellProduct(), _ => SelectedProduct != null && SellQuantity > 0);
            RestockCommand = new RelayCommand(_ => RestockProduct(), _ => SelectedProduct != null && RestockQuantity > 0);
            LoadCommand = new RelayCommand(_ => LoadProducts());
            ClearCommand = new RelayCommand(_ => ClearForm());
            SalesReturnCommand = new RelayCommand(_ => ProcessSalesReturn(), _ => SelectedProduct != null && ReturnQuantity > 0);
            PurchaseReturnCommand = new RelayCommand(_ => ProcessPurchaseReturn(), _ => SelectedProduct != null && ReturnQuantity > 0);
            ProductLossCommand = new RelayCommand(_ => ProcessProductLoss(), _ => SelectedProduct != null && LossQuantity > 0);
            SearchCommand = new RelayCommand(_ => ApplyFilters());
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());

            LoadProducts();
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set { _filteredProducts = value; OnPropertyChanged(); }
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

        public int ReturnQuantity
        {
            get => _returnQuantity;
            set {_returnQuantity = value; OnPropertyChanged(); }
        }

        public int LossQuantity
        {
            get => _lossQuantity;
            set {_lossQuantity = value; OnPropertyChanged(); }
        }

        public string ReturnReason
        {
            get => _returnReason;
            set { _returnReason = value; OnPropertyChanged(); }
        }

        public string LossReason
        {
            get => _lossReason;
            set { _lossReason = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set 
            { 
                _searchText = value; 
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public string SelectedCategoryFilter
        {
            get => _selectedCategoryFilter;
            set 
            { 
                _selectedCategoryFilter = value; 
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public List<string> Categories { get; } = new List<string>
        {
            "All Categories",
            "Garments",
            "Snacks",
            "Biscuits",
            "Beverages",
            "Accessories",
            "Souvenir Bag",
            "Stuff Toy"
        };

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SellCommand { get; }
        public ICommand RestockCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand SalesReturnCommand { get; }
        public ICommand PurchaseReturnCommand { get; }
        public ICommand ProductLossCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        private void LoadProducts()
        {
            Products.Clear();
            _allProducts = _dataService.GetAllProducts();
            foreach (var product in _allProducts)
            {
                Products.Add(product);
            }
            ApplyFilters();
            CheckLowStock();
        }

        private void ApplyFilters()
        {
            var filtered = _allProducts.AsEnumerable();

            // Apply category filter
            if (!string.IsNullOrEmpty(SelectedCategoryFilter) && SelectedCategoryFilter != "All Categories")
            {
                filtered = filtered.Where(p => 
                    p.Category.Equals(SelectedCategoryFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(p => 
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Id.ToString().Contains(SearchText));
            }

            FilteredProducts.Clear();
            foreach (var product in filtered)
            {
                FilteredProducts.Add(product);
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            SelectedCategoryFilter = "All Categories";
            ApplyFilters();
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
            
            var stockLog = new StockLog
            {
                ProductId = SelectedProduct?.Id ?? 0,
                ProductName = Name,
                Action = "New Product",
                Quantity = Stock,
                StockBefore = 0,
                StockAfter = 0,
                Date = DateTime.Now
            };

            _dataService.SaveProduct(product);
            _dataService.SaveStockLog(stockLog);
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
            SellQuantity = 1;
            MessageBox.Show($"Sold {sale.Quantity} units!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            RestockQuantity = 1;
            MessageBox.Show($"Added {stockLog.Quantity} units!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void ProcessSalesReturn()
        {
            if (SelectedProduct == null) return;

            if (string.IsNullOrWhiteSpace(ReturnReason))
            {
                MessageBox.Show("Please enter a reason for the return.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var stockLog = new StockLog
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Action = "Sales Return",
                Quantity = ReturnQuantity,
                StockBefore = SelectedProduct.Stock,
                StockAfter = SelectedProduct.Stock + ReturnQuantity,
                Reason = ReturnReason,
                Date = DateTime.Now
            };

            SelectedProduct.Stock += ReturnQuantity;
            _dataService.SaveProduct(SelectedProduct);
            _dataService.SaveStockLog(stockLog);
            
            LoadProducts();
            ReturnReason = string.Empty;
            ReturnQuantity = 1;
            MessageBox.Show($"Sales return processed: {stockLog.Quantity} units returned to stock!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ProcessPurchaseReturn()
        {
            if (SelectedProduct == null) return;

            if (string.IsNullOrWhiteSpace(ReturnReason))
            {
                MessageBox.Show("Please enter a reason for the return.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ReturnQuantity > SelectedProduct.Stock)
            {
                MessageBox.Show("Return quantity cannot exceed current stock!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var stockLog = new StockLog
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Action = "Purchase Return",
                Quantity = ReturnQuantity,
                StockBefore = SelectedProduct.Stock,
                StockAfter = SelectedProduct.Stock - ReturnQuantity,
                Reason = ReturnReason,
                Date = DateTime.Now
            };

            SelectedProduct.Stock -= ReturnQuantity;
            _dataService.SaveProduct(SelectedProduct);
            _dataService.SaveStockLog(stockLog);
            
            LoadProducts();
            ReturnReason = string.Empty;
            ReturnQuantity = 1;
            MessageBox.Show($"Purchase return processed: {stockLog.Quantity} units returned to supplier!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ProcessProductLoss()
        {
            if (SelectedProduct == null) return;

            if (string.IsNullOrWhiteSpace(LossReason))
            {
                MessageBox.Show("Please enter a reason for the loss (e.g., 'Expired', 'Damaged', 'Broken').", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (LossQuantity > SelectedProduct.Stock)
            {
                MessageBox.Show("Loss quantity cannot exceed current stock!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Confirm product loss:\n\nProduct: {SelectedProduct.Name}\nQuantity: {LossQuantity}\nReason: {LossReason}\n\nThis action cannot be undone.",
                "Confirm Loss",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes) return;

            var stockLog = new StockLog
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Action = "Loss",
                Quantity = LossQuantity,
                StockBefore = SelectedProduct.Stock,
                StockAfter = SelectedProduct.Stock - LossQuantity,
                Reason = LossReason,
                Date = DateTime.Now
            };

            SelectedProduct.Stock -= LossQuantity;
            _dataService.SaveProduct(SelectedProduct);
            _dataService.SaveStockLog(stockLog);
            
            LoadProducts();
            LossReason = string.Empty;
            LossQuantity = 1;
            MessageBox.Show($"Product loss recorded: {stockLog.Quantity} units removed from inventory!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}