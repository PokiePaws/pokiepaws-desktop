using System.Text.Json.Serialization;

namespace PokiePawsDesk.Models
{
    public class WarehouseWorkerMe
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("warehouseId")]
        public long WarehouseId { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }
}