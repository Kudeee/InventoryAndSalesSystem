using InventoryAndSalesSystem.ViewModels;
using ScottPlot;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventoryAndSalesSystem.Views
{
    public partial class ReportsView : UserControl
    {
        private ReportViewModel? ViewModel => DataContext as ReportViewModel;

        public ReportsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateChart();
        }

        private void ChartType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateChart();
        }

        private void UpdateChart()
        {
            if (ViewModel == null || ChartControl == null) return;

            ChartControl.Plot.Clear();

            var sales = ViewModel.CurrentSales;
            var products = ViewModel.CurrentProducts;

            if (!sales.Any()) return;

            switch (ViewModel.SelectedReportType)
            {
                case "Revenue":
                    LoadRevenueChart(sales);
                    break;
                case "Profit":
                    LoadProfitChart(sales, products);
                    break;
                case "Units Sold":
                    LoadUnitsSoldChart(sales);
                    break;
            }

            ChartControl.Refresh();
        }

        private void LoadRevenueChart(System.Collections.Generic.List<Models.Sale> sales)
        {
            var productSales = sales
                .GroupBy(s => s.ProductName)
                .Select(g => new { Product = g.Key, Total = (double)g.Sum(s => s.Total) })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            var values = productSales.Select(p => p.Total).ToArray();
            var labels = productSales.Select(p => p.Product).ToArray();

            var bar = ChartControl.Plot.Add.Bars(values);
            bar.Color = ScottPlot.Color.FromHex("#1E88E5");
            
            ChartControl.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray(),
                labels
            );
            ChartControl.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            ChartControl.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            
            ChartControl.Plot.Title("Revenue by Product");
            ChartControl.Plot.YLabel("Revenue (₱)");
            ChartControl.Plot.XLabel("Products");
        }

        private void LoadProfitChart(System.Collections.Generic.List<Models.Sale> sales, System.Collections.Generic.List<Models.Product> products)
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
                        Profit = (double)profit
                    };
                })
                .OrderByDescending(x => x.Profit)
                .Take(10)
                .ToList();

            var values = productProfits.Select(p => p.Profit).ToArray();
            var labels = productProfits.Select(p => p.Product).ToArray();

            var bar = ChartControl.Plot.Add.Bars(values);
            bar.Color = ScottPlot.Color.FromHex("#43A047");

            ChartControl.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray(),
                labels
            );
            ChartControl.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            ChartControl.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;

            ChartControl.Plot.Title("Profit by Product");
            ChartControl.Plot.YLabel("Profit (₱)");
            ChartControl.Plot.XLabel("Products");
        }

        private void LoadUnitsSoldChart(System.Collections.Generic.List<Models.Sale> sales)
        {
            var productUnits = sales
                .GroupBy(s => s.ProductName)
                .Select(g => new { Product = g.Key, Units = (double)g.Sum(s => s.Quantity) })
                .OrderByDescending(x => x.Units)
                .Take(10)
                .ToList();

            var values = productUnits.Select(p => p.Units).ToArray();
            var labels = productUnits.Select(p => p.Product).ToArray();

            var bar = ChartControl.Plot.Add.Bars(values);
            bar.Color = ScottPlot.Color.FromHex("#FB8C00");

            ChartControl.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray(),
                labels
            );
            ChartControl.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            ChartControl.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;

            ChartControl.Plot.Title("Units Sold by Product");
            ChartControl.Plot.YLabel("Units");
            ChartControl.Plot.XLabel("Products");
        }
    }
}