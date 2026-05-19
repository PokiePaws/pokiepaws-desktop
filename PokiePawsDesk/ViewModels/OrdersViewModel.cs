using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PokiePawsDesk.ViewModels
{
    public partial class OrdersViewModel : ObservableObject
    {
        private readonly IOrderService _orderService;
        private readonly IClinicService _clinicService;
        private List<Order> _allOrders = new();
        private string? _selectedStatus;

        [ObservableProperty] private ObservableCollection<Order> filteredOrders = new();
        [ObservableProperty] private ObservableCollection<Clinic> clinicList = new();
        [ObservableProperty] private Clinic? selectedClinicFilter;
        [ObservableProperty] private string orderCountText = "";

        public OrdersViewModel(IOrderService orderService, IClinicService clinicService)
        {
            _orderService = orderService;
            _clinicService = clinicService;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            var allOption = new Clinic { Id = 0, ClinicName = LanguageService.Get("Orders_AllClinics") };
            var local = _clinicService.GetLocalClinics();
            var list = new List<Clinic> { allOption };
            list.AddRange(local);
            ClinicList = new ObservableCollection<Clinic>(list);
            SelectedClinicFilter = allOption;

            _allOrders = _orderService.GetLocalOrders();
            ApplyFilters();

            var freshClinics = await _clinicService.GetClinicsAsync();
            var prevId = SelectedClinicFilter?.Id;
            list = new List<Clinic> { allOption };
            list.AddRange(freshClinics);
            ClinicList = new ObservableCollection<Clinic>(list);
            SelectedClinicFilter = list.FirstOrDefault(c => c.Id == prevId) ?? allOption;

            _allOrders = await _orderService.GetOrdersAsync();
            ApplyFilters();
        }

        public void SetStatusFilter(string? status)
        {
            _selectedStatus = status;
            ApplyFilters();
        }

        partial void OnSelectedClinicFilterChanged(Clinic? value) => ApplyFilters();

        private void ApplyFilters()
        {
            var filtered = _allOrders.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedStatus))
                filtered = filtered.Where(o => o.Status == _selectedStatus);

            if (SelectedClinicFilter != null && SelectedClinicFilter.Id > 0)
                filtered = filtered.Where(o => o.ClinicId == SelectedClinicFilter.Id);

            var list = filtered.ToList();
            FilteredOrders = new ObservableCollection<Order>(list);
            OrderCountText = $"{list.Count} {LanguageService.Get("Orders_CountSuffix")}";
        }

        private async Task RefreshOrdersAsync()
        {
            _allOrders = await _orderService.GetOrdersAsync();
            ApplyFilters();
        }

        [RelayCommand]
        private async Task StartOrder(Order? order)
        {
            if (order == null) return;
            await ChangeStatusAsync(order, "IN_PROGRESS");
        }

        [RelayCommand]
        private async Task RejectOrder(Order? order)
        {
            if (order == null) return;
            var result = MessageBox.Show(
                $"{LanguageService.Get("Orders_RejectConfirm")} #{order.Id}?",
                LanguageService.Get("Orders_RejectTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            await ChangeStatusAsync(order, "REJECTED");
        }

        [RelayCommand]
        private async Task ShipOrder(Order? order)
        {
            if (order == null) return;
            await ChangeStatusAsync(order, "SHIPPED");
        }

        [RelayCommand]
        private async Task ConfirmOrder(Order? order)
        {
            if (order == null) return;
            await ChangeStatusAsync(order, "DELIVERED");
        }

        private async Task ChangeStatusAsync(Order order, string newStatus)
        {
            try
            {
                await _orderService.UpdateStatusAsync(order.Id, newStatus);
                await RefreshOrdersAsync();
            }
            catch
            {
                MessageBox.Show(
                    LanguageService.Get("Orders_Error"),
                    LanguageService.Get("Error_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}