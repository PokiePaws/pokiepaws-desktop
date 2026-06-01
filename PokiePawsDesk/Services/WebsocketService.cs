using PokiePawsDesk.Core;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PokiePawsDesk.Services
{
    public class WebSocketService
    {
        private ClientWebSocket? _ws;
        private CancellationTokenSource _cts = new();

        public event Action<string>? OnNewOrder;
        public event Action? OnConnected;
        public event Action? OnDisconnected;

        private readonly string _url;
        private readonly Func<string?> _getToken;

        private static readonly int[] BackoffSeconds = { 2, 4, 8, 16, 32, 60 };

        public WebSocketService(string url, Func<string?> getToken)
        {
            _url = url;
            _getToken = getToken;
        }

        public async Task ConnectAsync()
        {
            _cts = new CancellationTokenSource();
            _ = ConnectLoopAsync();
            await Task.CompletedTask;
        }

        private async Task ConnectLoopAsync()
        {
            int attempt = 0;

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var token = _getToken();
                    if (token == null) break;

                    _ws = new ClientWebSocket();
                    _ws.Options.SetRequestHeader("Authorization", $"Bearer {token}");
                    await _ws.ConnectAsync(new Uri(_url), _cts.Token);
                    attempt = 0;

                    await SendFrameAsync("CONNECT", new[]
                    {
                        "accept-version:1.2",
                        "heart-beat:0,0",
                        $"Authorization:Bearer {token}",
                    });

                    await ReceiveLoopAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning($"WebSocket error: {ex.Message}");
                }

                if (_cts.Token.IsCancellationRequested)
                    break;

                OnDisconnected?.Invoke();

                int delay = BackoffSeconds[Math.Min(attempt, BackoffSeconds.Length - 1)];
                attempt++;

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(delay), _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public async Task DisconnectAsync()
        {
            await _cts.CancelAsync();
            if (_ws?.State == WebSocketState.Open)
            {
                try
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                catch { }
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[65536];
            var builder = new StringBuilder();

            while (_ws?.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning($"WebSocket receive error: {ex.Message}");
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                if (!result.EndOfMessage)
                    continue;

                var frame = builder.ToString();
                builder.Clear();

                if (!string.IsNullOrWhiteSpace(frame))
                    HandleFrame(frame);
            }
        }

        private void HandleFrame(string frame)
        {
            var trimmed = frame.TrimStart('\n', '\r');

            if (trimmed.StartsWith("CONNECTED", StringComparison.Ordinal))
            {
                OnConnected?.Invoke();
                _ = SendFrameAsync("SUBSCRIBE", new[]
                {
                    "id:sub-orders",
                    "destination:/topic/orders",
                });
            }
            else if (trimmed.StartsWith("MESSAGE", StringComparison.Ordinal))
            {
                var sep = trimmed.IndexOf("\n\n", StringComparison.Ordinal);
                if (sep < 0)
                    sep = trimmed.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                if (sep < 0) return;

                var body = sep + 2 <= trimmed.Length
                    ? trimmed.Substring(sep + 2).TrimEnd('\0', '\n', '\r')
                    : string.Empty;

                if (!string.IsNullOrWhiteSpace(body))
                    OnNewOrder?.Invoke(body);
            }
        }

        private async Task SendFrameAsync(string command, string[] headers, string body = "")
        {
            if (_ws?.State != WebSocketState.Open) return;
            var frame = command + "\n" + string.Join("\n", headers) + "\n\n" + body + "\0";
            var bytes = Encoding.UTF8.GetBytes(frame);
            try
            {
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"WebSocket send error: {ex.Message}");
            }
        }
    }
}