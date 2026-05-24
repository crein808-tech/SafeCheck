using System.Collections.ObjectModel;
using System.Windows.Input;
using SafeCheck.Services;

namespace SafeCheck.ViewModels;

public class ChatMessage
{
    public string Role { get; set; } = "";
    public string Text { get; set; } = "";
    public bool IsUser => Role == "user";
}

public class AskAIVM : BaseViewModel
{
    private string _question = "";
    public string Question
    {
        get => _question;
        set { SetProperty(ref _question, value); OnPropertyChanged(nameof(CanAsk)); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { SetProperty(ref _isLoading, value); OnPropertyChanged(nameof(CanAsk)); }
    }

    public bool CanAsk => !IsLoading && !string.IsNullOrWhiteSpace(Question);

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ICommand AskCommand { get; }

    public AskAIVM()
    {
        AskCommand = new RelayCommand(async () => await AskAsync(), () => CanAsk);
        Messages.Add(new ChatMessage
        {
            Role = "assistant",
            Text = "Hi! I'm SafeCheck AI. Ask me anything about online safety — suspicious emails, " +
                   "scam calls, unsafe websites, or anything else you're unsure about."
        });
    }

    private async Task AskAsync()
    {
        if (string.IsNullOrWhiteSpace(Question)) return;
        if (!ApiKeyService.HasAnyAiKey())
        {
            Messages.Add(new ChatMessage { Role = "user", Text = Question.Trim() });
            Messages.Add(new ChatMessage
            {
                Role = "assistant",
                Text = "AI is not set up yet. Go to Home then Settings and add a free Gemini or Groq API key."
            });
            Question = "";
            return;
        }
        var q = Question.Trim();
        Question = "";

        var history = Messages
            .Where(m => m.Role is "user" or "assistant")
            .Skip(1)
            .Select(m => (m.Role, m.Text))
            .ToList();

        Messages.Add(new ChatMessage { Role = "user", Text = q });
        IsLoading = true;
        try
        {
            var answer = await GeminiService.AskAsync(q, history);
            Messages.Add(new ChatMessage { Role = "assistant", Text = answer });
        }
        finally
        {
            IsLoading = false;
        }
    }
}
