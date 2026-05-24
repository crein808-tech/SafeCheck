using SafeCheck.Models;

namespace SafeCheck.Services;

public static class FallbackSearchService
{
    public static async Task EnrichWithAI(ScanResult result, string type, string input)
    {
        if (!ApiKeyService.HasAnyAiKey())
        {
            result.Findings.Add("AI analysis unavailable - add a Gemini or Groq API key in Settings.");
            return;
        }

        var prompt = type switch
        {
            "phone" => $"Someone received a call from {input}. Is this phone number associated with scams, telemarketers, or fraud? What should they do? Be specific and keep it under 100 words.",
            "email" => $"Someone received an email from {input}. Is this email address suspicious? Is the domain known for phishing or scams? What should they do? Be specific and keep it under 100 words.",
            "link"  => $"Someone wants to visit this website: {input}. Is this URL safe? Is the domain known for phishing, malware, or scams? What should they do? Be specific and keep it under 100 words.",
            "file" => $"Someone downloaded a file named \"{input}\". Is this file name associated with malware, viruses, or scams? What should they do? Be specific and keep it under 100 words.",
            _ => $"Is this safe? {input}. Keep it under 100 words."
        };

        try
        {
            var aiResponse = await GeminiService.AskAsync(prompt);
            result.Findings.Add("AI Analysis: " + aiResponse);
        }
        catch (Exception ex)
        {
            result.Findings.Add("AI analysis unavailable: " + ex.Message);
        }
    }

    public static string GetGoogleSearchUrl(string type, string input)
    {
        var query = type switch
        {
            "phone" => $"{input} scam OR spam OR fraud OR telemarketer",
            "email" => $"\"{input}\" scam OR phishing OR spam OR fraud",
            "link"  => $"\"{input}\" scam OR phishing OR malware OR unsafe",
            "file" => $"\"{input}\" malware OR virus OR trojan OR scam",
            _ => $"{input} scam"
        };
        return $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";
    }

    public static List<LookupLink> GetLookupLinks(string type, string input)
    {
        var cleaned = new string(input.Where(c => char.IsDigit(c) || c == '+').ToArray());
        return type switch
        {
            "phone" =>
            [
                new() { Label = "Nomorobo Lookup", Url = $"https://www.nomorobo.com/lookup/{Uri.EscapeDataString(cleaned)}" },
                new() { Label = "WhoCalledMe", Url = $"https://www.whocalledme.com/phone-number/{Uri.EscapeDataString(cleaned)}" },
                new() { Label = "Should I Answer", Url = $"https://www.shouldianswer.com/phone-number/{Uri.EscapeDataString(cleaned)}" },
                new() { Label = "SpamCalls.net", Url = $"https://spamcalls.net/en/number/{Uri.EscapeDataString(cleaned)}" },
            ],
            "email" =>
            [
                new() { Label = "IPQS Email Check", Url = $"https://www.ipqualityscore.com/free-email-verifier/lookup/{Uri.EscapeDataString(input)}" },
                new() { Label = "CleanTalk Check", Url = $"https://cleantalk.org/email-checker/{Uri.EscapeDataString(input)}" },
            ],
            "link" =>
            [
                new() { Label = "CheckPhish Scan", Url = $"https://checkphish.bolster.ai/?url={Uri.EscapeDataString(input)}" },
                new() { Label = "URLVoid Scan", Url = $"https://www.urlvoid.com/scan/{input.Replace("https://", "").Replace("http://", "").TrimEnd('/')}" },
                new() { Label = "Google Safe Browsing", Url = $"https://transparencyreport.google.com/safe-browsing/search?url={Uri.EscapeDataString(input)}" },
                new() { Label = "Sucuri SiteCheck", Url = $"https://sitecheck.sucuri.net/results/{input.Replace("https://", "").Replace("http://", "")}" },
            ],
            "file" =>
            [
                new() { Label = "VirusTotal Web", Url = $"https://www.virustotal.com/gui/search/{Uri.EscapeDataString(input)}" },
            ],
            _ => []
        };
    }
}
