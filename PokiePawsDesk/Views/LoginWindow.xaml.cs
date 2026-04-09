using Microsoft.Extensions.DependencyInjection;
using PokiePawsDesk.Services;
using System.Windows;

namespace PokiePawsDesk.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;

        public LoginWindow(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ErrorText.Text = "Podaj e-mail i hasło.";
                return;
            }

            try
            {
                var result = await _authService.LoginAsync(email, password);

                if (result != null)
                {
                    var dashboard = App.Services.GetRequiredService<DashboardWindow>();
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    ErrorText.Text = "Nieprawidłowy e-mail lub hasło.";
                }
            }
            catch
            {
                ErrorText.Text = "Nie można połączyć się z serwerem.";
            }
        }
    }
}