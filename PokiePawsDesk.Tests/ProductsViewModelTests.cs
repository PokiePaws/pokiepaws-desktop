using Moq;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;

namespace PokiePawsDesk.Tests
{
    public class ProductsViewModelTests
    {
        private readonly List<Product> _testProducts = new()
        {
            new Product { Id = 1, Name = "Produkt A", Category = "Leki",       Unit = "szt", Amount = 5  },
            new Product { Id = 2, Name = "Produkt B", Category = "Szczepionki", Unit = "ml",  Amount = 50 },
            new Product { Id = 3, Name = "Produkt C", Category = "Leki",       Unit = "szt", Amount = 3  },
            new Product { Id = 4, Name = "Produkt D", Category = "Sprzęt",     Unit = "szt", Amount = 20 },
        };

        private (ProductsViewModel vm, Mock<IProductService> mock) Build()
        {
            var mock = new Mock<IProductService>();
            mock.Setup(x => x.GetLocalProducts()).Returns(new List<Product>());
            mock.Setup(x => x.GetWarehouseIdAsync()).ReturnsAsync(1L);
            mock.Setup(x => x.GetProductsAsync()).ReturnsAsync(_testProducts);
            return (new ProductsViewModel(mock.Object), mock);
        }

        [Fact]
        public async Task AvailableCategories_ExtractsDistinctCategoriesOrdered()
        {
            var (vm, _) = Build();
            await Task.Delay(300);
            Assert.Equal(new[] { "Leki", "Sprzęt", "Szczepionki" }, vm.AvailableCategories);
        }

        [Fact]
        public async Task AvailableUnits_ExtractsDistinctUnits()
        {
            var (vm, _) = Build();
            await Task.Delay(300);
            Assert.Equal(2, vm.AvailableUnits.Count);
            Assert.Contains("szt", vm.AvailableUnits);
            Assert.Contains("ml", vm.AvailableUnits);
        }

        [Fact]
        public async Task LowStockVisible_TrueWhenProductsBelowThreshold()
        {
            var (vm, _) = Build();
            await Task.Delay(300);
            Assert.True(vm.LowStockVisible);
        }

        [Fact]
        public async Task LowStockProducts_ContainsOnlyProductsBelowOrEqualToThreshold()
        {
            var (vm, _) = Build();
            await Task.Delay(300);
            Assert.Equal(2, vm.LowStockProducts.Count);
            Assert.All(vm.LowStockProducts, p => Assert.True(p.Amount <= 10));
        }

        [Fact]
        public async Task LowStockVisible_FalseWhenNoLowStockProducts()
        {
            var mock = new Mock<IProductService>();
            var highStock = new List<Product>
            {
                new() { Id = 1, Name = "A", Category = "Leki", Unit = "szt", Amount = 100 },
                new() { Id = 2, Name = "B", Category = "Leki", Unit = "szt", Amount = 200 },
            };
            mock.Setup(x => x.GetLocalProducts()).Returns(new List<Product>());
            mock.Setup(x => x.GetWarehouseIdAsync()).ReturnsAsync(1L);
            mock.Setup(x => x.GetProductsAsync()).ReturnsAsync(highStock);
            var vm = new ProductsViewModel(mock.Object);
            await Task.Delay(300);
            Assert.False(vm.LowStockVisible);
        }

        [Fact]
        public async Task DeleteProductAsync_RemovesSelectedProductFromCollection()
        {
            var (vm, mock) = Build();
            mock.Setup(x => x.DeleteAsync(It.IsAny<long>())).Returns(Task.CompletedTask);
            await Task.Delay(300);
            vm.SelectedProduct = vm.Products[0];
            var initialCount = vm.Products.Count;
            await vm.DeleteProductAsync();
            Assert.Equal(initialCount - 1, vm.Products.Count);
            mock.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Once);
        }

        [Fact]
        public async Task AddProductAsync_CallsCreateAndAddsToCollection()
        {
            var (vm, mock) = Build();
            var newProduct = new Product { Id = 99, Name = "Nowy produkt", Category = "Inne", Unit = "szt", Amount = 0 };
            mock.Setup(x => x.CreateAsync(It.IsAny<Product>())).ReturnsAsync(newProduct);
            await Task.Delay(300);
            var initialCount = vm.Products.Count;
            await vm.AddProductAsync();
            Assert.Equal(initialCount + 1, vm.Products.Count);
            mock.Verify(x => x.CreateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_CallsUpdateOnService()
        {
            var (vm, mock) = Build();
            var product = _testProducts[0];
            mock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(product);
            await Task.Delay(300);
            await vm.UpdateProductAsync(product);
            mock.Verify(x => x.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task ApplyDeliveryAsync_UpdatesProductAmountInCollection()
        {
            var (vm, mock) = Build();
            var updatedProduct = new Product { Id = 1, Name = "Produkt A", Category = "Leki", Unit = "szt", Amount = 15 };
            mock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(updatedProduct);
            await Task.Delay(300);
            vm.SelectedProduct = vm.Products.First(p => p.Id == 1);
            await vm.ApplyDeliveryAsync(10);
            var updated = vm.Products.First(p => p.Id == 1);
            Assert.Equal(15, updated.Amount);
        }
    }
}