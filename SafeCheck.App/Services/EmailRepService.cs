using System.Net.Http;
using Newtonsoft.Json.Linq;
using SafeCheck.Models;

namespace SafeCheck.Services;

public static class EmailRepService
{
    private static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri("https://emailrep.io/"),
        Timeout = TimeSpan.FromSeconds(15)
    };

    static EmailRepService()
    {
        Http.DefaultRequestHeaders.Add("User-Agent", "SafeCheck/1.0");
    }

    public static async Task<ScanResult> CheckAsync(string email)
    {
        var result = new ScanResult { Title = "Email Check", Input = email };

        try
        {
            var httpResponse = await Http.GetAsync(email);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var msg = httpResponse.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                    ? "EmailRep is rate-limiting requests - asking AI instead..."
                    : "Not found in email databases - asking AI for more info...";
                result.Risk = RiskLevel.Unknown;
                result.Summary = msg;
                result.Findings.Add("Email not found in reputation databases");
                await FallbackSearchService.EnrichWithAI(result, "email", email);
                result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("email", email);
                result.LookupLinks = FallbackSearchService.GetLookupLinks("email", email);
                result.Recommendations.Add("This doesn't mean it's unsafe — just that it hasn't been reported");
                result.Recommendations.Add("Be cautious if the email asks for money or personal info");
                return result;
            }
            var response = await httpResponse.Content.ReadAsStringAsync();
            JObject data;
            try { data = JObject.Parse(response); }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                result.Risk = RiskLevel.Unknown;
                result.Summary = "Received an unexpected response from EmailRep.";
                result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("email", email);
                result.LookupLinks = FallbackSearchService.GetLookupLinks("email", email);
                return result;
            }

            var reputation = data["reputation"]?.ToString() ?? "none";
            var suspicious = data["suspicious"]?.Value<bool>() ?? false;
            var details = data["details"] as JObject;

            var blacklisted = details?["blacklisted"]?.Value<bool>() ?? false;
            var maliciousActivity = details?["malicious_activity"]?.Value<bool>() ?? false;
            var credentialsLeaked = details?["credentials_leaked"]?.Value<bool>() ?? false;
            var spammy = details?["spam"]?.Value<bool>() ?? false;
            var daysSinceDomainCreation = details?["days_since_domain_creation"]?.Value<int>() ?? -1;
            var freeProvider = details?["free_provider"]?.Value<bool>() ?? false;
            var deliverable = details?["deliverable"]?.Value<bool>() ?? true;
            var spoofable = details?["spoofable"]?.Value<bool>() ?? false;

            int riskPercent = 10;
            if (reputation == "high") riskPercent = 5;
            else if (reputation == "medium") riskPercent = 30;
            else if (reputation == "low") riskPercent = 65;
            else if (reputation == "none") riskPercent = 50;

            if (suspicious) riskPercent = Math.Max(riskPercent, 75);
            if (blacklisted) { riskPercent += 30; result.Findings.Add("This email is blacklisted"); }
            if (maliciousActivity) { riskPercent += 25; result.Findings.Add("Linked to malicious activity"); }
            if (credentialsLeaked) { riskPercent += 10; result.Findings.Add("Credentials leaked in a data breach"); }
            if (spammy) { riskPercent += 15; result.Findings.Add("Known for sending spam"); }
            if (spoofable) { riskPercent += 10; result.Findings.Add("Email domain can be spoofed"); }
            if (daysSinceDomainCreation >= 0 && daysSinceDomainCreation < 30)
            {
                riskPercent += 20;
                result.Findings.Add("Domain was created very recently (possible scam)");
            }

            if (!blacklisted && !maliciousActivity && !spammy && reputation is "high" or "medium")
                result.Findings.Add("No negative reports found");
            if (freeProvider)
                result.Findings.Add("Uses a free email provider (Gmail, Yahoo, etc.)");

            if (reputation == "none" && !blacklisted && !maliciousActivity && !spammy)
                await FallbackSearchService.EnrichWithAI(result, "email", email);

            result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("email", email);
            result.LookupLinks = FallbackSearchService.GetLookupLinks("email", email);

            riskPercent = Math.Clamp(riskPercent, 0, 100);
            result.RiskPercent = riskPercent;
            result.Risk = ScanResult.PercentToRisk(riskPercent);

            if (result.Risk == RiskLevel.Danger)
            {
                result.Recommendations.Add("DO NOT reply to this email");
                result.Recommendations.Add("DO NOT click any links in the email");
                result.Recommendations.Add("DO NOT open any attachments");
                result.Recommendations.Add("DELETE this email");
            }
            else if (result.Risk is RiskLevel.Warning or RiskLevel.Caution)
            {
                result.Recommendations.Add("Be careful with this email");
                result.Recommendations.Add("Do not click links unless you expected this email");
                result.Recommendations.Add("Do not share personal information");
            }
            else
            {
                result.Recommendations.Add("This email appears legitimate");
                result.Recommendations.Add("Still verify links before clicking");
                result.Recommendations.Add("Never share passwords via email");
            }

            result.Summary = $"Email reputation: {reputation}. Risk: {riskPercent}%.";
        }
        catch (HttpRequestException)
        {
            result.Risk = RiskLevel.Unknown;
            result.Summary = "Could not check this email — no internet or service unavailable.";
            result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("email", email);
            result.LookupLinks = FallbackSearchService.GetLookupLinks("email", email);
            result.Recommendations.Add("Try again in a moment");
            result.Recommendations.Add("Check your internet connection");
        }

        return result;
    }
}
