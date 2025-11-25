using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InventoryAndSalesSystem
{
    public partial class SalesReportWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public List<Product> Products {get; set;}
        public SalesReportWindow(List<Product> products)
        {
            InitializeComponent();
            Products = products;
            LoadChartData();
            LoadStatistics();
            DataContext = this;
        }

        private void LoadChartData()
        {
            var topProducts = Products.OrderByDescending(p => p.TotalSales).Take(10).ToList();

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Sales Revenue",
                    Values = new ChartValues<decimal>(topProducts.Select(p => p.TotalSales))
                }
            };

            Labels = topProducts.Select(p => p.Name).ToArray();
        }

        private void LoadStatistics()
        {
            TotalRevenueText.Text = $"${Products.Sum(p => p.TotalSales):N2}";
            TotalProductsText.Text = Products.Count.ToString();
            TotalStockText.Text = Products.Sum(p => p.Stock).ToString();
            TotalSoldText.Text = Products.Sum(p => p.SoldUnits).ToString();

            var topProduct = Products.OrderByDescending(p => p.TotalSales).FirstOrDefault();
            TopProductText.Text = topProduct != null ? $"{topProduct.Name} (${topProduct.TotalSales:N2})" : "N/A";
        }
    }
}
