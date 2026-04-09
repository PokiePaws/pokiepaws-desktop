using PokiePawsDesk.Core;
using PokiePawsDesk.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PokiePawsDesk.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;
        private const string BaseUrl = "http://localhost:9090";

        public ProductService(HttpClient httpClient, AppDbContext db)
        {
            _httpClient = httpClient;
            _db = db;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<Product>>($"{BaseUrl}/api/products");
                if (products != null)
                {
                    await SyncToLocalAsync(products);
                    AppLogger.LogInfo("Produkty pobrane z API i zsynchronizowane lokalnie");
                    return products;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Brak połączenia z API, tryb offline: {ex.Message}");
            }

            AppLogger.LogInfo("Produkty pobrane z lokalnej bazy");
            return await GetProductsFromLocalAsync();
        }

        private async Task SyncToLocalAsync(List<Product> products)
        {
            _db.Products.RemoveRange(_db.Products);
            _db.Products.AddRange(products);
            await _db.SaveChangesAsync();
        }

        private Task<List<Product>> GetProductsFromLocalAsync()
        {
            return Task.FromResult(_db.Products.ToList());
        }
    }
}