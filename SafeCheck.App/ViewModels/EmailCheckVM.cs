using System.Windows.Input;
using SafeCheck.Models;
using SafeCheck.Services;

namespace SafeCheck.ViewModels;

public class EmailCheckVM : BaseViewModel
{
    private string _emailInput = "";
    public string EmailInput
    {
        get => _emailInput;
        set { SetProperty(ref _emailInput, value); OnPropertyChanged(nameof(CanScan)); }
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

    public bool CanScan => !IsLoading && !string.IsNullOrWhiteSpace(EmailInput);

    public ICommand ScanCommand { get; }

    public EmailCheckVM()
    {
        ScanCommand = new RelayCommand(async () => await ScanAsync(), () => CanScan);
    }

    private async Task ScanAsync()
    {
        if (string.IsNullOrWhiteSpace(EmailInput)) return;
        IsLoading = true;
        Result = null;
        try
        {
            Result = await EmailRepService.CheckAsync(EmailInput.Trim());
        }
        finally
        {
            IsLoading = false;
        }
    }
}
