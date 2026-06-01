using PokiePawsDesk.Core;
using PokiePawsDesk.Services;
using PokiePawsDesk.Tests.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PokiePawsDesk.Tests
{
    public class AuthHandlerTests
    {
        private static async Task<AuthService> CreateLoggedInAuthService(string token)
        {
            var json = $$"""{"accessToken":"{{token}}","refreshToken":"test-refresh"}""";
            var handler = new MessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var service = new AuthService(client);
            await service.LoginAsync("wh1@pokiepaws.pl", "Worker1234!");
            return service;
        }

        [Fact]
        public async Task SendAsync_AttachesBearerTokenToRequest()
        {
            HttpRequestMessage? captured = null;
            var authService = await CreateLoggedInAuthService("my-jwt");
            var inner = new MessageHandler(req =>
            {
                captured = req;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var client = new HttpClient(new AuthHandler(authService, inner))
            {
                BaseAddress = new Uri("http://localhost")
            };
            await client.GetAsync("/api/test");

            Assert.Equal("Bearer", captured?.Headers.Authorization?.Scheme);
            Assert.Equal("my-jwt", captured?.Headers.Authorization?.Parameter);
        }

        [Fact]
        public async Task SendAsync_On401_RefreshesTokenAndRetries()
        {
            var authCall = 0;
            var authHandler = new MessageHandler(_ =>
            {
                var json = authCall++ == 0
                    ? """{"accessToken":"old-jwt","refreshToken":"valid-refresh"}"""
                    : """{"accessToken":"new-jwt","refreshToken":"new-refresh"}""";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });
            var authClient = new HttpClient(authHandler) { BaseAddress = new Uri("http://localhost") };
            var authService = new AuthService(authClient);
            await authService.LoginAsync("wh1@pokiepaws.pl", "Worker1234!");

            var requestCount = 0;
            var inner = new MessageHandler(_ =>
            {
                requestCount++;
                return requestCount == 1
                    ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    : new HttpResponseMessage(HttpStatusCode.OK);
            });

            var client = new HttpClient(new AuthHandler(authService, inner))
            {
                BaseAddress = new Uri("http://localhost")
            };
            var response = await client.GetAsync("/api/test");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, requestCount);
        }

        [Fact]
        public async Task SendAsync_SuccessResponse_DoesNotRetry()
        {
            var authService = await CreateLoggedInAuthService("my-jwt");
            var requestCount = 0;
            var inner = new MessageHandler(_ =>
            {
                requestCount++;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var client = new HttpClient(new AuthHandler(authService, inner))
            {
                BaseAddress = new Uri("http://localhost")
            };
            await client.GetAsync("/api/test");

            Assert.Equal(1, requestCount);
        }
    }
}