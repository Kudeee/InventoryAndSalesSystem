using System;
using System.Diagnostics;
using System.Windows;

namespace InventoryAndSalesSystem
{
    public partial class ProductDialog : Window
    {
        public Product Product { get; set; } 
        public ProductDialog(Product product = null)
        {
            InitializeComponent();
            if (product != null)
            {
                Product = new Product
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    Price = product.Price,
                    Stock = product.Stock,
                    SoldUnits = product.SoldUnits
                };

                NameTextBox.Text = Product.Name;
                CategoryTextBox.Text = Product.Category;
                PriceTextBox.Text = Product.Price.ToString();
                StockTextBox.Text = Product.Stock.ToString();
                SoldUnitsTextBox.Text = Product.SoldUnits.ToString();
                Title = "Edit Product";
            }
            else
            {
                Product = new Product();
                Title = "Add Product";
            }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text)){
                MessageBox.Show("Please entre a product name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if(!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if(!int.TryParse(StockTextBox.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Please enter a valid stock quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if(!int.TryParse(SoldUnitsTextBox.Text, out int soldUnits) || soldUnits < 0)
            {
                MessageBox.Show("Please enter a valid sold unit quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Product.Name = NameTextBox.Text;
            Product.Category = CategoryTextBox.Text;
            Product.Price = price;
            Product.Stock = stock;
            Product.SoldUnits = soldUnits;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
