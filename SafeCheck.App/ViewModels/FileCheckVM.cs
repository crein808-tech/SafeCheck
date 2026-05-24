using System.Windows.Input;
using SafeCheck.Models;
using SafeCheck.Services;

namespace SafeCheck.ViewModels;

public class FileCheckVM : BaseViewModel
{
    private string _filePath = "";
    public string FilePath
    {
        get => _filePath;
        set { SetProperty(ref _filePath, value); OnPropertyChanged(nameof(CanScan)); }
    }

    private string _statusText = "";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { SetProperty(ref _isLoading, value); OnPropertyChanged(nameof(CanScan)); }
    }

    private ScanResult? _result;
    public ScanResult? Result
    {
        get => _result;
        set => SetProperty(ref _result, value);
    }

    public bool CanScan => !IsLoading && !string.IsNullOrWhiteSpace(FilePath);

    public ICommand ScanCommand { get; }

    public FileCheckVM()
    {
        ScanCommand = new RelayCommand(async () => await ScanAsync(), () => CanScan);
    }

    private async Task ScanAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath)) return;
        if (!ApiKeyService.HasVirusTotalKey())
        {
            Result = new ScanResult
            {
                Title = "Setup Required", Input = "",
                Risk = RiskLevel.Unknown,
                Summary = "VirusTotal API key not configured.",
                Recommendations = { "Tap 'Back to Home' then 'Settings' to add your free VirusTotal API key.",
                                    "Get a free key at virustotal.com (500 scans/day)." }
            };
            return;
        }
        if (!System.IO.File.Exists(FilePath))
        {
            StatusText = "File not found. Check the path and try again.";
            return;
        }

        IsLoading = true;
        Result = null;
        StatusText = "";
        try
        {
            var progress = new Progress<string>(s => StatusText = s);
            Result = await VirusTotalService.CheckFileAsync(FilePath.Trim(), progress);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
