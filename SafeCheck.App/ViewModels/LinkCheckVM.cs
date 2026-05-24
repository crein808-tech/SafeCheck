using System.Windows.Input;
using SafeCheck.Models;
using SafeCheck.Services;

namespace SafeCheck.ViewModels;

public class LinkCheckVM : BaseViewModel
{
    private string _urlInput = "";
    public string UrlInput
    {
        get => _urlInput;
        set { SetProperty(ref _urlInput, value); OnPropertyChanged(nameof(CanScan)); }
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

    public bool CanScan => !IsLoading && !string.IsNullOrWhiteSpace(UrlInput);

    public ICommand ScanCommand { get; }

    public LinkCheckVM()
    {
        ScanCommand = new RelayCommand(async () => await ScanAsync(), () => CanScan);
    }

    private async Task ScanAsync()
    {
        if (string.IsNullOrWhiteSpace(UrlInput)) return;
        if (!ApiKeyService.HasAnyLinkKey())
        {
            Result = new ScanResult
            {
                Title = "Setup Required", Input = "",
                Risk = RiskLevel.Unknown,
                Summary = "No link-scanning API key configured.",
                Recommendations = { "Tap 'Back to Home' then 'Settings' to add a free API key.",
                                    "VirusTotal: virustotal.com (500 scans/day)",
                                    "CheckPhish: checkphish.bolster.ai (25 scans/day)" }
            };
            return;
        }
        IsLoading = true;
        Result = null;
        try
        {
            var url = UrlInput.Trim();
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                Result = new ScanResult
                {
                    Title = "Link Check", Input = url, Risk = RiskLevel.Unknown,
                    Summary = "That doesn't look like a valid URL. Check the address and try again."
                };
                return;
            }

            if (ApiKeyService.HasVirusTotalKey())
            {
                Result = await VirusTotalService.CheckUrlAsync(url);
            }
            else
            {
                var cpResult = await CheckPhishService.CheckUrlAsync(url);
                if (cpResult != null)
                {
                    cpResult.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("link", url);
                    cpResult.LookupLinks = FallbackSearchService.GetLookupLinks("link", url);
                    Result = cpResult;
                }
                else
                {
                    Result = new ScanResult { Title = "Link Scan", Input = url, Risk = RiskLevel.Unknown,
                        Summary = "Could not reach CheckPhish - asking AI instead..." };
                    await FallbackSearchService.EnrichWithAI(Result, "link", url);
                    Result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("link", url);
                    Result.LookupLinks = FallbackSearchService.GetLookupLinks("link", url);
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
