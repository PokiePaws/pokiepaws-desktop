using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PokiePawsDesk.Views
{
    public partial class ProductsPage : Page
    {
        private readonly ProductService _productService;
        private ObservableCollection<Product> _products = new();
        private List<Product> _lowStockProducts = new();
        private const int LowStockThreshold = 10;
        private long _warehouseId = 1;

        public ProductsPage(ProductService productService)
        {
            InitializeComponent();
            _productService = productService;
            LoadData();
        }

        private async void LoadData()
        {
            _warehouseId = await _productService.GetMyWarehouseIdAsync();
            var products = await _productService.GetProductsAsync();
            _products = new ObservableCollection<Product>(products);
            ProductsGrid.ItemsSource = _products;
            CheckLowStock();
        }

        private void CheckLowStock()
        {
            _lowStockProducts = _products.Where(p => p.Amount <= LowStockThreshold).ToList();
            LowStockGrid.ItemsSource = _lowStockProducts;

            if (!_lowStockProducts.Any())
            {
                DetailsPanelColumn.Width = new GridLength(0);
                LowStockIndicator.Visibility = Visibility.Collapsed;
                return;
            }

            LowStockIndicatorText.Text = $"{_lowStockProducts.Count} produktów z niskim stanem";
            LowStockIndicator.Visibility = Visibility.Visible;
            LowStockSubtitle.Text = $"{_lowStockProducts.Count} produktów wymaga uzupełnienia";
            DetailsPanelColumn.Width = new GridLength(320);
        }

        private void LowStockIndicator_Click(object sender, MouseButtonEventArgs e)
        {
            DetailsPanelColumn.Width = new GridLength(320);
        }

        private void ClosePanelButton_Click(object sender, RoutedEventArgs e)
        {
            DetailsPanelColumn.Width = new GridLength(0);
        }

        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var newProduct = new Product
            {
                WarehouseId = _warehouseId,
                Name = "Nowy produkt",
                Category = "Inne",
                Amount = 0,
                Price = 0
            };

            try
            {
                var created = await _productService.CreateAsync(newProduct);
                if (created != null)
                {
                    _products.Add(created);
                    ProductsGrid.SelectedItem = created;
                    ProductsGrid.ScrollIntoView(created);
                    CheckLowStock();
                }
            }
            catch
            {
                MessageBox.Show("Nie udało się dodać produktu.");
            }
        }

        private async void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is not Product selected)
            {
                MessageBox.Show("Wybierz produkt do usunięcia.");
                return;
            }

            try
            {
                await _productService.DeleteAsync(selected.Id);
                _products.Remove(selected);
                CheckLowStock();
            }
            catch
            {
                MessageBox.Show("Nie udało się usunąć produktu.");
            }
        }

        private async void ProductsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
            if (e.Row.Item is not Product product) return;
            if (product.Id == 0) return;

            await Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    product.WarehouseId = _warehouseId;
                    var updated = await _productService.UpdateAsync(product);
                    if (updated != null)
                    {
                        var idx = _products.IndexOf(product);
                        if (idx >= 0)
                            _products[idx] = updated;
                    }
                    CheckLowStock();
                }
                catch
                {
                    MessageBox.Show("Nie udało się zapisać zmian.");
                }
            });
        }
    }
}