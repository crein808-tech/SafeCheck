using System.IO;

namespace SafeCheck.Services;

public static class LogService
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SafeCheck", "logs");

    public static void Log(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(LogDir);
            var file = Path.Combine(LogDir, $"safecheck-{DateTime.Now:yyyy-MM-dd}.log");
            var line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}\n";
            File.AppendAllText(file, line);
        }
        catch { }
    }

    public static void Error(string message) => Log("ERROR", message);
    public static void Info(string message) => Log("INFO", message);

    public static void CleanOldLogs(int keepDays = 30)
    {
        try
        {
            if (!Directory.Exists(LogDir)) return;
            foreach (var file in Directory.GetFiles(LogDir, "safecheck-*.log"))
                if (File.GetLastWriteTime(file) < DateTime.Now.AddDays(-keepDays))
                    File.Delete(file);
        }
        catch { }
    }
}
