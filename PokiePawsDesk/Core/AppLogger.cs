using Serilog;

namespace PokiePawsDesk.Core
{
    public static class AppLogger
    {
        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.File(
                    path: "logs/pokiepaws-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();
        }

        public static void LogInfo(string message) =>
            Log.Information(message);

        public static void LogWarning(string message) =>
            Log.Warning(message);

        public static void LogError(string message, Exception? ex = null) =>
            Log.Error(ex, message);

        public static void Shutdown() =>
            Log.CloseAndFlush();
    }
}