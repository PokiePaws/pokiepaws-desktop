using System;
using System.Text.Json.Serialization;
using System.Windows.Media;
using PokiePawsDesk.Core;

namespace PokiePawsDesk.Models
{
    public class Product
    {
        public long Id { get; set; }
        public long WarehouseId { get; set; }
        public string? Name { get; set; }
        public string? AssortmentDescription { get; set; }
        public double? Price { get; set; }
        public string? Unit { get; set; }
        public string? Category { get; set; }
        public int Amount { get; set; }
        [JsonConverter(typeof(DateOnlyConverter))]
        public DateTime? ExpiryDate { get; set; }
        public string? Status { get; set; }

        [JsonIgnore]
        public string StockStatus => Amount <= 5 ? "Krytyczny" : Amount <= 10 ? "Niski stan" : "OK";

        [JsonIgnore]
        public Brush StockBadgeBrush => Amount <= 5
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fee2e2"))
            : Amount <= 10
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fef3c7"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d1fae5"));

        [JsonIgnore]
        public Brush StockTextBrush => Amount <= 5
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991b1b"))
            : Amount <= 10
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400e"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065f46"));
    }
}