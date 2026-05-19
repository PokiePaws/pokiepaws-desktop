using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using PokiePawsDesk.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PokiePawsDesk.Views
{
    public partial class ProductsPage : Page
    {
        private readonly ProductsViewModel _viewModel;
        private DispatcherTimer? _notificationTimer;

        public ProductsPage(IProductService productService)
        {
            InitializeComponent();
            _viewModel = new ProductsViewModel(productService);
            DataContext = _viewModel;
        }

        private void LowStockIndicator_Click(object sender, MouseButtonEventArgs e)
        {
            DetailsPanelColumn.Width = new GridLength(320);
        }

        private void ClosePanelButton_Click(object sender, RoutedEventArgs e)
        {
            DetailsPanelColumn.Width = new GridLength(0);
        }

        private void ShowNotification(string message)
        {
            _notificationTimer?.Stop();
            InlineNotificationText.Text = message;
            InlineNotification.Visibility = Visibility.Visible;

            _notificationTimer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(3) };
            _notificationTimer.Tick += (s, e) =>
            {
                InlineNotification.Visibility = Visibility.Collapsed;
                _notificationTimer!.Stop();
            };
            _notificationTimer.Start();
        }

        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.AddProductAsync();
                if (_viewModel.SelectedProduct != null)
                    ProductsGrid.ScrollIntoView(_viewModel.SelectedProduct);
            }
            catch
            {
                MessageBox.Show("Nie udało się dodać produktu.");
            }
        }

        private async void DeliveryButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedProduct == null)
            {
                ShowNotification(LanguageService.Get("Products_Delivery_NoSelection"));
                return;
            }

            var dialog = new DeliveryDialog(_viewModel.SelectedProduct.Name ?? string.Empty)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                await _viewModel.ApplyDeliveryAsync(dialog.DeliveredQuantity);
            }
            catch
            {
                MessageBox.Show(
                    LanguageService.Get("Products_Delivery_Error"),
                    LanguageService.Get("Error_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedProduct == null)
            {
                ShowNotification(LanguageService.Get("Products_Delivery_NoSelection"));
                return;
            }

            try
            {
                await _viewModel.DeleteProductAsync();
            }
            catch
            {
                MessageBox.Show("Nie udało się usunąć produktu.");
            }
        }

        private void ProductsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
            if (e.Row.Item is not Product product) return;
            if (product.Id == 0) return;

            Dispatcher.BeginInvoke(new Action(async () =>
            {
                try
                {
                    await _viewModel.UpdateProductAsync(product);
                }
                catch
                {
                    MessageBox.Show("Nie udało się zapisać zmian. Sprawdź czy nazwa produktu jest unikalna.");
                }
            }));
        }
    }
}