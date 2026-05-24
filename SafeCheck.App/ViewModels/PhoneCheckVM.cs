using System.Windows.Input;
using SafeCheck.Models;
using SafeCheck.Services;

namespace SafeCheck.ViewModels;

public class PhoneCheckVM : BaseViewModel
{
    private string _phoneInput = "";
    public string PhoneInput
    {
        get => _phoneInput;
        set { SetProperty(ref _phoneInput, value); OnPropertyChanged(nameof(CanScan)); }
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

    public bool CanScan => !IsLoading && !string.IsNullOrWhiteSpace(PhoneInput);

    public ICommand ScanCommand { get; }

    public PhoneCheckVM()
    {
        ScanCommand = new RelayCommand(async () => await ScanAsync(), () => CanScan);
    }

    private async Task ScanAsync()
    {
        if (string.IsNullOrWhiteSpace(PhoneInput)) return;
        IsLoading = true;
        Result = null;
        try
        {
            var cleaned = new string(PhoneInput.Where(c => char.IsDigit(c) || c == '+').ToArray());
            var result = new ScanResult { Title = "Phone Number Check", Input = PhoneInput.Trim() };

            result.Risk = RiskLevel.Unknown;
            result.Summary = "Checking phone number with AI and lookup sites...";
            result.Findings.Add("Phone database lookup unavailable - using AI analysis");

            await FallbackSearchService.EnrichWithAI(result, "phone", PhoneInput.Trim());
            result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("phone", cleaned);
            result.LookupLinks = FallbackSearchService.GetLookupLinks("phone", cleaned);

            result.Recommendations.Add("Search Google and the lookup sites below for reports about this number");
            result.Recommendations.Add("If they asked for money or personal info, be cautious");

            Result = result;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
