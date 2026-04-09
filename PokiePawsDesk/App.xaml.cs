using Microsoft.Extensions.DependencyInjection;
using PokiePawsDesk.Core;
using PokiePawsDesk.Models;
using PokiePawsDesk.Services;
using PokiePawsDesk.Views;
using System.Net.Http;
using System.Windows;

namespace PokiePawsDesk
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppLogger.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            var db = Services.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedDatabase(db);

            var loginWindow = Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppLogger.Shutdown();
            base.OnExit(e);
        }

        private void SeedDatabase(AppDbContext db)
        {
            if (db.Orders.Any()) return;

            db.Clinics.AddRange(
                new Clinic { Id = 1, Name = "Gabinet Wrocław Centrum", City = "Wrocław", Address = "ul. Świdnicka 12", VetName = "dr Jan Kowalski", IsActive = true },
                new Clinic { Id = 2, Name = "Gabinet Kraków", City = "Kraków", Address = "ul. Floriańska 5", VetName = "dr Anna Nowak", IsActive = true },
                new Clinic { Id = 3, Name = "Gabinet Warszawa Mokotów", City = "Warszawa", Address = "ul. Puławska 34", VetName = "dr Piotr Wiśniewski", IsActive = false },
                new Clinic { Id = 4, Name = "Gabinet Poznań", City = "Poznań", Address = "ul. Półwiejska 8", VetName = "dr Maria Wójcik", IsActive = true },
                new Clinic { Id = 5, Name = "Gabinet Gdańsk", City = "Gdańsk", Address = "ul. Długa 22", VetName = "dr Tomasz Lewandowski", IsActive = true }
            );

            db.Orders.AddRange(
                new Order { Id = 1, ClinicId = 1, ClinicName = "Gabinet Wrocław Centrum", Status = "Nowe", CreatedAt = DateTime.Now.AddDays(-1), TotalPrice = 450.00m },
                new Order { Id = 2, ClinicId = 2, ClinicName = "Gabinet Kraków", Status = "W realizacji", CreatedAt = DateTime.Now.AddDays(-2), TotalPrice = 1200.50m },
                new Order { Id = 3, ClinicId = 3, ClinicName = "Gabinet Warszawa Mokotów", Status = "Wysłane", CreatedAt = DateTime.Now.AddDays(-3), TotalPrice = 890.00m },
                new Order { Id = 4, ClinicId = 4, ClinicName = "Gabinet Poznań", Status = "Dostarczone", CreatedAt = DateTime.Now.AddDays(-5), TotalPrice = 340.00m },
                new Order { Id = 5, ClinicId = 1, ClinicName = "Gabinet Wrocław Centrum", Status = "Nowe", CreatedAt = DateTime.Now, TotalPrice = 675.00m }
            );

            db.Products.AddRange(
                new Product { Id = 1, Name = "Amoksycylina 500mg", Category = "Leki", Stock = 150, Price = 25.00m, LowStockThreshold = 20 },
                new Product { Id = 2, Name = "Szczepionka przeciw wściekliźnie", Category = "Szczepionki", Stock = 8, Price = 120.00m, LowStockThreshold = 10 },
                new Product { Id = 3, Name = "Strzykawki 5ml", Category = "Sprzęt", Stock = 500, Price = 2.50m, LowStockThreshold = 50 },
                new Product { Id = 4, Name = "Rękawiczki lateksowe", Category = "Sprzęt", Stock = 5, Price = 15.00m, LowStockThreshold = 20 },
                new Product { Id = 5, Name = "Ibuprofen 200mg", Category = "Leki", Stock = 200, Price = 18.00m, LowStockThreshold = 30 }
            );

            db.SaveChanges();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();
            services.AddSingleton<AppDbContext>();
            services.AddSingleton<AuthService>();
            services.AddSingleton<OrderService>();
            services.AddSingleton<ProductService>();
            services.AddSingleton<ClinicService>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<LoginWindow>();
            services.AddSingleton<DashboardWindow>();
        }
    }
}