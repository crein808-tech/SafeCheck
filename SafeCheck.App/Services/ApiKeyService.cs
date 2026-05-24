using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using SafeCheck.Models;

namespace SafeCheck.Services;

public static class ApiKeyService
{
    private const string DefaultVirusTotalKey = "";
    private const string DefaultGeminiKey     = "";
    private const string DefaultGroqKey       = "";
    private const string DefaultCheckPhishKey = "";

    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SafeCheck");
    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.dat");

    private static AppConfig? _cached;

    public static AppConfig Load()
    {
        if (_cached != null) return _cached;

        if (File.Exists(ConfigPath))
        {
            try
            {
                var encrypted = File.ReadAllBytes(ConfigPath);
                var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                var json = Encoding.UTF8.GetString(decrypted);
                _cached = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                if (HasAnyKey(_cached)) return _cached;
            }
            catch (Exception) { }
        }

        _cached = new AppConfig
        {
            VirusTotalApiKey = DefaultVirusTotalKey,
            GeminiApiKey = DefaultGeminiKey,
            GroqApiKey = DefaultGroqKey,
            CheckPhishApiKey = DefaultCheckPhishKey
        };
        return _cached;
    }

    private static bool HasAnyKey(AppConfig c) =>
        !string.IsNullOrWhiteSpace(c.VirusTotalApiKey) ||
        !string.IsNullOrWhiteSpace(c.GeminiApiKey) ||
        !string.IsNullOrWhiteSpace(c.GroqApiKey) ||
        !string.IsNullOrWhiteSpace(c.CheckPhishApiKey);

    public static void Save(AppConfig config)
    {
        _cached = config;
        Directory.CreateDirectory(ConfigDir);
        var json = JsonConvert.SerializeObject(config);
        var bytes = Encoding.UTF8.GetBytes(json);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(ConfigPath, encrypted);
    }

    public static bool HasVirusTotalKey() => !string.IsNullOrWhiteSpace(Load().VirusTotalApiKey);
    public static bool HasGeminiKey() => !string.IsNullOrWhiteSpace(Load().GeminiApiKey);
    public static bool HasGroqKey() => !string.IsNullOrWhiteSpace(Load().GroqApiKey);
    public static bool HasCheckPhishKey() => !string.IsNullOrWhiteSpace(Load().CheckPhishApiKey);
    public static bool HasAnyAiKey() => HasGeminiKey() || HasGroqKey();
    public static bool HasAnyLinkKey() => HasVirusTotalKey() || HasCheckPhishKey();
}
