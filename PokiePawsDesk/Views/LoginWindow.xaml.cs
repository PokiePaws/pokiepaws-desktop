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
                ErrorText.Text = LanguageService.Get("Login_Error_Empty");
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
                    ErrorText.Text = LanguageService.Get("Login_Error_Invalid");
                }
            }
            catch
            {
                ErrorText.Text = LanguageService.Get("Login_Error_Server");
            }
        }
    }
}