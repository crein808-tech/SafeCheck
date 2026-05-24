using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using SafeCheck.Models;

namespace SafeCheck.Services;

public static class CheckPhishService
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private const string ScanUrl = "https://developers.bolster.ai/api/neo/scan";
    private const string StatusUrl = "https://developers.bolster.ai/api/neo/scan/status";

    public static async Task<ScanResult?> CheckUrlAsync(string url)
    {
        var key = ApiKeyService.Load().CheckPhishApiKey;
        if (string.IsNullOrWhiteSpace(key))
            return null;

        try
        {
            var jobId = await SubmitScan(key, url);
            if (string.IsNullOrEmpty(jobId))
                return null;

            await Task.Delay(3000);

            for (int i = 0; i < 10; i++)
            {
                var result = await GetScanResult(key, jobId, url);
                if (result != null)
                    return result;
                await Task.Delay(2000);
            }
        }
        catch (HttpRequestException)
        {
        }

        return null;
    }

    private static async Task<string?> SubmitScan(string apiKey, string url)
    {
        var body = new JObject
        {
            ["apiKey"] = apiKey,
            ["urlInfo"] = new JObject { ["url"] = url },
            ["scanType"] = "quick"
        };

        var content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
        var response = await Http.PostAsync(ScanUrl, content);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            return null;

        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();

        JObject data;
        try { data = JObject.Parse(text); }
        catch (Newtonsoft.Json.JsonReaderException) { return null; }

        return data["jobID"]?.ToString();
    }

    private static async Task<ScanResult?> GetScanResult(string apiKey, string jobId, string url)
    {
        var body = new JObject
        {
            ["apiKey"] = apiKey,
            ["jobID"] = jobId,
            ["insights"] = true
        };

        var content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
        var response = await Http.PostAsync(StatusUrl, content);
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();

        JObject data;
        try { data = JObject.Parse(text); }
        catch (Newtonsoft.Json.JsonReaderException) { return null; }

        var status = data["job_status"]?.ToString();
        if (status != "DONE")
            return null;

        var disposition = data["disposition"]?.ToString()?.ToLowerInvariant() ?? "";
        var brand = data["brand"]?.ToString() ?? "";

        var result = new ScanResult { Title = "Link Scan (CheckPhish)", Input = url };

        switch (disposition)
        {
            case "clean":
                result.Risk = RiskLevel.Safe;
                result.RiskPercent = 5;
                result.Summary = "CheckPhish: No threats detected.";
                result.Findings.Add("CheckPhish AI scan found no phishing or scam indicators");
                result.Recommendations.Add("This link appears safe");
                break;

            case "phish" or "phishing":
                result.Risk = RiskLevel.Danger;
                result.RiskPercent = 95;
                result.Summary = "CheckPhish: Phishing site detected!";
                result.Findings.Add("CheckPhish flagged this URL as a phishing site");
                if (!string.IsNullOrEmpty(brand))
                    result.Findings.Add($"Impersonating: {brand}");
                result.Recommendations.Add("DO NOT enter any personal information");
                result.Recommendations.Add("Close this website immediately");
                break;

            case "suspicious":
                result.Risk = RiskLevel.Warning;
                result.RiskPercent = 75;
                result.Summary = "CheckPhish: Suspicious site detected.";
                result.Findings.Add("CheckPhish flagged this URL as suspicious");
                if (!string.IsNullOrEmpty(brand))
                    result.Findings.Add($"May be impersonating: {brand}");
                result.Recommendations.Add("Be very careful with this site");
                result.Recommendations.Add("Do not enter passwords or personal info");
                break;

            default:
                result.Risk = RiskLevel.Caution;
                result.RiskPercent = 40;
                result.Summary = $"CheckPhish: {disposition}";
                result.Findings.Add($"CheckPhish disposition: {disposition}");
                result.Recommendations.Add("Exercise caution with this site");
                break;
        }

        return result;
    }
}
