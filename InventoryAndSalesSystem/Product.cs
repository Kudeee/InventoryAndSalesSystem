using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryAndSalesSystem
{
    public class Product: INotifyPropertyChanged
    {
        private int id;
        private string name;
        private string category;
        private decimal price;
        private int stock;
        private int soldUnits;

        public int Id
        {
            get => id;
            set { id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Category
        {
            get => category;
            set { category = value; OnPropertyChanged(nameof(Category)); }
        }

        public decimal Price
        {
            get => price;
            set { price = value; OnPropertyChanged(nameof(Price)); }
        }

        public int Stock
        {
            get => stock;
            set { stock = value; OnPropertyChanged(nameof(Stock)); }
        }

        public int SoldUnits
        {
            get => soldUnits;
            set { soldUnits = value; OnPropertyChanged(nameof(SoldUnits)); }
        }

        public decimal TotalSales => Price * SoldUnits;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
