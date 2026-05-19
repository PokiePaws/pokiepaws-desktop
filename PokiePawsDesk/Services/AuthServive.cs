using CredentialManagement;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PokiePawsDesk.Services
{
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        [JsonPropertyName("accessToken")]
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }

    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string CredentialKey = "PokiePaws_JWT";

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;

                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });
                if (!response.IsSuccessStatusCode) return null;

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Token == null) return null;

                SaveToken(result.Token);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

                return result;
            }
            catch
            {
                return null;
            }
        }

        public void SaveToken(string token)
        {
            new Credential
            {
                Target = CredentialKey,
                Username = "pokiepaws_user",
                Password = token,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer
            }.Save();
        }

        public string? GetToken()
        {
            var credential = new Credential { Target = CredentialKey };
            return credential.Load() ? credential.Password : null;
        }

        public void RestoreToken()
        {
            var token = GetToken();
            if (token != null)
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void RemoveToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            new Credential { Target = CredentialKey }.Delete();
        }
    }
}