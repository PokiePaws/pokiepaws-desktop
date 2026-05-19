using System.IO;
using System.Security.Cryptography;

namespace PokiePawsDesk.Core
{
    public static class DbKeyProvider
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PokiePaws");

        public static string DbPath => Path.Combine(AppDataPath, "pokiepaws.db");

        private static string KeyPath => Path.Combine(AppDataPath, "db.key");

        public static string GetOrCreateKey()
        {
            Directory.CreateDirectory(AppDataPath);

            byte[] key;

            if (File.Exists(KeyPath))
            {
                var encrypted = File.ReadAllBytes(KeyPath);
                key = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            }
            else
            {
                key = RandomNumberGenerator.GetBytes(32);
                var encrypted = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(KeyPath, encrypted);
            }

            return Convert.ToHexString(key);
        }
    }
}