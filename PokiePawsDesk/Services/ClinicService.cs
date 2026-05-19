using PokiePawsDesk.Core;
using PokiePawsDesk.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PokiePawsDesk.Services
{
    public class ClinicService : IClinicService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;

        public ClinicService(HttpClient httpClient, AppDbContext db)
        {
            _httpClient = httpClient;
            _db = db;
        }

        public async Task<List<Clinic>> GetClinicsAsync()
        {
            try
            {
                var clinics = await _httpClient.GetFromJsonAsync<List<Clinic>>("/api/clinics");
                if (clinics != null)
                {
                    await SyncToLocalAsync(clinics);
                    return clinics;
                }
            }
            catch { }

            return _db.Clinics.ToList();
        }

        public List<Clinic> GetLocalClinics()
        {
            return _db.Clinics.ToList();
        }

        private async Task SyncToLocalAsync(List<Clinic> clinics)
        {
            _db.Clinics.RemoveRange(_db.Clinics);
            _db.Clinics.AddRange(clinics);
            await _db.SaveChangesAsync();
        }
    }
}