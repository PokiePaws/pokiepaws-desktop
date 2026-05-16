using System;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace PokiePawsDesk.Models
{
    public class Order
    {
        public long Id { get; set; }
        public long ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public string? Name { get; set; }
        public int Amount { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? Unit { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [JsonIgnore]
        public string StatusLabel => Status switch
        {
            "PENDING" => "Oczekujące",
            "IN_PROGRESS" => "W trakcie",
            "SHIPPED" => "Wysłane",
            "DELIVERED" => "Dostarczone",
            "REJECTED" => "Odrzucone",
            _ => Status ?? "—"
        };

        [JsonIgnore]
        public Brush StatusBadgeBrush => Status switch
        {
            "PENDING" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fef3c7")),
            "IN_PROGRESS" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dbeafe")),
            "SHIPPED" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ede9fe")),
            "DELIVERED" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d1fae5")),
            "REJECTED" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fee2e2")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f5f5f4"))
        };

        [JsonIgnore]
        public Brush StatusTextBrush => Status switch
        {
            "PENDING" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400e")),
            "IN_PROGRESS" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e40af")),
            "SHIPPED" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5b21b6")),
            "DELIVERED" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065f46")),
            "REJECTED" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991b1b")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#44403c"))
        };

        [JsonIgnore]
        public Visibility PendingActionsVisibility =>
            Status == "PENDING" ? Visibility.Visible : Visibility.Collapsed;

        [JsonIgnore]
        public Visibility InProgressActionsVisibility =>
            Status == "IN_PROGRESS" ? Visibility.Visible : Visibility.Collapsed;

        [JsonIgnore]
        public Visibility ShippedActionsVisibility =>
            Status == "SHIPPED" ? Visibility.Visible : Visibility.Collapsed;
    }
}