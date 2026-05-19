using Microsoft.Extensions.DependencyInjection;
using PokiePawsDesk.Core;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PokiePawsDesk.Views
{
    public partial class DashboardWindow : Window
    {
        private readonly AuthService _authService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IClinicService _clinicService;
        private readonly AppDbContext _db;
        private readonly HttpClient _httpClient;
        private WebSocketService? _webSocketService;

        private Button? _activeNav;
        private int _newOrderCount = 0;
        private bool _isOnline = true;

        public DashboardWindow(AuthService authService, IOrderService orderService,
            IProductService productService, IClinicService clinicService,
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
            UpdateLanguageToggle();
            LoadUserInfo();
            _ = CheckConnectionAsync();
            ConnectWebSocket();
            MainFrame.Navigate(new OverviewPage(_orderService, _productService, _clinicService));
        }

        private void UpdateLanguageToggle()
        {
            bool isPl = LanguageService.Instance.CurrentLanguage == "pl";
            BtnPL.Style = isPl
                ? (Style)FindResource("PillBtnActive")
                : (Style)FindResource("PillBtnInactive");
            BtnEN.Style = isPl
                ? (Style)FindResource("PillBtnInactive")
                : (Style)FindResource("PillBtnActive");
        }

        private void BtnPL_Click(object sender, RoutedEventArgs e)
        {
            if (LanguageService.Instance.CurrentLanguage == "pl") return;
            LanguageService.Instance.SetLanguage("pl");
            UpdateLanguageToggle();
            RefreshCurrentPage();
        }

        private void BtnEN_Click(object sender, RoutedEventArgs e)
        {
            if (LanguageService.Instance.CurrentLanguage == "en") return;
            LanguageService.Instance.SetLanguage("en");
            UpdateLanguageToggle();
            RefreshCurrentPage();
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
                UserNameText.Text = LanguageService.Get("Nav_RoleName");
            }
        }

        private async Task CheckConnectionAsync()
        {
            while (true)
            {
                bool online;
                try
                {
                    var response = await _httpClient.GetAsync("/api/clinics");
                    online = response.IsSuccessStatusCode;
                }
                catch
                {
                    online = false;
                }

                Dispatcher.Invoke(() =>
                {
                    if (online)
                    {
                        ConnectionDot.Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                        ConnectionText.Text = LanguageService.Get("Connection_Online");
                    }
                    else
                    {
                        ConnectionDot.Fill = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                        ConnectionText.Text = LanguageService.Get("Connection_Offline");
                    }

                    if (online && !_isOnline)
                        RefreshCurrentPage();

                    _isOnline = online;
                });

                await Task.Delay(30000);
            }
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

                string text = LanguageService.Get("Notification_NewOrder");
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("clinicName", out var clinic) &&
                        clinic.GetString() is string clinicName &&
                        !string.IsNullOrWhiteSpace(clinicName))
                    {
                        text = $"{LanguageService.Get("Notification_NewOrderFrom")} {clinicName}";
                    }
                }
                catch { }

                NotificationText.Text = text;
                NotificationBanner.Visibility = Visibility.Visible;
            });
        }

        private void DismissNotification_Click(object sender, RoutedEventArgs e)
        {
            NotificationBanner.Visibility = Visibility.Collapsed;
        }

        private void RefreshCurrentPage()
        {
            if (_activeNav == BtnOverview)
                MainFrame.Navigate(new OverviewPage(_orderService, _productService, _clinicService));
            else if (_activeNav == BtnOrders)
                MainFrame.Navigate(new OrdersPage(_orderService, _clinicService));
            else if (_activeNav == BtnProducts)
                MainFrame.Navigate(new ProductsPage(_productService));
            else if (_activeNav == BtnClinics)
                MainFrame.Navigate(new ClinicsPage(_clinicService, _orderService));
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
            MainFrame.Navigate(new ClinicsPage(_clinicService, _orderService));
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