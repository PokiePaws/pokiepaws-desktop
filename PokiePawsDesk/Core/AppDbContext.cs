using Microsoft.EntityFrameworkCore;
using PokiePawsDesk.Models;
using System.Threading.Tasks;

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
            options.UseSqlite("Data Source=pokiepaws.db;Password=PokiePaws2026!");
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