namespace PokiePawsDesk.Models
{
    public class Clinic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string VetName { get; set; }
        public bool IsActive { get; set; }

        public string StatusText => IsActive ? "Aktywny" : "Zawieszony";
        public string StatusColor => IsActive ? "#2E7D32" : "#B71C1C";
    }
}