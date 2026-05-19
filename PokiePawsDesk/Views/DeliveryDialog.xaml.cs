using PokiePawsDesk.Services;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PokiePawsDesk.Views
{
    public partial class DeliveryDialog : Window
    {
        public int DeliveredQuantity { get; private set; }

        public DeliveryDialog(string productName)
        {
            InitializeComponent();

            TitleText.Text = LanguageService.Get("Products_Delivery_Title");
            ProductLabelText.Text = LanguageService.Get("Products_Delivery_Product");
            ProductNameText.Text = productName;
            QtyLabelText.Text = LanguageService.Get("Products_Delivery_Qty");
            CancelBtn.Content = LanguageService.Get("Products_Delivery_Cancel");
            ConfirmBtn.Content = LanguageService.Get("Products_Delivery_Confirm");

            QuantityBox.Focus();
        }

        private void QuantityBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void QuantityBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                TryConfirm();
            else if (e.Key == Key.Escape)
                DialogResult = false;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            TryConfirm();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TryConfirm()
        {
            if (!int.TryParse(QuantityBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show(
                    LanguageService.Get("Products_Delivery_InvalidQty"),
                    LanguageService.Get("Error_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                QuantityBox.Focus();
                return;
            }

            DeliveredQuantity = qty;
            DialogResult = true;
        }
    }
}