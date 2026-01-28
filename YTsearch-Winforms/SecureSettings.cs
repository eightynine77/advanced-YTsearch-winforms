using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YTsearch_Winforms
{
    public static class SecureSettings
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.dat");

        public static void SaveApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(apiKey);
                // Encrypt data using the current user's credentials
                byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(FilePath, encrypted);
            }
            catch (Exception ex)
            {
                // In production, log this error
                System.Diagnostics.Debug.WriteLine("Encryption failed: " + ex.Message);
            }
        }

        public static string LoadApiKey()
        {
            if (!File.Exists(FilePath)) return null;

            try
            {
                byte[] encrypted = File.ReadAllBytes(FilePath);
                // Decrypt data
                byte[] data = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return null; // Return null if decryption fails (e.g. moved to different PC)
            }
        }
    }
}