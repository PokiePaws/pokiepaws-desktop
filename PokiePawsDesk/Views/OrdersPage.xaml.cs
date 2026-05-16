using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PokiePawsDesk.Views
{
    public partial class OrdersPage : Page
    {
        private readonly OrderService _orderService;
        private readonly ClinicService _clinicService;
        private ObservableCollection<Order> _allOrders = new();
        private string? _selectedStatus;
        private long? _selectedClinicId;

        public OrdersPage(OrderService orderService, ClinicService clinicService)
        {
            InitializeComponent();
            _orderService = orderService;
            _clinicService = clinicService;
            _ = InitialLoadAsync();
        }

        private async Task InitialLoadAsync()
        {
            var clinics = await _clinicService.GetClinicsAsync();
            var allOption = new Clinic { Id = 0, ClinicName = "Wszystkie gabinety" };
            var clinicList = new List<Clinic> { allOption };
            clinicList.AddRange(clinics);
            ClinicFilter.ItemsSource = clinicList;
            ClinicFilter.SelectedIndex = 0;

            await RefreshOrdersAsync();
        }

        private async Task RefreshOrdersAsync()
        {
            var orders = await _orderService.GetOrdersAsync();
            _allOrders = new ObservableCollection<Order>(orders);
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (OrdersGrid == null) return;

            var filtered = _allOrders.AsEnumerable();

            if (!string.IsNullOrEmpty(_selectedStatus))
                filtered = filtered.Where(o => o.Status == _selectedStatus);

            if (_selectedClinicId.HasValue && _selectedClinicId > 0)
                filtered = filtered.Where(o => o.ClinicId == _selectedClinicId);

            var list = filtered.ToList();
            OrdersGrid.ItemsSource = list;
            OrderCountText.Text = $"{list.Count} zamówień";
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusFilter.SelectedItem is ComboBoxItem item)
                _selectedStatus = item.Tag?.ToString();
            else
                _selectedStatus = null;
            ApplyFilters();
        }

        private void ClinicFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClinicFilter.SelectedItem is Clinic clinic && clinic.Id > 0)
                _selectedClinicId = clinic.Id;
            else
                _selectedClinicId = null;
            ApplyFilters();
        }

        private async void StartOrder_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Order order) return;
            await ChangeStatusAsync(order, "IN_PROGRESS");
        }

        private async void RejectOrder_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Order order) return;
            var result = MessageBox.Show(
                $"Czy na pewno odrzucić zamówienie #{order.Id}?",
                "Odrzucenie zamówienia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            await ChangeStatusAsync(order, "REJECTED");
        }

        private async void ShipOrder_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Order order) return;
            await ChangeStatusAsync(order, "SHIPPED");
        }

        private async void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Order order) return;
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
                    "Nie udało się zaktualizować statusu zamówienia.",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
