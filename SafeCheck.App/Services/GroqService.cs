using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SafeCheck.Services;

public static class GroqService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private const string SystemPrompt =
        "You are SafeCheck AI, a friendly security assistant helping non-technical people stay safe online. " +
        "Explain things simply — no jargon, no technical terms. Use short sentences. " +
        "If something is dangerous, say so clearly and tell them exactly what to do. " +
        "If something is safe, reassure them but remind them to stay careful. " +
        "Keep answers under 150 words. " +
        "IMPORTANT: You cannot search the internet or look up phone numbers, emails, or websites. " +
        "If someone asks you to check a specific phone number, email, or link, tell them: " +
        "'I can't look things up, but go back to the home screen and use Check Phone Number, Check Email, or Check Link — those tools will search scam databases and Google for you.' " +
        "You CAN give general safety advice about scam tactics, what to watch for, and what to do.";

    private static readonly string[] ModelNames = [
        "llama-3.3-70b-versatile",
        "llama-3.1-8b-instant",
        "meta-llama/llama-4-scout-17b-16e-instruct"
    ];

    public static Task<string> AskAsync(string question)
        => AskAsync(question, null);

    public static async Task<string> AskAsync(string question, List<(string Role, string Text)>? history)
    {
        var key = ApiKeyService.Load().GroqApiKey;
        if (string.IsNullOrWhiteSpace(key))
            return "AI assistant is not available — no Groq API key configured.";

        string lastError = "";

        foreach (var model in ModelNames)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
                request.Headers.Add("Authorization", $"Bearer {key}");

                var messages = new List<object> { new { role = "system", content = SystemPrompt } };
                if (history != null)
                {
                    foreach (var msg in history)
                        messages.Add(new { role = msg.Role == "user" ? "user" : "assistant", content = msg.Text });
                }
                messages.Add(new { role = "user", content = question });

                var body = new
                {
                    model,
                    messages,
                    max_tokens = 300
                };

                request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await Http.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    lastError = $"Groq ({model}): {response.StatusCode}";
                    continue;
                }

                JObject result;
                try { result = JObject.Parse(responseText); }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    lastError = "Unexpected response format from Groq";
                    continue;
                }
                var text = result["choices"]?[0]?["message"]?["content"]?.ToString();
                if (!string.IsNullOrEmpty(text)) return text;
                lastError = "Empty response from Groq";
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }
        }

        return $"Could not reach AI assistant. Last error: {lastError}";
    }
}
