using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace PokiePawsDesk.Views
{
    public partial class OrdersPage : Page
    {
        private readonly OrderService _orderService;
        private ObservableCollection<Order> _orders;

        public static List<string> Statuses { get; } = new List<string>
        {
            "Nowe", "W realizacji", "Wysłane", "Dostarczone"
        };

        public OrdersPage(OrderService orderService)
        {
            InitializeComponent();
            _orderService = orderService;
            LoadOrders();
        }

        private async void LoadOrders()
        {
            var orders = await _orderService.GetOrdersAsync();
            _orders = new ObservableCollection<Order>(orders);
            OrdersGrid.ItemsSource = _orders;
        }
    }
}