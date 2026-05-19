using CommunityToolkit.Mvvm.ComponentModel;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PokiePawsDesk.ViewModels
{
    public partial class ProductsViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private const int LowStockThreshold = 10;

        [ObservableProperty] private ObservableCollection<Product> products = new();
        [ObservableProperty] private ObservableCollection<Product> lowStockProducts = new();
        [ObservableProperty] private List<string> availableCategories = new();
        [ObservableProperty] private List<string> availableUnits = new();
        [ObservableProperty] private Product? selectedProduct;
        [ObservableProperty] private bool lowStockIndicatorVisible = false;
        [ObservableProperty] private string lowStockIndicatorText = "";
        [ObservableProperty] private string lowStockSubtitleText = "";

        public long WarehouseId { get; private set; } = 1;

        public ProductsViewModel(IProductService productService)
        {
            _productService = productService;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            var local = _productService.GetLocalProducts();
            if (local.Count > 0)
            {
                Products = new ObservableCollection<Product>(local);
                RefreshFilters();
                CheckLowStock();
            }

            WarehouseId = await _productService.GetMyWarehouseIdAsync();
            var fresh = await _productService.GetProductsAsync();
            Products = new ObservableCollection<Product>(fresh);
            RefreshFilters();
            CheckLowStock();
        }

        private void RefreshFilters()
        {
            AvailableCategories = Products
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList()!;

            AvailableUnits = Products
                .Select(p => p.Unit)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct()
                .OrderBy(u => u)
                .ToList()!;
        }

        private void CheckLowStock()
        {
            var low = Products.Where(p => p.Amount <= LowStockThreshold).ToList();
            LowStockProducts = new ObservableCollection<Product>(low);

            if (low.Count == 0)
            {
                LowStockIndicatorVisible = false;
                return;
            }

            LowStockIndicatorText = $"{low.Count} {LanguageService.Get("Products_LowStock_Badge")}";
            LowStockSubtitleText = $"{low.Count} {LanguageService.Get("Products_LowStock_Subtitle")}";
            LowStockIndicatorVisible = true;
        }

        public async Task AddProductAsync()
        {
            const string defaultName = "Nowy produkt";
            var existing = Products.FirstOrDefault(p =>
                p.Name?.Equals(defaultName, System.StringComparison.OrdinalIgnoreCase) == true);
            if (existing != null)
            {
                SelectedProduct = existing;
                return;
            }

            var newProduct = new Product
            {
                WarehouseId = WarehouseId,
                Name = defaultName,
                Category = AvailableCategories.FirstOrDefault() ?? "Inne",
                Unit = AvailableUnits.FirstOrDefault() ?? "szt",
                Amount = 0,
                Price = 0
            };

            var created = await _productService.CreateAsync(newProduct);
            if (created != null)
            {
                Products.Add(created);
                RefreshFilters();
                CheckLowStock();
                SelectedProduct = created;
            }
        }

        public async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;
            var toDelete = SelectedProduct;
            await _productService.DeleteAsync(toDelete.Id);
            Products.Remove(toDelete);
            RefreshFilters();
            CheckLowStock();
        }

        public async Task UpdateProductAsync(Product product)
        {
            product.WarehouseId = WarehouseId;
            await _productService.UpdateAsync(product);
            RefreshFilters();
            CheckLowStock();
        }

        public async Task ApplyDeliveryAsync(int deliveredQuantity)
        {
            if (SelectedProduct == null) return;
            var product = SelectedProduct;
            product.WarehouseId = WarehouseId;
            product.Amount += deliveredQuantity;

            var updated = await _productService.UpdateAsync(product);
            if (updated != null)
            {
                var idx = Products.IndexOf(product);
                if (idx >= 0)
                    Products[idx] = updated;
            }
            RefreshFilters();
            CheckLowStock();
        }
    }
}