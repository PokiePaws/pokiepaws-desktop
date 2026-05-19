using PokiePawsDesk.Models;

namespace PokiePawsDesk.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersAsync(long? clinicId = null, string? status = null);
        List<Order> GetLocalOrders();
        Task<Order?> UpdateStatusAsync(long id, string status);
    }
}