namespace PokiePawsDesk.Core
{
    public static class AppConfig
    {
#if DEBUG
        public const string ApiBaseUrl = "http://localhost:9090";
        public const string WebSocketUrl = "ws://localhost:9090/ws";
#else
        public const string ApiBaseUrl = "https://api.pokiepaws.pl";
        public const string WebSocketUrl = "wss://api.pokiepaws.pl/ws";
#endif
    }
}