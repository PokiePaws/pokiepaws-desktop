namespace PokiePawsDesk.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public int LowStockThreshold { get; set; }

        public string StockStatus => Stock <= LowStockThreshold ? "Niski stan" : "OK";
        public string StockColor => Stock <= LowStockThreshold ? "#B71C1C" : "#2E7D32";
    }
}