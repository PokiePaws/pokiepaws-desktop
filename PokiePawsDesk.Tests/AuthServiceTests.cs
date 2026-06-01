using PokiePawsDesk.Services;
using PokiePawsDesk.Tests.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PokiePawsDesk.Tests
{
    public class AuthServiceTests
    {
        private static HttpClient MakeClient(string json, HttpStatusCode status = HttpStatusCode.OK)
        {
            var handler = new MessageHandler(_ =>
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            return new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsTokenInResponse()
        {
            var service = new AuthService(MakeClient(
                """{"accessToken":"test-jwt","refreshToken":"test-refresh","email":"wh1@pokiepaws.pl"}"""));

            var result = await service.LoginAsync("wh1@pokiepaws.pl", "Worker1234!");

            Assert.NotNull(result);
            Assert.Equal("test-jwt", result.Token);
        }

        [Fact]
        public async Task LoginAsync_Unauthorized_ThrowsException()
        {
            var service = new AuthService(MakeClient("Unauthorized", HttpStatusCode.Unauthorized));

            await Assert.ThrowsAsync<Exception>(() => service.LoginAsync("bad@test.com", "wrong"));
        }

        [Fact]
        public async Task GetToken_AfterLogin_ReturnsCachedToken()
        {
            var service = new AuthService(MakeClient(
                """{"accessToken":"cached-jwt","refreshToken":"refresh"}"""));

            await service.LoginAsync("wh1@pokiepaws.pl", "Worker1234!");

            Assert.Equal("cached-jwt", service.GetToken());
        }

        [Fact]
        public async Task RefreshTokenAsync_ValidRefreshToken_ReturnsNewAccessToken()
        {
            var call = 0;
            var handler = new MessageHandler(_ =>
            {
                var json = call++ == 0
                    ? """{"accessToken":"old-jwt","refreshToken":"valid-refresh"}"""
                    : """{"accessToken":"new-jwt","refreshToken":"new-refresh"}""";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new AuthService(client);
            await service.LoginAsync("wh1@pokiepaws.pl", "Worker1234!");

            var newToken = await service.RefreshTokenAsync();

            Assert.Equal("new-jwt", newToken);
        }

        [Fact]
        public async Task RemoveToken_AfterLogin_GetTokenReturnsNull()
        {
            var service = new AuthService(MakeClient(
                """{"accessToken":"to-remove","refreshToken":"refresh"}"""));
            await service.LoginAsync("wh1@pokiepaws.pl", "Worker1234!");

            service.RemoveToken();

            Assert.Null(service.GetToken());
        }
    }
}