using InventoryAndSalesSystem.Helpers;
using InventoryAndSalesSystem.Models;
using InventoryAndSalesSystem.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;

namespace InventoryAndSalesSystem.ViewModels
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private readonly ExcelDataService _dataService;
        private ObservableCollection<Sale> _recentSales;
        private ObservableCollection<StockLog> _recentLogs;
        private ObservableCollection<ProductProfitReport> _profitReports;
        private decimal _totalRevenue;
        private decimal _totalCost;
        private decimal _totalProfit;
        private decimal _profitMargin;
        private int _totalSales;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedReportType = "Revenue";

        // Chart data for ScottPlot
        private List<Sale> _currentSales = new List<Sale>();
        private List<Product> _currentProducts = new List<Product>();

        public ReportViewModel(ExcelDataService dataService)
        {
            _dataService = dataService;
            _recentSales = new ObservableCollection<Sale>();
            _recentLogs = new ObservableCollection<StockLog>();
            _profitReports = new ObservableCollection<ProductProfitReport>();
            
            _startDate = new DateTime(2020, 1, 1);
            _endDate = DateTime.Now.AddYears(1);

            ApplyFilterCommand = new RelayCommand(_ => LoadReports());
            ResetFilterCommand = new RelayCommand(_ => ResetFilters());
            ExportToPdfCommand = new RelayCommand(_ => ExportToPdf());
            
            LoadReports();
        }

        public ObservableCollection<Sale> RecentSales
        {
            get => _recentSales;
            set { _recentSales = value; OnPropertyChanged(); }
        }

        public ObservableCollection<StockLog> RecentLogs
        {
            get => _recentLogs;
            set { _recentLogs = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProductProfitReport> ProfitReports
        {
            get => _profitReports;
            set { _profitReports = value; OnPropertyChanged(); }
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(); }
        }

        public decimal TotalCost
        {
            get => _totalCost;
            set { _totalCost = value; OnPropertyChanged(); }
        }

        public decimal TotalProfit
        {
            get => _totalProfit;
            set { _totalProfit = value; OnPropertyChanged(); }
        }

        public decimal ProfitMargin
        {
            get => _profitMargin;
            set { _profitMargin = value; OnPropertyChanged(); }
        }

        public int TotalSales
        {
            get => _totalSales;
            set { _totalSales = value; OnPropertyChanged(); }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); }
        }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set 
            { 
                _selectedReportType = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChartNeedsUpdate));
            }
        }

        public List<string> ReportTypes { get; } = new List<string>
        {
            "Revenue",
            "Profit",
            "Units Sold"
        };

        public ICommand ApplyFilterCommand { get; }
        public ICommand ResetFilterCommand { get; }
        public ICommand ExportToPdfCommand { get; }

        // Property to trigger chart update
        public bool ChartNeedsUpdate => true;

        public List<Sale> CurrentSales => _currentSales;
        public List<Product> CurrentProducts => _currentProducts;

        private void ResetFilters()
        {
            StartDate = new DateTime(2020, 1, 1);
            EndDate = DateTime.Now.AddYears(1);
            LoadReports();
        }

        private void LoadReports()
        {
            var allSales = _dataService.GetAllSales();
            var allProducts = _dataService.GetAllProducts();
            var logs = _dataService.GetAllStockLogs();

            if (!allSales.Any())
            {
                MessageBox.Show("No sales data available!", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var filteredSales = allSales.Where(s => 
                s.Date.Date >= StartDate.Date && 
                s.Date.Date <= EndDate.Date
            ).ToList();

            if (!filteredSales.Any())
            {
                filteredSales = allSales.ToList();
            }

            _currentSales = filteredSales;
            _currentProducts = allProducts;

            RecentSales.Clear();
            foreach (var sale in filteredSales.OrderByDescending(s => s.Date).Take(10))
            {
                RecentSales.Add(sale);
            }

            RecentLogs.Clear();
            foreach (var log in logs.OrderByDescending(l => l.Date).Take(10))
            {
                RecentLogs.Add(log);
            }

            TotalRevenue = filteredSales.Sum(s => s.Total);
            TotalSales = filteredSales.Sum(s => s.Quantity);

            TotalCost = 0;
            foreach (var sale in filteredSales)
            {
                var product = allProducts.FirstOrDefault(p => p.Id == sale.ProductId);
                if (product != null)
                {
                    TotalCost += product.UnitCost * sale.Quantity;
                }
            }

            TotalProfit = TotalRevenue - TotalCost;
            ProfitMargin = TotalRevenue > 0 ? (TotalProfit / TotalRevenue) * 100 : 0;

            GenerateProfitReports(filteredSales, allProducts);
            
            OnPropertyChanged(nameof(ChartNeedsUpdate));
        }

        private void GenerateProfitReports(List<Sale> sales, List<Product> products)
        {
            ProfitReports.Clear();

            var productGroups = sales.GroupBy(s => s.ProductId);

            foreach (var group in productGroups)
            {
                var product = products.FirstOrDefault(p => p.Id == group.Key);
                if (product == null) continue;

                var totalRevenue = group.Sum(s => s.Total);
                var totalQuantity = group.Sum(s => s.Quantity);
                var totalCost = product.UnitCost * totalQuantity;
                var profit = totalRevenue - totalCost;
                var margin = totalRevenue > 0 ? (profit / totalRevenue) * 100 : 0;

                ProfitReports.Add(new ProductProfitReport
                {
                    ProductName = product.Name,
                    UnitsSold = totalQuantity,
                    Revenue = totalRevenue,
                    Cost = totalCost,
                    Profit = profit,
                    ProfitMargin = margin
                });
            }
        }

        private void ExportToPdf()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Sales_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    DefaultExt = "pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var pdfService = new PdfExportService();
                    
                    pdfService.ExportReportToPdf(
                        saveDialog.FileName,
                        StartDate,
                        EndDate,
                        TotalRevenue,
                        TotalCost,
                        TotalProfit,
                        ProfitMargin,
                        TotalSales,
                        ProfitReports.ToList(),
                        RecentSales.ToList(),
                        RecentLogs.ToList()
                    );

                    MessageBox.Show(
                        $"Report exported successfully to:\n{saveDialog.FileName}", 
                        "Export Successful", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error exporting report: {ex.Message}", 
                    "Export Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProductProfitReport
    {
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
    }
}