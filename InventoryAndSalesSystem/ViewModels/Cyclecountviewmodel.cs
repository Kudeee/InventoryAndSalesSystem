using InventoryAndSalesSystem.Helpers;
using InventoryAndSalesSystem.Models;
using InventoryAndSalesSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace InventoryAndSalesSystem.ViewModels
{
    public class CycleCountViewModel : INotifyPropertyChanged
    {
        private readonly CycleCountService _cycleCountService;
        private readonly ExcelDataService _dataService;
        private readonly AuditTrailService _auditService;

        private ObservableCollection<CycleCountSession> _sessions = new();
        private ObservableCollection<CycleCountItem> _sessionItems = new();
        private CycleCountSession? _selectedSession;
        private CycleCountItem? _selectedItem;
        private int _physicalCount;
        private string _countNotes = "";
        private string _newSessionNotes = "";
        private bool _isSessionOpen;

        public CycleCountViewModel(CycleCountService cycleCountService,
                                    ExcelDataService dataService,
                                    AuditTrailService auditService)
        {
            _cycleCountService = cycleCountService;
            _dataService = dataService;
            _auditService = auditService;

            NewSessionCommand = new RelayCommand(_ => StartNewSession());
            SaveCountCommand = new RelayCommand(_ => SaveCount(), _ => SelectedItem != null && IsSessionOpen);
            CompleteSessionCommand = new RelayCommand(_ => CompleteSession(), _ => SelectedSession?.Status == "Open");
            RefreshCommand = new RelayCommand(_ => LoadSessions());

            LoadSessions();
        }

        public ObservableCollection<CycleCountSession> Sessions
        {
            get => _sessions;
            set { _sessions = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CycleCountItem> SessionItems
        {
            get => _sessionItems;
            set { _sessionItems = value; OnPropertyChanged(); }
        }

        public CycleCountSession? SelectedSession
        {
            get => _selectedSession;
            set
            {
                _selectedSession = value;
                OnPropertyChanged();
                IsSessionOpen = value?.Status == "Open";
                if (value != null) LoadSessionItems(value.SessionId);
                else SessionItems.Clear();
            }
        }

        public CycleCountItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                PhysicalCount = value?.CountedQty >= 0 ? value.CountedQty : value?.ExpectedQty ?? 0;
                CountNotes = value?.Notes ?? "";
            }
        }

        public int PhysicalCount
        {
            get => _physicalCount;
            set { _physicalCount = value; OnPropertyChanged(); }
        }

        public string CountNotes
        {
            get => _countNotes;
            set { _countNotes = value; OnPropertyChanged(); }
        }

        public string NewSessionNotes
        {
            get => _newSessionNotes;
            set { _newSessionNotes = value; OnPropertyChanged(); }
        }

        public bool IsSessionOpen
        {
            get => _isSessionOpen;
            set { _isSessionOpen = value; OnPropertyChanged(); }
        }

        // Summary stats for selected session
        public int TotalItems => SessionItems.Count;
        public int CountedItems => SessionItems.Count(i => i.Counted);
        public int PendingItems => SessionItems.Count(i => !i.Counted);
        public int VarianceItems => SessionItems.Count(i => i.HasVariance);
        public decimal TotalVarianceValue => SessionItems.Where(i => i.Counted).Sum(i => i.VarianceValue);

        public ICommand NewSessionCommand { get; }
        public ICommand SaveCountCommand { get; }
        public ICommand CompleteSessionCommand { get; }
        public ICommand RefreshCommand { get; }

        private void LoadSessions()
        {
            Sessions.Clear();
            foreach (var s in _cycleCountService.GetAllSessions())
                Sessions.Add(s);
        }

        private void LoadSessionItems(int sessionId)
        {
            SessionItems.Clear();
            foreach (var item in _cycleCountService.GetSessionItems(sessionId))
                SessionItems.Add(item);
            RefreshSummary();
        }

        private void RefreshSummary()
        {
            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(CountedItems));
            OnPropertyChanged(nameof(PendingItems));
            OnPropertyChanged(nameof(VarianceItems));
            OnPropertyChanged(nameof(TotalVarianceValue));
        }

        private void StartNewSession()
        {
            var products = _dataService.GetAllProducts();
            if (!products.Any())
            {
                MessageBox.Show("No products found. Add products before starting a cycle count.",
                    "No Products", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Start a new cycle count session for {products.Count} products?\n\nThe system stock quantities will be recorded as expected counts.",
                "New Cycle Count", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            var sessionId = _cycleCountService.CreateSession(products, NewSessionNotes);
            _auditService.Log("Cycle Count Started", "System", sessionId, $"Session #{sessionId}",
                $"Products: {products.Count}, Notes: {NewSessionNotes}");

            NewSessionNotes = "";
            LoadSessions();

            // Auto-select the new session
            SelectedSession = Sessions.FirstOrDefault(s => s.SessionId == sessionId);
            MessageBox.Show($"Cycle count session #{sessionId} started with {products.Count} products.",
                "Session Started", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveCount()
        {
            if (SelectedItem == null || SelectedSession == null) return;

            if (PhysicalCount < 0)
            {
                MessageBox.Show("Physical count cannot be negative.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _cycleCountService.SaveItemCount(SelectedSession.SessionId, SelectedItem.ProductId,
                PhysicalCount, CountNotes);

            _auditService.Log("Cycle Count Item", "Product", SelectedItem.ProductId, SelectedItem.ProductName,
                $"Session #{SelectedSession.SessionId}, Expected: {SelectedItem.ExpectedQty}, Counted: {PhysicalCount}, Variance: {PhysicalCount - SelectedItem.ExpectedQty}");

            // Reload items to reflect update
            LoadSessionItems(SelectedSession.SessionId);

            // Move to next uncounted item automatically
            var next = SessionItems.FirstOrDefault(i => !i.Counted);
            SelectedItem = next;
        }

        private void CompleteSession()
        {
            if (SelectedSession == null) return;

            var uncounted = SessionItems.Count(i => !i.Counted);
            if (uncounted > 0)
            {
                var result = MessageBox.Show(
                    $"{uncounted} items have not been counted yet.\n\nComplete session anyway?",
                    "Uncounted Items", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;
            }

            _cycleCountService.CompleteSession(SelectedSession.SessionId);
            _auditService.Log("Cycle Count Completed", "System", SelectedSession.SessionId,
                $"Session #{SelectedSession.SessionId}",
                $"Items: {TotalItems}, Variances: {VarianceItems}, Total Variance Value: {TotalVarianceValue:C}");

            LoadSessions();
            SelectedSession = Sessions.FirstOrDefault(s => s.SessionId == SelectedSession?.SessionId);
            MessageBox.Show("Cycle count session completed!", "Completed",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}