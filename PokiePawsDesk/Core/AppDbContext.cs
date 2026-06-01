using Microsoft.EntityFrameworkCore;
using PokiePawsDesk.Models;

namespace PokiePawsDesk.Core
{
    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Clinic> Clinics { get; set; }

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (options.IsConfigured) return;
            SQLitePCL.Batteries_V2.Init();
            var key = DbKeyProvider.GetOrCreateKey();
            options.UseSqlite($"Data Source={DbKeyProvider.DbPath};Password={key}");
        }

        public async Task ClearAllDataAsync()
        {
            Orders.RemoveRange(Orders);
            Products.RemoveRange(Products);
            Clinics.RemoveRange(Clinics);
            await SaveChangesAsync();
        }
    }
}