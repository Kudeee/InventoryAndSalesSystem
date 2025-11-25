using InventoryAndSalesSystem.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InventoryAndSalesSystem.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ExcelDataService _dataService;

        public MainViewModel(ExcelDataService dataService)
        {
            _dataService = dataService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
