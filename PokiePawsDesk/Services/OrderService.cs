using PokiePawsDesk.Core;
using PokiePawsDesk.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PokiePawsDesk.Services
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;
        private const string BaseUrl = "http://localhost:9090";

        public OrderService(HttpClient httpClient, AppDbContext db)
        {
            _httpClient = httpClient;
            _db = db;
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            try
            {
                var orders = await _httpClient.GetFromJsonAsync<List<Order>>($"{BaseUrl}/api/orders");
                if (orders != null)
                {
                    await SyncToLocalAsync(orders);
                    AppLogger.LogInfo("Zamówienia pobrane z API i zsynchronizowane lokalnie");
                    return orders;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Brak połączenia z API, tryb offline: {ex.Message}");
            }

            AppLogger.LogInfo("Zamówienia pobrane z lokalnej bazy");
            return await GetOrdersFromLocalAsync();
        }

        private async Task SyncToLocalAsync(List<Order> orders)
        {
            _db.Orders.RemoveRange(_db.Orders);
            _db.Orders.AddRange(orders);
            await _db.SaveChangesAsync();
        }

        private Task<List<Order>> GetOrdersFromLocalAsync()
        {
            return Task.FromResult(_db.Orders.ToList());
        }
    }
}