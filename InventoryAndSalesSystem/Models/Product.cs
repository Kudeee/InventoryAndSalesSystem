using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InventoryAndSalesSystem.Models
{
    public class Product: INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _category = string.Empty;
        private decimal _price;
        private int _stock;
        private int _minStock;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLowStock)); }
        }

        public int MinStock
        {
            get => _minStock;
            set { _minStock = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLowStock)); }
        }

        public bool IsLowStock => Stock <= MinStock;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
