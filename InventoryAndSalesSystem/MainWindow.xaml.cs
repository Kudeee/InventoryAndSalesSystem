using InventoryAndSalesSystem.Models;
using InventoryAndSalesSystem.Services;
using InventoryAndSalesSystem.ViewModels;
using InventoryAndSalesSystem.Views;
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
        private readonly ExcelDataService _dataService;
        private readonly MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _dataService = new ExcelDataService();
            _mainViewModel = new MainViewModel(_dataService);
            ShowProductList_Click(null, null);
        }

        private void ShowProductList_Click(object? sender, RoutedEventArgs e)
        {
            var view = new ProductListView();
            view.DataContext = new ProductViewModel(_dataService);
            ContentArea.Content = view;
        }

        private void ShowAddEdit_Click(object? sender, RoutedEventArgs e)
        {
            var view = new AddEditProductView();
            view.DataContext = new ProductViewModel(_dataService);
            ContentArea.Content = view;
        }

        private void ShowSell_Click(object? sender, RoutedEventArgs e)
        {
            var view = new SellView();
            view.DataContext = new ProductViewModel(_dataService);
            ContentArea.Content = view;
        }

        private void ShowRestock_Click(object? sender, RoutedEventArgs e)
        {
            var view = new RestockView();
            view.DataContext = new ProductViewModel(_dataService);
            ContentArea.Content = view;
        }

        private void ShowReports_Click(object? sender, RoutedEventArgs e)
        {
            var view = new ReportsView();
            view.DataContext = new ReportViewModel(_dataService);
            ContentArea.Content = view;
        }

        private void ShowSalesReturn_Click(object? sender, RoutedEventArgs e)
        {
            var view = new SalesReturnView();
            view.DataContext = new ProductViewModel(_dataService);
            ContentArea.Content = view;
        }

        private void ShowPurchaseReturn_Click(object? sender, RoutedEventArgs e)
        {
            var view = new PurchaseReturnView();
            view.DataContext = new ProductViewModel(_dataService);
            ContentArea.Content = view;
        }

        private void ShowProductLoss_Click(object? sender, RoutedEventArgs e)
        {
            var view = new ProductLossView();
            view.DataContext = new ProductViewModel(_dataService);
            ContentArea.Content = view;
        }
    }
}