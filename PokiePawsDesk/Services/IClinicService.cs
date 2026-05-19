using PokiePawsDesk.Models;

namespace PokiePawsDesk.Services
{
    public interface IClinicService
    {
        Task<List<Clinic>> GetClinicsAsync();
        List<Clinic> GetLocalClinics();
    }
}