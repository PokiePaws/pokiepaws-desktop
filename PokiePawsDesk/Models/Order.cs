namespace PokiePawsDesk.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; }
    }
}