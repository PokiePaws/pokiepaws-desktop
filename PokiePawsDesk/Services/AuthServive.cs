using CredentialManagement;
using PokiePawsDesk.Core;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PokiePawsDesk.Services
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:9090";
        private const string CredentialKey = "PokiePaws_JWT";

        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest { Email = email, Password = password };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/auth/login", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    SaveToken(result.Token);
                    AppLogger.LogInfo($"Zalogowano użytkownika: {email}");
                    return result;
                }

                AppLogger.LogWarning($"Nieudana próba logowania dla: {email}");
                return null;
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Błąd podczas logowania", ex);
                return null;
            }
        }

        public void SaveToken(string token)
        {
            var credential = new Credential
            {
                Target = CredentialKey,
                Username = "pokiepaws_user",
                Password = token,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer
            };
            credential.Save();
        }

        public string GetToken()
        {
            var credential = new Credential { Target = CredentialKey };
            if (credential.Load())
                return credential.Password;
            return null;
        }

        public void RemoveToken()
        {
            AppLogger.LogInfo("Użytkownik wylogowany");
            var credential = new Credential { Target = CredentialKey };
            credential.Delete();
        }
    }
}