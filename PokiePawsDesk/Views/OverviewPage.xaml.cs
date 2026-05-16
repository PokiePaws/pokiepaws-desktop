using PokiePawsDesk.Services;
using System.Linq;
using System.Windows.Controls;

namespace PokiePawsDesk.Views
{
    public partial class OverviewPage : Page
    {
        private readonly OrderService _orderService;
        private readonly ProductService _productService;
        private readonly ClinicService _clinicService;

        public OverviewPage(OrderService orderService, ProductService productService, ClinicService clinicService)
        {
            InitializeComponent();
            _orderService = orderService;
            _productService = productService;
            _clinicService = clinicService;
            LoadStats();
        }

        private async void LoadStats()
        {
            var orders = await _orderService.GetOrdersAsync();
            var products = await _productService.GetProductsAsync();
            var clinics = await _clinicService.GetClinicsAsync();

            PendingCount.Text = orders.Count(o => o.Status == "PENDING").ToString();
            ProductCount.Text = products.Count.ToString();
            LowStockCount.Text = products.Count(p => p.Amount <= 10).ToString();
            ClinicCount.Text = clinics.Count(c => c.Active).ToString();

            var pending = orders.Where(o => o.Status == "PENDING").Take(5).ToList();
            RecentOrdersGrid.ItemsSource = pending;
        }
    }
}