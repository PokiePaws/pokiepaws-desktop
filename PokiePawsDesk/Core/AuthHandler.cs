using PokiePawsDesk.Services;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PokiePawsDesk.Core
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly AuthService _authService;

        public AuthHandler(AuthService authService, HttpMessageHandler? inner = null)
            : base(inner ?? new HttpClientHandler())
        {
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _authService.GetToken();
            if (token != null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            byte[]? contentBytes = null;
            string? contentType = null;
            if (request.Content != null)
            {
                contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                contentType = request.Content.Headers.ContentType?.ToString();
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            var newToken = await _authService.RefreshTokenAsync();
            if (newToken == null)
            {
                _authService.RaiseSessionExpired();
                return response;
            }

            var retry = new HttpRequestMessage(request.Method, request.RequestUri);
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

            foreach (var header in request.Headers)
            {
                if (header.Key != "Authorization")
                    retry.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (contentBytes != null)
            {
                retry.Content = new ByteArrayContent(contentBytes);
                if (contentType != null)
                    retry.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
            }

            return await base.SendAsync(retry, cancellationToken);
        }
    }
}