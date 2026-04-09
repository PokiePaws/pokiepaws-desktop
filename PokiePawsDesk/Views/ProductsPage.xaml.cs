using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PokiePawsDesk.Views
{
    public partial class ProductsPage : Page
    {
        private readonly ProductService _productService;
        private ObservableCollection<Product> _products;
        private List<Product> _lowStockProducts;

        public ProductsPage(ProductService productService)
        {
            InitializeComponent();
            _productService = productService;
            LoadProducts();
        }

        private async void LoadProducts()
        {
            var products = await _productService.GetProductsAsync();
            _products = new ObservableCollection<Product>(products);
            ProductsGrid.ItemsSource = _products;
            CheckLowStock();
        }

        private void CheckLowStock()
        {
            _lowStockProducts = _products.Where(p => p.Stock <= p.LowStockThreshold).ToList();

            if (!_lowStockProducts.Any())
            {
                LowStockBanner.Visibility = Visibility.Collapsed;
                DetailsPanelColumn.Width = new GridLength(0);
                return;
            }

            LowStockText.Text = $"{_lowStockProducts.Count} produktów ma niski stan magazynowy.";
            LowStockBanner.Visibility = Visibility.Visible;
        }

        private void LowStockDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            LowStockGrid.ItemsSource = _lowStockProducts;
            DetailsPanelColumn.Width = new GridLength(400);
        }

        private void ClosePanelButton_Click(object sender, RoutedEventArgs e)
        {
            DetailsPanelColumn.Width = new GridLength(0);
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var newProduct = new Product
            {
                Id = _products.Count + 1,
                Name = "Nowy produkt",
                Category = "Inne",
                Stock = 0,
                Price = 0,
                LowStockThreshold = 10
            };

            _products.Add(newProduct);
            ProductsGrid.SelectedItem = newProduct;
            ProductsGrid.ScrollIntoView(newProduct);
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is not Product selected)
            {
                MessageBox.Show("Wybierz produkt do usunięcia.");
                return;
            }

            _products.Remove(selected);
        }
    }
}