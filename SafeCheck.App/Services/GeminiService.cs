using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SafeCheck.Services;

public static class GeminiService
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
        "gemini-2.5-flash",
        "gemini-2.0-flash",
        "gemini-1.5-flash"
    ];

    public static Task<string> AskAsync(string question)
        => AskAsync(question, null);

    public static async Task<string> AskAsync(string question, List<(string Role, string Text)>? history)
    {
        var key = ApiKeyService.Load().GeminiApiKey;
        if (string.IsNullOrWhiteSpace(key))
            return "AI assistant is not set up. Add a Gemini API key in Settings.";

        string lastError = "";

        foreach (var model in ModelNames)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

                var contents = new List<object>();
                if (history != null)
                {
                    foreach (var msg in history)
                    {
                        var role = msg.Role == "user" ? "user" : "model";
                        contents.Add(new { role, parts = new[] { new { text = msg.Text } } });
                    }
                }
                contents.Add(new { role = "user", parts = new[] { new { text = question } } });

                var body = new
                {
                    system_instruction = new { parts = new[] { new { text = SystemPrompt } } },
                    contents
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("x-goog-api-key", key);
                request.Content = content;
                var response = await Http.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    lastError = "Rate limited";
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    lastError = $"Gemini ({model}): {response.StatusCode}";
                    continue;
                }

                JObject result;
                try { result = JObject.Parse(responseText); }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    lastError = "Unexpected response format from Gemini";
                    continue;
                }
                var text = result["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                if (!string.IsNullOrEmpty(text)) return text;
                lastError = "Empty response from Gemini";
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }
        }

        var groqResult = await GroqService.AskAsync(question, history);
        if (!groqResult.StartsWith("Could not") && !groqResult.StartsWith("AI assistant"))
            return groqResult;

        return $"AI unavailable. Gemini: {lastError}. Groq: {groqResult}";
    }
}
