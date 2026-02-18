using InventoryAndSalesSystem.Helpers;
using InventoryAndSalesSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace InventoryAndSalesSystem.ViewModels
{
    public class AuditTrailViewModel : INotifyPropertyChanged
    {
        private readonly AuditTrailService _auditService;
        private ObservableCollection<AuditEntry> _entries = new();
        private ObservableCollection<AuditEntry> _filteredEntries = new();
        private string _searchText = "";
        private string _selectedActionFilter = "All Actions";
        private DateTime _fromDate = DateTime.Now.AddMonths(-1);
        private DateTime _toDate = DateTime.Now;

        public AuditTrailViewModel(AuditTrailService auditService)
        {
            _auditService = auditService;
            RefreshCommand = new RelayCommand(_ => LoadEntries());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            LoadEntries();
        }

        public ObservableCollection<AuditEntry> FilteredEntries
        {
            get => _filteredEntries;
            set { _filteredEntries = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        public string SelectedActionFilter
        {
            get => _selectedActionFilter;
            set { _selectedActionFilter = value; OnPropertyChanged(); ApplyFilters(); }
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set { _fromDate = value; OnPropertyChanged(); }
        }

        public DateTime ToDate
        {
            get => _toDate;
            set { _toDate = value; OnPropertyChanged(); }
        }

        public System.Collections.Generic.List<string> ActionFilters { get; } = new()
        {
            "All Actions",
            "Product Added",
            "Product Edited",
            "Product Deleted",
            "Sale",
            "Restock",
            "Sales Return",
            "Purchase Return",
            "Product Loss",
            "Cycle Count Started",
            "Cycle Count Item",
            "Cycle Count Completed",
            "Manual Backup",
            "Auto Backup"
        };

        public int TotalShown => FilteredEntries.Count;
        public int TotalRecords => _entries.Count;

        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        private void LoadEntries()
        {
            _entries.Clear();
            foreach (var e in _auditService.GetAllEntries())
                _entries.Add(e);
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _entries.AsEnumerable();

            if (SelectedActionFilter != "All Actions")
                filtered = filtered.Where(e => e.Action == SelectedActionFilter);

            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(e =>
                    e.EntityName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.Details.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.Action.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            filtered = filtered.Where(e =>
                e.Timestamp.Date >= FromDate.Date &&
                e.Timestamp.Date <= ToDate.Date);

            FilteredEntries.Clear();
            foreach (var e in filtered)
                FilteredEntries.Add(e);

            OnPropertyChanged(nameof(TotalShown));
            OnPropertyChanged(nameof(TotalRecords));
        }

        private void ClearFilters()
        {
            SearchText = "";
            SelectedActionFilter = "All Actions";
            FromDate = DateTime.Now.AddMonths(-1);
            ToDate = DateTime.Now;
            ApplyFilters();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}