using PokiePawsDesk.Core;
using PokiePawsDesk.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PokiePawsDesk.Services
{
    public class ClinicService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;
        private const string BaseUrl = "http://localhost:9090";

        public ClinicService(HttpClient httpClient, AppDbContext db)
        {
            _httpClient = httpClient;
            _db = db;
        }

        public async Task<List<Clinic>> GetClinicsAsync()
        {
            try
            {
                var clinics = await _httpClient.GetFromJsonAsync<List<Clinic>>($"{BaseUrl}/api/clinics");
                if (clinics != null)
                {
                    await SyncToLocalAsync(clinics);
                    AppLogger.LogInfo("Gabinety pobrane z API i zsynchronizowane lokalnie");
                    return clinics;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Brak połączenia z API, tryb offline: {ex.Message}");
            }

            AppLogger.LogInfo("Gabinety pobrane z lokalnej bazy");
            return await GetClinicsFromLocalAsync();
        }

        public async Task<List<Order>> GetClinicOrdersAsync(int clinicId)
        {
            try
            {
                var orders = await _httpClient.GetFromJsonAsync<List<Order>>($"{BaseUrl}/api/clinics/{clinicId}/orders");
                if (orders != null)
                {
                    AppLogger.LogInfo($"Zamówienia gabinetu {clinicId} pobrane z API");
                    return orders;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Brak połączenia z API dla zamówień gabinetu {clinicId}: {ex.Message}");
            }

            return _db.Orders.Where(o => o.ClinicId == clinicId).ToList();
        }

        private async Task SyncToLocalAsync(List<Clinic> clinics)
        {
            _db.Clinics.RemoveRange(_db.Clinics);
            _db.Clinics.AddRange(clinics);
            await _db.SaveChangesAsync();
        }

        private Task<List<Clinic>> GetClinicsFromLocalAsync()
        {
            return Task.FromResult(_db.Clinics.ToList());
        }
    }
}