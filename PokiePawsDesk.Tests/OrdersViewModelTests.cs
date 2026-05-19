using Moq;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;

namespace PokiePawsDesk.Tests
{
    public class OrdersViewModelTests
    {
        private readonly Mock<IOrderService> _mockOrderService = new();
        private readonly Mock<IClinicService> _mockClinicService = new();

        private readonly List<Order> _testOrders = new()
        {
            new Order { Id = 1, Status = "PENDING",     ClinicId = 1, ClinicName = "Klinika A" },
            new Order { Id = 2, Status = "IN_PROGRESS", ClinicId = 1, ClinicName = "Klinika A" },
            new Order { Id = 3, Status = "PENDING",     ClinicId = 2, ClinicName = "Klinika B" },
            new Order { Id = 4, Status = "SHIPPED",     ClinicId = 2, ClinicName = "Klinika B" },
            new Order { Id = 5, Status = "DELIVERED",   ClinicId = 1, ClinicName = "Klinika A" },
        };

        private OrdersViewModel CreateViewModel()
        {
            _mockOrderService
                .Setup(x => x.GetLocalOrders())
                .Returns(_testOrders);
            _mockOrderService
                .Setup(x => x.GetOrdersAsync(It.IsAny<long?>(), It.IsAny<string?>()))
                .ReturnsAsync(_testOrders);
            _mockClinicService
                .Setup(x => x.GetLocalClinics())
                .Returns(new List<Clinic>());
            _mockClinicService
                .Setup(x => x.GetClinicsAsync())
                .ReturnsAsync(new List<Clinic>());

            return new OrdersViewModel(_mockOrderService.Object, _mockClinicService.Object);
        }

        [Fact]
        public async Task SetStatusFilter_Pending_ShowsOnlyPendingOrders()
        {
            var vm = CreateViewModel();
            await Task.Delay(300);

            vm.SetStatusFilter("PENDING");

            Assert.Equal(2, vm.FilteredOrders.Count);
            Assert.All(vm.FilteredOrders, o => Assert.Equal("PENDING", o.Status));
        }

        [Fact]
        public async Task SetStatusFilter_Null_ShowsAllOrders()
        {
            var vm = CreateViewModel();
            await Task.Delay(300);

            vm.SetStatusFilter(null);

            Assert.Equal(_testOrders.Count, vm.FilteredOrders.Count);
        }

        [Fact]
        public async Task SetStatusFilter_Shipped_ShowsOnlyShippedOrders()
        {
            var vm = CreateViewModel();
            await Task.Delay(300);

            vm.SetStatusFilter("SHIPPED");

            Assert.Single(vm.FilteredOrders);
            Assert.Equal("SHIPPED", vm.FilteredOrders[0].Status);
        }

        [Fact]
        public async Task ClinicFilter_FiltersOrdersByClinic()
        {
            var vm = CreateViewModel();
            await Task.Delay(300);

            var clinicFilter = new Clinic { Id = 2 };
            vm.SelectedClinicFilter = clinicFilter;

            Assert.Equal(2, vm.FilteredOrders.Count);
            Assert.All(vm.FilteredOrders, o => Assert.Equal(2, o.ClinicId));
        }

        [Fact]
        public async Task StatusAndClinicFilter_CombinedFiltersWork()
        {
            var vm = CreateViewModel();
            await Task.Delay(300);

            vm.SelectedClinicFilter = new Clinic { Id = 1 };
            vm.SetStatusFilter("PENDING");

            Assert.Single(vm.FilteredOrders);
            Assert.Equal(1, vm.FilteredOrders[0].Id);
        }

        [Fact]
        public async Task StartOrderCommand_CallsUpdateStatusWithInProgress()
        {
            _mockOrderService
                .Setup(x => x.UpdateStatusAsync(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(new Order());

            var vm = CreateViewModel();
            await Task.Delay(300);

            var order = _testOrders[0];
            await vm.StartOrderCommand.ExecuteAsync(order);

            _mockOrderService.Verify(x => x.UpdateStatusAsync(order.Id, "IN_PROGRESS"), Times.Once);
        }

        [Fact]
        public async Task ShipOrderCommand_CallsUpdateStatusWithShipped()
        {
            _mockOrderService
                .Setup(x => x.UpdateStatusAsync(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(new Order());

            var vm = CreateViewModel();
            await Task.Delay(300);

            var order = _testOrders[1];
            await vm.ShipOrderCommand.ExecuteAsync(order);

            _mockOrderService.Verify(x => x.UpdateStatusAsync(order.Id, "SHIPPED"), Times.Once);
        }

        [Fact]
        public async Task ConfirmOrderCommand_CallsUpdateStatusWithDelivered()
        {
            _mockOrderService
                .Setup(x => x.UpdateStatusAsync(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(new Order());

            var vm = CreateViewModel();
            await Task.Delay(300);

            var order = _testOrders[3];
            await vm.ConfirmOrderCommand.ExecuteAsync(order);

            _mockOrderService.Verify(x => x.UpdateStatusAsync(order.Id, "DELIVERED"), Times.Once);
        }
    }
}