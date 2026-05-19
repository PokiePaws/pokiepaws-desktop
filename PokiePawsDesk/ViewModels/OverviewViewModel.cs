using CommunityToolkit.Mvvm.ComponentModel;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PokiePawsDesk.ViewModels
{
    public partial class OverviewViewModel : ObservableObject
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IClinicService _clinicService;

        [ObservableProperty] private string pendingCount = "0";
        [ObservableProperty] private string productCount = "0";
        [ObservableProperty] private string lowStockCount = "0";
        [ObservableProperty] private string clinicCount = "0";
        [ObservableProperty] private ObservableCollection<Order> recentOrders = new();

        public OverviewViewModel(IOrderService orderService, IProductService productService, IClinicService clinicService)
        {
            _orderService = orderService;
            _productService = productService;
            _clinicService = clinicService;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            UpdateStats(_orderService.GetLocalOrders(), _productService.GetLocalProducts(), _clinicService.GetLocalClinics());

            var orders = await _orderService.GetOrdersAsync();
            var products = await _productService.GetProductsAsync();
            var clinics = await _clinicService.GetClinicsAsync();
            UpdateStats(orders, products, clinics);
        }

        private void UpdateStats(List<Order> orders, List<Product> products, List<Clinic> clinics)
        {
            PendingCount = orders.Count(o => o.Status == "PENDING").ToString();
            ProductCount = products.Count.ToString();
            LowStockCount = products.Count(p => p.Amount <= 10).ToString();
            ClinicCount = clinics.Count(c => c.Active).ToString();
            RecentOrders = new ObservableCollection<Order>(orders.Where(o => o.Status == "PENDING").Take(5));
        }
    }
}