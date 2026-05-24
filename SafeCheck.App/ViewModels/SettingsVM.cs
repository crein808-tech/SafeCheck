using System.Windows.Input;
using SafeCheck.Services;

namespace SafeCheck.ViewModels;

public class SettingsVM : BaseViewModel
{
    private string _vtKey = "";
    public string VtKey
    {
        get => _vtKey;
        set => SetProperty(ref _vtKey, value);
    }

    private string _geminiKey = "";
    public string GeminiKey
    {
        get => _geminiKey;
        set => SetProperty(ref _geminiKey, value);
    }

    private string _groqKey = "";
    public string GroqKey
    {
        get => _groqKey;
        set => SetProperty(ref _groqKey, value);
    }

    private string _checkPhishKey = "";
    public string CheckPhishKey
    {
        get => _checkPhishKey;
        set => SetProperty(ref _checkPhishKey, value);
    }

    private string _statusText = "";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public ICommand SaveCommand { get; }

    public SettingsVM()
    {
        SaveCommand = new RelayCommand(Save);
        var config = ApiKeyService.Load();
        _vtKey = config.VirusTotalApiKey;
        _geminiKey = config.GeminiApiKey;
        _groqKey = config.GroqApiKey;
        _checkPhishKey = config.CheckPhishApiKey;
    }

    private void Save()
    {
        var config = new Models.AppConfig
        {
            VirusTotalApiKey = VtKey.Trim(),
            GeminiApiKey = GeminiKey.Trim(),
            GroqApiKey = GroqKey.Trim(),
            CheckPhishApiKey = CheckPhishKey.Trim()
        };
        ApiKeyService.Save(config);
        StatusText = "Settings saved!";
    }
}
