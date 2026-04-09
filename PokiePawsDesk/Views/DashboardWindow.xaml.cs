using Microsoft.Extensions.DependencyInjection;
using PokiePawsDesk.Core;
using PokiePawsDesk.Services;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;

namespace PokiePawsDesk.Views
{
    public partial class DashboardWindow : Window
    {
        private readonly AuthService _authService;
        private readonly OrderService _orderService;
        private readonly ProductService _productService;
        private readonly ClinicService _clinicService;
        private readonly AppDbContext _db;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:9090";

        public DashboardWindow(AuthService authService, OrderService orderService,
            ProductService productService, ClinicService clinicService,
            AppDbContext db, HttpClient httpClient)
        {
            InitializeComponent();
            _authService = authService;
            _orderService = orderService;
            _productService = productService;
            _clinicService = clinicService;
            _db = db;
            _httpClient = httpClient;

            SetUserEmail();
            CheckConnection();
            MainFrame.Navigate(new OrdersPage(_orderService));
        }

        private void SetUserEmail()
        {
            var token = _authService.GetToken();
            if (token != null)
                UserEmailText.Text = "magazyn@pokiepaws.pl";
        }

        private async void CheckConnection()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/actuator/health");
                if (response.IsSuccessStatusCode)
                {
                    ConnectionDot.Fill = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                    ConnectionText.Text = "Online";
                    return;
                }
            }
            catch { }

            ConnectionDot.Fill = new SolidColorBrush(Color.FromRgb(255, 82, 82));
            ConnectionText.Text = "Offline";
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Czy na pewno chcesz się wylogować?",
                "Wylogowanie", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            _authService.RemoveToken();
            await _db.ClearAllDataAsync();

            var loginWindow = App.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrdersPage(_orderService));
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProductsPage(_productService));
        }

        private void ClinicsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClinicsPage(_clinicService));
        }
    }
}