namespace PokiePawsDesk.Core
{
    public static class AppConfig
    {
#if DEBUG
        public const string ApiBaseUrl = "http://localhost:9090";
        public const string WebSocketUrl = "ws://localhost:9090/ws-native";
#else
        public const string ApiBaseUrl = "https://api.pokiepaws.pl";
        public const string WebSocketUrl = "wss://ws.pokiepaws.pl";
#endif
    }
}