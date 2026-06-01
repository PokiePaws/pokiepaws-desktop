using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokiePawsDesk.Core;
using PokiePawsDesk.Services;
using PokiePawsDesk.Views;
using System.Net.Http;
using System.Windows;

namespace PokiePawsDesk
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppLogger.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            var db = Services.GetRequiredService<AppDbContext>();
            db.Database.Migrate();

            var authService = Services.GetRequiredService<AuthService>();
            var existingToken = authService.GetToken();

            if (existingToken != null)
            {
                var dashboard = Services.GetRequiredService<DashboardWindow>();
                dashboard.Show();
            }
            else
            {
                var loginWindow = Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppLogger.Shutdown();
            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var baseUri = new Uri(AppConfig.ApiBaseUrl);

            var authHttpClient = new HttpClient
            {
                BaseAddress = baseUri,
                Timeout = TimeSpan.FromSeconds(30)
            };

            var authService = new AuthService(authHttpClient);
            services.AddSingleton(authService);

            var authHandler = new AuthHandler(authService);
            var mainHttpClient = new HttpClient(authHandler)
            {
                BaseAddress = baseUri,
                Timeout = TimeSpan.FromSeconds(30)
            };

            services.AddSingleton(mainHttpClient);
            services.AddSingleton<AppDbContext>();
            services.AddSingleton<IOrderService, OrderService>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<IClinicService, ClinicService>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<DashboardWindow>();
        }
    }
}