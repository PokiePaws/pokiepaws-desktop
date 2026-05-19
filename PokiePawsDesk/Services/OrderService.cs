using PokiePawsDesk.Core;
using PokiePawsDesk.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PokiePawsDesk.Services
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;

        public OrderService(HttpClient httpClient, AppDbContext db)
        {
            _httpClient = httpClient;
            _db = db;
        }

        public async Task<List<Order>> GetOrdersAsync(long? clinicId = null, string? status = null)
        {
            try
            {
                var url = "/api/orders";
                var query = new List<string>();
                if (clinicId.HasValue) query.Add($"clinicId={clinicId}");
                if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");
                if (query.Count > 0) url += "?" + string.Join("&", query);

                var orders = await _httpClient.GetFromJsonAsync<List<Order>>(url);
                if (orders != null)
                {
                    await SyncToLocalAsync(orders);
                    return orders;
                }
            }
            catch { }

            return _db.Orders.ToList();
        }

        public List<Order> GetLocalOrders()
        {
            return _db.Orders.ToList();
        }

        public async Task<Order?> UpdateStatusAsync(long id, string status)
        {
            var body = new StringContent(JsonSerializer.Serialize(new { status }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"/api/orders/{id}/status", body);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }

        private async Task SyncToLocalAsync(List<Order> orders)
        {
            _db.Orders.RemoveRange(_db.Orders);
            _db.Orders.AddRange(orders);
            await _db.SaveChangesAsync();
        }
    }
}