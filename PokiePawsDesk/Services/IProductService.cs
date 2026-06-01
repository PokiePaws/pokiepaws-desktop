using PokiePawsDesk.Models;

namespace PokiePawsDesk.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        List<Product> GetLocalProducts();
        Task<long> GetWarehouseIdAsync();
        Task<Product?> CreateAsync(Product product);
        Task<Product?> UpdateAsync(Product product);
        Task DeleteAsync(long id);
    }
}