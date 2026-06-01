using Moq;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;

namespace PokiePawsDesk.Tests
{
    public class OverviewViewModelTests
    {
        private readonly Mock<IOrderService> _mockOrderService = new();
        private readonly Mock<IProductService> _mockProductService = new();
        private readonly Mock<IClinicService> _mockClinicService = new();

        private OverviewViewModel CreateViewModel(
            List<Order>? orders = null,
            List<Product>? products = null,
            List<Clinic>? clinics = null)
        {
            orders ??= new List<Order>();
            products ??= new List<Product>();
            clinics ??= new List<Clinic>();

            _mockOrderService.Setup(x => x.GetLocalOrders()).Returns(new List<Order>());
            _mockOrderService
                .Setup(x => x.GetOrdersAsync(It.IsAny<long?>(), It.IsAny<string?>()))
                .ReturnsAsync(orders);

            _mockProductService.Setup(x => x.GetLocalProducts()).Returns(new List<Product>());
            _mockProductService.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            _mockClinicService.Setup(x => x.GetLocalClinics()).Returns(new List<Clinic>());
            _mockClinicService.Setup(x => x.GetClinicsAsync()).ReturnsAsync(clinics);

            return new OverviewViewModel(
                _mockOrderService.Object,
                _mockProductService.Object,
                _mockClinicService.Object);
        }

        [Fact]
        public async Task PendingCount_ReflectsOnlyPendingOrders()
        {
            var orders = new List<Order>
            {
                new() { Status = "PENDING" },
                new() { Status = "PENDING" },
                new() { Status = "DELIVERED" },
            };

            var vm = CreateViewModel(orders: orders);
            await Task.Delay(300);

            Assert.Equal("2", vm.PendingCount);
        }

        [Fact]
        public async Task ProductCount_ReflectsTotalProductCount()
        {
            var products = new List<Product>
            {
                new() { Id = 1, Amount = 10 },
                new() { Id = 2, Amount = 20 },
                new() { Id = 3, Amount = 30 },
            };

            var vm = CreateViewModel(products: products);
            await Task.Delay(300);

            Assert.Equal("3", vm.ProductCount);
        }

        [Fact]
        public async Task LowStockCount_ReflectsProductsBelowOrEqualToTen()
        {
            var products = new List<Product>
            {
                new() { Id = 1, Amount = 5  },
                new() { Id = 2, Amount = 10 },
                new() { Id = 3, Amount = 11 },
                new() { Id = 4, Amount = 50 },
            };

            var vm = CreateViewModel(products: products);
            await Task.Delay(300);

            Assert.Equal("2", vm.LowStockCount);
        }

        [Fact]
        public async Task ClinicCount_ReflectsOnlyActiveClinics()
        {
            var clinics = new List<Clinic>
            {
                new() { Id = 1, Active = true  },
                new() { Id = 2, Active = true  },
                new() { Id = 3, Active = false },
            };

            var vm = CreateViewModel(clinics: clinics);
            await Task.Delay(300);

            Assert.Equal("2", vm.ClinicCount);
        }

        [Fact]
        public async Task RecentOrders_ContainsOnlyPendingOrders()
        {
            var orders = new List<Order>
            {
                new() { Id = 1, Status = "PENDING"   },
                new() { Id = 2, Status = "SHIPPED"   },
                new() { Id = 3, Status = "PENDING"   },
                new() { Id = 4, Status = "DELIVERED" },
            };

            var vm = CreateViewModel(orders: orders);
            await Task.Delay(300);

            Assert.Equal(2, vm.RecentOrders.Count);
            Assert.All(vm.RecentOrders, o => Assert.Equal("PENDING", o.Status));
        }
    }
}