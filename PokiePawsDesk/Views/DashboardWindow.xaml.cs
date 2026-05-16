using Microsoft.Extensions.DependencyInjection;
using PokiePawsDesk.Core;
using PokiePawsDesk.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
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
        private WebSocketService? _webSocketService;

        private Button? _activeNav;
        private int _newOrderCount = 0;

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

            _activeNav = BtnOverview;
            LoadUserInfo();
            CheckConnection();
            ConnectWebSocket();
            MainFrame.Navigate(new OverviewPage(_orderService, _productService, _clinicService));
        }

        private async void LoadUserInfo()
        {
            try
            {
                var me = await _httpClient.GetFromJsonAsync<WarehouseWorkerMe>("/api/warehouse-workers/me");
                if (me != null)
                {
                    UserNameText.Text = $"{me.FirstName} {me.LastName}";
                    UserEmailText.Text = me.Email ?? "";
                }
            }
            catch
            {
                UserNameText.Text = "Pracownik magazynu";
            }
        }

        private async void CheckConnection()
        {
            while (true)
            {
                try
                {
                    var response = await _httpClient.GetAsync("/actuator/health");
                    Dispatcher.Invoke(() =>
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            ConnectionDot.Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                            ConnectionText.Text = "Online";
                        }
                        else
                        {
                            SetOffline();
                        }
                    });
                }
                catch
                {
                    Dispatcher.Invoke(SetOffline);
                }

                await Task.Delay(30000);
            }
        }

        private void SetOffline()
        {
            ConnectionDot.Fill = new SolidColorBrush(Color.FromRgb(220, 38, 38));
            ConnectionText.Text = "Offline";
        }

        private void ConnectWebSocket()
        {
            var token = _authService.GetToken();
            if (token == null) return;

            _webSocketService = new WebSocketService("ws://localhost:9090/ws", token);
            _webSocketService.OnNewOrder += OnNewOrderReceived;
            _ = _webSocketService.ConnectAsync();
        }

        private void OnNewOrderReceived(string json)
        {
            Dispatcher.Invoke(() =>
            {
                _newOrderCount++;
                OrdersBadge.Visibility = Visibility.Visible;
                OrdersBadgeText.Text = _newOrderCount.ToString();
                NotificationText.Text = "Nowe zamówienie od gabinetu!";
                NotificationBanner.Visibility = Visibility.Visible;
            });
        }

        private void DismissNotification_Click(object sender, RoutedEventArgs e)
        {
            NotificationBanner.Visibility = Visibility.Collapsed;
        }

        private void SetActive(Button btn)
        {
            if (_activeNav != null)
                _activeNav.Style = (Style)FindResource("NavBtn");
            btn.Style = (Style)FindResource("NavBtnActive");
            _activeNav = btn;
        }

        private void BtnOverview_Click(object sender, RoutedEventArgs e)
        {
            SetActive(BtnOverview);
            MainFrame.Navigate(new OverviewPage(_orderService, _productService, _clinicService));
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            SetActive(BtnOrders);
            _newOrderCount = 0;
            OrdersBadge.Visibility = Visibility.Collapsed;
            MainFrame.Navigate(new OrdersPage(_orderService, _clinicService));
        }

        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            SetActive(BtnProducts);
            MainFrame.Navigate(new ProductsPage(_productService));
        }

        private void BtnClinics_Click(object sender, RoutedEventArgs e)
        {
            SetActive(BtnClinics);
            MainFrame.Navigate(new ClinicsPage(_clinicService));
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_webSocketService != null)
                await _webSocketService.DisconnectAsync();

            _authService.RemoveToken();
            await _db.ClearAllDataAsync();

            var loginWindow = App.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
            this.Close();
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            if (_webSocketService != null)
                await _webSocketService.DisconnectAsync();
        }
    }
}