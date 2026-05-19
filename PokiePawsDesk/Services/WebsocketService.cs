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
        private readonly string _token;

        private static readonly int[] BackoffSeconds = { 2, 4, 8, 16, 32, 60 };

        public WebSocketService(string url, string token)
        {
            _url = url;
            _token = token;
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
                    _ws = new ClientWebSocket();
                    await _ws.ConnectAsync(new Uri(_url), _cts.Token);
                    attempt = 0;

                    await SendFrameAsync("CONNECT", new[]
                    {
                        "accept-version:1.2",
                        "heart-beat:0,0",
                        $"Authorization:Bearer {_token}"
                    });

                    await ReceiveLoopAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
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
            _cts.Cancel();
            if (_ws?.State == WebSocketState.Open)
            {
                try
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                catch { }
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[8192];
            var builder = new StringBuilder();

            while (_ws?.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                if (!result.EndOfMessage)
                    continue;

                HandleFrame(builder.ToString());
                builder.Clear();
            }
        }

        private void HandleFrame(string frame)
        {
            if (frame.StartsWith("CONNECTED"))
            {
                OnConnected?.Invoke();
                _ = SendFrameAsync("SUBSCRIBE", new[]
                {
                    "id:sub-orders",
                    "destination:/topic/orders"
                });
            }
            else if (frame.StartsWith("MESSAGE"))
            {
                var sep = frame.IndexOf("\n\n");
                if (sep < 0) return;
                var body = frame.Substring(sep + 2).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(body))
                    OnNewOrder?.Invoke(body);
            }
        }

        private async Task SendFrameAsync(string command, string[] headers, string body = "")
        {
            if (_ws?.State != WebSocketState.Open) return;
            var frame = command + "\n" + string.Join("\n", headers) + "\n\n" + body + "\0";
            var bytes = Encoding.UTF8.GetBytes(frame);
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
        }
    }
}