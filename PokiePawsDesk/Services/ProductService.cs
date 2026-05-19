using PokiePawsDesk.Core;
using PokiePawsDesk.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PokiePawsDesk.Services
{
    public class WarehouseWorkerMe
    {
        public long WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;

        public ProductService(HttpClient httpClient, AppDbContext db)
        {
            _httpClient = httpClient;
            _db = db;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<Product>>("/api/warehouse/stock");
                if (products != null)
                {
                    await SyncToLocalAsync(products);
                    return products;
                }
            }
            catch { }

            return _db.Products.ToList();
        }

        public List<Product> GetLocalProducts()
        {
            return _db.Products.ToList();
        }

        public async Task<List<Product>> GetLowStockAsync(int threshold = 10)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<Product>>($"/api/warehouse/stock/low-stock?threshold={threshold}");
                return result ?? _db.Products.Where(p => p.Amount <= threshold).ToList();
            }
            catch { }

            return _db.Products.Where(p => p.Amount <= threshold).ToList();
        }

        public async Task<long> GetMyWarehouseIdAsync()
        {
            try
            {
                var me = await _httpClient.GetFromJsonAsync<WarehouseWorkerMe>("/api/warehouse-workers/me");
                return me?.WarehouseId ?? 1;
            }
            catch { return 1; }
        }

        public async Task<Product?> CreateAsync(Product product)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/warehouse/stock", product);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        public async Task<Product?> UpdateAsync(Product product)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/warehouse/stock/{product.Id}", product);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        public async Task DeleteAsync(long id)
        {
            var response = await _httpClient.DeleteAsync($"/api/warehouse/stock/{id}");
            response.EnsureSuccessStatusCode();
        }

        private async Task SyncToLocalAsync(List<Product> products)
        {
            _db.Products.RemoveRange(_db.Products);
            _db.Products.AddRange(products);
            await _db.SaveChangesAsync();
        }
    }
}