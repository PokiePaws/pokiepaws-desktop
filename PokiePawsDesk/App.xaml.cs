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

            var auth = Services.GetRequiredService<AuthService>();
            auth.RestoreToken();

            var loginWindow = Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppLogger.Shutdown();
            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:9090") };
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            services.AddSingleton(httpClient);
            services.AddSingleton<AppDbContext>();
            services.AddSingleton<AuthService>();
            services.AddSingleton<OrderService>();
            services.AddSingleton<ProductService>();
            services.AddSingleton<ClinicService>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<DashboardWindow>();
        }
    }
}