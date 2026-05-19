using Moq;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;

namespace PokiePawsDesk.Tests
{
    public class ClinicsViewModelTests
    {
        private readonly Mock<IClinicService> _mockClinicService = new();
        private readonly Mock<IOrderService> _mockOrderService = new();

        private ClinicsViewModel CreateViewModel()
        {
            _mockClinicService.Setup(x => x.GetLocalClinics()).Returns(new List<Clinic>());
            _mockClinicService.Setup(x => x.GetClinicsAsync()).ReturnsAsync(new List<Clinic>());
            _mockOrderService.Setup(x => x.GetLocalOrders()).Returns(new List<Order>());
            _mockOrderService
                .Setup(x => x.GetOrdersAsync(It.IsAny<long?>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<Order>());

            return new ClinicsViewModel(_mockClinicService.Object, _mockOrderService.Object);
        }

        [Fact]
        public void SelectedClinic_WhenSet_IsClinicSelectedIsTrue()
        {
            var vm = CreateViewModel();

            vm.SelectedClinic = new Clinic { Id = 1, ClinicName = "Testowa" };

            Assert.True(vm.IsClinicSelected);
            Assert.False(vm.IsClinicNotSelected);
        }

        [Fact]
        public void SelectedClinic_WhenSetToNull_IsClinicNotSelectedIsTrue()
        {
            var vm = CreateViewModel();
            vm.SelectedClinic = new Clinic { Id = 1 };

            vm.SelectedClinic = null;

            Assert.False(vm.IsClinicSelected);
            Assert.True(vm.IsClinicNotSelected);
        }

        [Fact]
        public void InitialState_IsClinicNotSelectedIsTrue()
        {
            var vm = CreateViewModel();

            Assert.False(vm.IsClinicSelected);
            Assert.True(vm.IsClinicNotSelected);
        }

        [Fact]
        public async Task ClinicOrders_FilteredBySelectedClinic()
        {
            var orders = new List<Order>
            {
                new() { Id = 1, ClinicId = 1, Status = "PENDING" },
                new() { Id = 2, ClinicId = 2, Status = "PENDING" },
                new() { Id = 3, ClinicId = 1, Status = "SHIPPED" },
            };

            _mockClinicService.Setup(x => x.GetLocalClinics()).Returns(new List<Clinic>());
            _mockClinicService.Setup(x => x.GetClinicsAsync()).ReturnsAsync(new List<Clinic>());
            _mockOrderService.Setup(x => x.GetLocalOrders()).Returns(orders);
            _mockOrderService
                .Setup(x => x.GetOrdersAsync(It.IsAny<long?>(), It.IsAny<string?>()))
                .ReturnsAsync(orders);

            var vm = new ClinicsViewModel(_mockClinicService.Object, _mockOrderService.Object);
            vm.SelectedClinic = new Clinic { Id = 1 };
            await Task.Delay(300);

            Assert.Equal(2, vm.ClinicOrders.Count);
            Assert.All(vm.ClinicOrders, o => Assert.Equal(1, o.ClinicId));
        }

        [Fact]
        public void SelectedClinic_WhenChanged_ClinicOrdersCleared()
        {
            var vm = CreateViewModel();
            vm.SelectedClinic = new Clinic { Id = 1 };

            vm.SelectedClinic = new Clinic { Id = 2 };

            Assert.Empty(vm.ClinicOrders);
        }
    }
}