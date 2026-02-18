using InventoryAndSalesSystem.Models;
using InventoryAndSalesSystem.Services;
using InventoryAndSalesSystem.ViewModels;
using InventoryAndSalesSystem.Views;
using System;
using System.IO;
using System.Windows;

namespace InventoryAndSalesSystem
{
    public partial class MainWindow : Window
    {
        private readonly ExcelDataService _dataService;
        private readonly MainViewModel _mainViewModel;
        private readonly BackupService _backupService;
        private readonly AuditTrailService _auditService;
        private readonly CycleCountService _cycleCountService;

        public MainWindow()
        {
            InitializeComponent();

            _dataService = new ExcelDataService();
            _mainViewModel = new MainViewModel(_dataService);

            // Resolve the data folder the same way ExcelDataService does
            var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            _auditService = new AuditTrailService(dataFolder);
            _backupService = new BackupService(dataFolder);
            _cycleCountService = new CycleCountService(dataFolder);

            // Start weekly auto-backup timer; logs to audit trail when fired
            _backupService.Start();

            ShowProductList_Click(null, null);
        }

        protected override void OnClosed(EventArgs e)
        {
            _backupService.Dispose();
            base.OnClosed(e);
        }

        private void ShowProductList_Click(object? sender, RoutedEventArgs e)
        {
            var view = new ProductListView();
            view.DataContext = new ProductViewModel(_dataService, _auditService);
            ContentArea.Content = view;
        }

        private void ShowAddEdit_Click(object? sender, RoutedEventArgs e)
        {
            var view = new AddEditProductView();
            view.DataContext = new ProductViewModel(_dataService, _auditService);
            ContentArea.Content = view;
        }

        private void ShowSell_Click(object? sender, RoutedEventArgs e)
        {
            var view = new SellView();
            view.DataContext = new ProductViewModel(_dataService, _auditService);
            ContentArea.Content = view;
        }

        private void ShowRestock_Click(object? sender, RoutedEventArgs e)
        {
            var view = new RestockView();
            view.DataContext = new ProductViewModel(_dataService, _auditService);
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
            view.DataContext = new ProductViewModel(_dataService, _auditService);
            ContentArea.Content = view;
        }

        private void ShowPurchaseReturn_Click(object? sender, RoutedEventArgs e)
        {
            var view = new PurchaseReturnView();
            view.DataContext = new ProductViewModel(_dataService, _auditService);
            ContentArea.Content = view;
        }

        private void ShowProductLoss_Click(object? sender, RoutedEventArgs e)
        {
            var view = new ProductLossView();
            view.DataContext = new ProductViewModel(_dataService, _auditService);
            ContentArea.Content = view;
        }

        private void ShowCycleCount_Click(object? sender, RoutedEventArgs e)
        {
            var view = new CycleCountView();
            view.DataContext = new CycleCountViewModel(_cycleCountService, _dataService, _auditService);
            ContentArea.Content = view;
        }

        private void ShowAuditTrail_Click(object? sender, RoutedEventArgs e)
        {
            var view = new AuditTrailView();
            view.DataContext = new AuditTrailViewModel(_auditService);
            ContentArea.Content = view;
        }

        private void ShowBackup_Click(object? sender, RoutedEventArgs e)
        {
            var view = new BackupView();
            view.DataContext = new BackupViewModel(_backupService, _auditService);
            ContentArea.Content = view;
        }
    }
}