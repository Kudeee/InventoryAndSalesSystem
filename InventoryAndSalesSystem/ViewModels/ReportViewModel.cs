using InventoryAndSalesSystem.Helpers;
using InventoryAndSalesSystem.Models;
using InventoryAndSalesSystem.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
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
        private ISeries[] _salesSeries = Array.Empty<ISeries>();
        private ISeries[] _profitSeries = Array.Empty<ISeries>();
        private Axis[] _xAxes = Array.Empty<Axis>();
        private Axis[] _profitXAxes = Array.Empty<Axis>();
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedReportType = "Revenue";

        public ReportViewModel(ExcelDataService dataService)
        {
            _dataService = dataService;
            _recentSales = new ObservableCollection<Sale>();
            _recentLogs = new ObservableCollection<StockLog>();
            _profitReports = new ObservableCollection<ProductProfitReport>();
            
            // Initialize date range to current month
            _startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _endDate = DateTime.Now;

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

        public ISeries[] SalesSeries
        {
            get => _salesSeries;
            set { _salesSeries = value; OnPropertyChanged(); }
        }

        public ISeries[] ProfitSeries
        {
            get => _profitSeries;
            set { _profitSeries = value; OnPropertyChanged(); }
        }

        public Axis[] XAxes
        {
            get => _xAxes;
            set { _xAxes = value; OnPropertyChanged(); }
        }

        public Axis[] ProfitXAxes
        {
            get => _profitXAxes;
            set { _profitXAxes = value; OnPropertyChanged(); }
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
                UpdateChartByType();
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

        private void ResetFilters()
        {
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = DateTime.Now;
            LoadReports();
        }

        private void LoadReports()
        {
            var allSales = _dataService.GetAllSales();
            var allProducts = _dataService.GetAllProducts();
            var logs = _dataService.GetAllStockLogs();

            // Filter sales by date range
            var filteredSales = allSales.Where(s => s.Date >= StartDate && s.Date <= EndDate.AddDays(1)).ToList();

            // Recent sales (latest 10)
            RecentSales.Clear();
            foreach (var sale in filteredSales.OrderByDescending(s => s.Date).Take(10))
            {
                RecentSales.Add(sale);
            }

            // Recent logs (latest 10)
            RecentLogs.Clear();
            foreach (var log in logs.OrderByDescending(l => l.Date).Take(10))
            {
                RecentLogs.Add(log);
            }

            // Calculate totals
            TotalRevenue = filteredSales.Sum(s => s.Total);
            TotalSales = filteredSales.Sum(s => s.Quantity);

            // Calculate cost and profit
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

            // Generate profit reports by product
            GenerateProfitReports(filteredSales, allProducts);

            // Load charts
            LoadRevenueChart(filteredSales);
            LoadProfitChart(filteredSales, allProducts);
            UpdateChartByType();
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

        private void LoadRevenueChart(List<Sale> sales)
        {
            var productSales = sales
                .GroupBy(s => s.ProductName)
                .Select(g => new { Product = g.Key, Total = g.Sum(s => s.Total) })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            SalesSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Revenue",
                    Values = productSales.Select(p => p.Total).ToArray(),
                    Fill = new SolidColorPaint(SKColors.DodgerBlue),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = productSales.Select(p => p.Product).ToArray(),
                    LabelsRotation = 45
                }
            };
        }

        private void LoadProfitChart(List<Sale> sales, List<Product> products)
        {
            var productProfits = sales
                .GroupBy(s => s.ProductId)
                .Select(g => 
                {
                    var product = products.FirstOrDefault(p => p.Id == g.Key);
                    var revenue = g.Sum(s => s.Total);
                    var cost = product != null ? product.UnitCost * g.Sum(s => s.Quantity) : 0;
                    var profit = revenue - cost;
                    
                    return new 
                    { 
                        Product = g.First().ProductName, 
                        Profit = profit,
                        Revenue = revenue,
                        Cost = cost
                    };
                })
                .OrderByDescending(x => x.Profit)
                .Take(10)
                .ToList();

            ProfitSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Profit",
                    Values = productProfits.Select(p => p.Profit).ToArray(),
                    Fill = new SolidColorPaint(SKColors.Green),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
                }
            };

            ProfitXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = productProfits.Select(p => p.Product).ToArray(),
                    LabelsRotation = 45
                }
            };
        }

        private void UpdateChartByType()
        {
            // This method is called when the chart type changes
            // The actual chart binding is handled in the XAML based on SelectedReportType
            OnPropertyChanged(nameof(SelectedReportType));
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