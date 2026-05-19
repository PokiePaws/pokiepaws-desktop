using System.Text.Json.Serialization;
using System.Windows.Media;
using PokiePawsDesk.Services;

namespace PokiePawsDesk.Models
{
    public class Clinic
    {
        public long Id { get; set; }
        public string? ClinicName { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? WorkingHours { get; set; }
        public bool Active { get; set; }

        [JsonIgnore]
        public string StatusText => Active ? LanguageService.Get("Clinic_Active") : LanguageService.Get("Clinic_Suspended");

        [JsonIgnore]
        public Brush StatusBadgeBrush => Active
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d1fae5"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fee2e2"));

        [JsonIgnore]
        public Brush StatusTextBrush => Active
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065f46"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991b1b"));
    }
}