using Microsoft.EntityFrameworkCore;
using PokiePawsDesk.Models;

namespace PokiePawsDesk.Core
{
    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Clinic> Clinics { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
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