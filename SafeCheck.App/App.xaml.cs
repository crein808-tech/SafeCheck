using System.Windows;
using SafeCheck.Services;

namespace SafeCheck;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        LogService.CleanOldLogs();
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            LogService.Error($"Unhandled: {(args.ExceptionObject as Exception)?.GetType().Name}");
        DispatcherUnhandledException += (_, args) =>
        {
            LogService.Error($"UI: {args.Exception.GetType().Name}");
            args.Handled = true;
        };
    }
}
