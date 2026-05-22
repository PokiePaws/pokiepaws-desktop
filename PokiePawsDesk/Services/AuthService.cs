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

        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        public string? Email { get; set; }
        public string? Role { get; set; }
    }

    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string AccessTokenKey = "PokiePaws_JWT";
        private const string RefreshTokenKey = "PokiePaws_Refresh";

        private string? _accessTokenCache;
        private string? _refreshTokenCache;

        public event Action? SessionExpired;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login",
                    new LoginRequest { Email = email, Password = password });
                if (!response.IsSuccessStatusCode) return null;

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Token == null) return null;

                _accessTokenCache = result.Token;
                _refreshTokenCache = result.RefreshToken;

                try
                {
                    SaveCredential(AccessTokenKey, result.Token);
                    if (result.RefreshToken != null)
                        SaveCredential(RefreshTokenKey, result.RefreshToken);
                }
                catch { }

                return result;
            }
            catch { return null; }
        }

        public async Task<string?> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = _refreshTokenCache ?? GetCredential(RefreshTokenKey);
                if (refreshToken == null) return null;

                var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh",
                    new { refreshToken });
                if (!response.IsSuccessStatusCode) return null;

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Token == null) return null;

                _accessTokenCache = result.Token;
                _refreshTokenCache = result.RefreshToken;

                try
                {
                    SaveCredential(AccessTokenKey, result.Token);
                    if (result.RefreshToken != null)
                        SaveCredential(RefreshTokenKey, result.RefreshToken);
                }
                catch { }

                return result.Token;
            }
            catch { return null; }
        }

        public void RaiseSessionExpired() => SessionExpired?.Invoke();

        public string? GetToken() => _accessTokenCache ?? GetCredential(AccessTokenKey);

        public void RemoveToken()
        {
            _accessTokenCache = null;
            _refreshTokenCache = null;
            try { new Credential { Target = AccessTokenKey }.Delete(); } catch { }
            try { new Credential { Target = RefreshTokenKey }.Delete(); } catch { }
        }

        private static void SaveCredential(string key, string value)
        {
            new Credential
            {
                Target = key,
                Username = "pokiepaws_user",
                Password = value,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer
            }.Save();
        }

        private static string? GetCredential(string key)
        {
            var credential = new Credential { Target = key };
            return credential.Load() ? credential.Password : null;
        }
    }
}