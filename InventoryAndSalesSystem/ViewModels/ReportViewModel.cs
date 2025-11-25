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
using System.Text;
using System.Threading.Tasks;

namespace InventoryAndSalesSystem.ViewModels
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private readonly ExcelDataService _dataService;
        private ObservableCollection<Sale> _recentSales;
        private ObservableCollection<StockLog> _recentLogs;
        private decimal _totalRevenue;
        private int _totalSales;
        private ISeries[] _salesSeries = Array.Empty<ISeries>();
        private Axis[] _xAxes = Array.Empty<Axis>();

        public ReportViewModel(ExcelDataService dataService)
        {
            _dataService = dataService;
            _recentSales = new ObservableCollection<Sale>();
            _recentLogs = new ObservableCollection<StockLog>();
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

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(); }
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

        public Axis[] XAxes
        {
            get => _xAxes;
            set { _xAxes = value; OnPropertyChanged(); }
        }

        private void LoadReports()
        {
            var sales = _dataService.GetAllSales();
            var logs = _dataService.GetAllStockLogs();

            RecentSales.Clear();
            foreach (var sale in sales.OrderByDescending(s => s.Date).Take(10))
            {
                RecentSales.Add(sale);
            }

            RecentLogs.Clear();
            foreach (var log in logs.OrderByDescending(l => l.Date).Take(10))
            {
                RecentLogs.Add(log);
            }

            TotalRevenue = sales.Sum(s => s.Total);
            TotalSales = sales.Sum(s => s.Quantity);

            LoadChart(sales);
        }

        private void LoadChart(List<Sale> sales)
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
