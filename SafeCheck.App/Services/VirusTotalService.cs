using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using SafeCheck.Models;

namespace SafeCheck.Services;

public static class VirusTotalService
{
    private static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri("https://www.virustotal.com/api/v3/"),
        Timeout = TimeSpan.FromSeconds(60)
    };

    private static HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl)
    {
        var key = ApiKeyService.Load().VirusTotalApiKey;
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("VirusTotal API key not configured. Go to Settings.");
        var request = new HttpRequestMessage(method, relativeUrl);
        request.Headers.Add("x-apikey", key);
        return request;
    }

    public static async Task<ScanResult> CheckFileAsync(string filePath, IProgress<string>? progress = null)
    {
        var result = new ScanResult { Title = "File Scan", Input = Path.GetFileName(filePath) };

        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > 650L * 1024 * 1024)
            {
                result.Risk = RiskLevel.Unknown;
                result.Summary = "File is too large for VirusTotal (max ~650 MB).";
                return result;
            }

            result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("file", Path.GetFileName(filePath));
            result.LookupLinks = FallbackSearchService.GetLookupLinks("file", Path.GetFileName(filePath));

            progress?.Report("Calculating file hash...");
            var hash = await ComputeSha256Async(filePath);

            progress?.Report("Checking if file was scanned before...");
            var existing = await TryGetFileReport(hash);
            if (existing != null)
            {
                ParseFileReport(existing, result);
                return result;
            }

            progress?.Report("Uploading file to VirusTotal...");
            var analysisId = await UploadFile(filePath);

            progress?.Report("Waiting for scan results...");
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(5000);
                var analysis = await GetAnalysis(analysisId);
                var status = analysis?["data"]?["attributes"]?["status"]?.ToString();
                if (status == "completed")
                {
                    var stats = analysis!["data"]!["attributes"]!["stats"] as JObject;
                    ParseAnalysisStats(stats, result);
                    return result;
                }
                progress?.Report($"Scanning... ({i * 5}s)");
            }

            result.Risk = RiskLevel.Unknown;
            result.Summary = "Scan is taking longer than expected. Try again later.";
            await FallbackSearchService.EnrichWithAI(result, "file", Path.GetFileName(filePath));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            result.Risk = RiskLevel.Unknown;
            result.Summary = "Daily scan limit reached - asking AI instead...";
            result.Recommendations.Add("Free VirusTotal allows 500 scans per day");
            await FallbackSearchService.EnrichWithAI(result, "file", Path.GetFileName(filePath));
        }
        catch (InvalidOperationException ex)
        {
            result.Risk = RiskLevel.Unknown;
            result.Summary = ex.Message;
        }
        catch (HttpRequestException)
        {
            result.Risk = RiskLevel.Unknown;
            result.Summary = "Could not reach VirusTotal - asking AI instead...";
            await FallbackSearchService.EnrichWithAI(result, "file", Path.GetFileName(filePath));
        }

        return result;
    }

    public static async Task<ScanResult> CheckUrlAsync(string url)
    {
        var result = new ScanResult { Title = "Link Scan", Input = url };

        try
        {
            var encodedUrl = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            using var getReq = CreateRequest(HttpMethod.Get, $"urls/{encodedUrl}");
            var getResp = await Http.SendAsync(getReq);
            getResp.EnsureSuccessStatusCode();
            var responseText = await getResp.Content.ReadAsStringAsync();

            JObject data;
            try { data = JObject.Parse(responseText); }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                result.Risk = RiskLevel.Unknown;
                result.Summary = "Received an unexpected response from VirusTotal.";
                return result;
            }

            var stats = data["data"]?["attributes"]?["last_analysis_stats"] as JObject;

            if (stats != null) ParseAnalysisStats(stats, result);
            else
            {
                using var submitReq = CreateRequest(HttpMethod.Post, "urls");
                submitReq.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("url", url) });
                var submitResp = await Http.SendAsync(submitReq);
                submitResp.EnsureSuccessStatusCode();
                var submitText = await submitResp.Content.ReadAsStringAsync();

                JObject submitData;
                try { submitData = JObject.Parse(submitText); }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    result.Risk = RiskLevel.Unknown;
                    result.Summary = "Received an unexpected response from VirusTotal.";
                    return result;
                }

                var analysisId = submitData["data"]?["id"]?.ToString();

                if (analysisId != null)
                {
                    await Task.Delay(10000);
                    var analysis = await GetAnalysis(analysisId);
                    var analysisStats = analysis?["data"]?["attributes"]?["stats"] as JObject;
                    ParseAnalysisStats(analysisStats, result);
                }
            }

            if (result.Risk == RiskLevel.Unknown)
            {
                var cpResult = await CheckPhishService.CheckUrlAsync(url);
                if (cpResult != null)
                    result = cpResult;
                else
                    await FallbackSearchService.EnrichWithAI(result, "link", url);
            }
            result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("link", url);
            result.LookupLinks = FallbackSearchService.GetLookupLinks("link", url);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            result.Risk = RiskLevel.Unknown;
            result.Summary = "VirusTotal limit reached - trying CheckPhish...";
            var cpResult = await CheckPhishService.CheckUrlAsync(url);
            if (cpResult != null)
                result = cpResult;
            else
            {
                result.Summary = "VirusTotal limit reached - asking AI instead...";
                await FallbackSearchService.EnrichWithAI(result, "link", url);
            }
            result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("link", url);
            result.LookupLinks = FallbackSearchService.GetLookupLinks("link", url);
        }
        catch (HttpRequestException)
        {
            result.Risk = RiskLevel.Unknown;
            result.Summary = "Could not reach VirusTotal - trying CheckPhish...";
            var cpResult = await CheckPhishService.CheckUrlAsync(url);
            if (cpResult != null)
                result = cpResult;
            else
            {
                result.Summary = "Could not reach VirusTotal - asking AI instead...";
                await FallbackSearchService.EnrichWithAI(result, "link", url);
            }
            result.GoogleSearchUrl = FallbackSearchService.GetGoogleSearchUrl("link", url);
            result.LookupLinks = FallbackSearchService.GetLookupLinks("link", url);
        }
        catch (InvalidOperationException ex)
        {
            result.Risk = RiskLevel.Unknown;
            result.Summary = ex.Message;
        }

        return result;
    }

    private static void ParseFileReport(JObject data, ScanResult result)
    {
        var stats = data["data"]?["attributes"]?["last_analysis_stats"] as JObject;
        ParseAnalysisStats(stats, result);
    }

    private static void ParseAnalysisStats(JObject? stats, ScanResult result)
    {
        if (stats == null)
        {
            result.Risk = RiskLevel.Unknown;
            result.Summary = "No analysis data available.";
            return;
        }

        var malicious = stats["malicious"]?.Value<int>() ?? 0;
        var suspicious = stats["suspicious"]?.Value<int>() ?? 0;
        var harmless = stats["harmless"]?.Value<int>() ?? 0;
        var undetected = stats["undetected"]?.Value<int>() ?? 0;
        var total = malicious + suspicious + harmless + undetected;

        if (total == 0) total = 1;
        int badCount = malicious + suspicious;
        int riskPercent = (int)((double)badCount / total * 100);

        if (malicious > 0) result.Findings.Add($"{malicious} security vendor(s) flagged this as malicious");
        if (suspicious > 0) result.Findings.Add($"{suspicious} vendor(s) flagged this as suspicious");
        result.Findings.Add($"{harmless + undetected} vendor(s) found nothing wrong");
        result.Findings.Add($"Scanned by {total} security vendors total");

        if (malicious == 0 && suspicious == 0) riskPercent = 5;
        else if (malicious <= 2) riskPercent = Math.Max(riskPercent, 40);
        else if (malicious <= 5) riskPercent = Math.Max(riskPercent, 70);
        else riskPercent = Math.Max(riskPercent, 90);

        riskPercent = Math.Clamp(riskPercent, 0, 100);
        result.RiskPercent = riskPercent;
        result.Risk = ScanResult.PercentToRisk(riskPercent);

        if (result.Risk is RiskLevel.Danger or RiskLevel.Warning)
        {
            result.Recommendations.Add("DO NOT open this file");
            result.Recommendations.Add("DELETE it from your computer");
            result.Recommendations.Add("Run a full antivirus scan on your computer");
        }
        else if (result.Risk == RiskLevel.Caution)
        {
            result.Recommendations.Add("Be careful with this file");
            result.Recommendations.Add("Only open it if you trust the source");
        }
        else
        {
            result.Recommendations.Add("No threats detected by security vendors");
            result.Recommendations.Add("File appears safe to open");
        }

        result.Summary = $"{badCount} of {total} vendors detected issues. Risk: {riskPercent}%.";
    }

    private static async Task<JObject?> TryGetFileReport(string sha256)
    {
        try
        {
            using var request = CreateRequest(HttpMethod.Get, $"files/{sha256}");
            var response = await Http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var text = await response.Content.ReadAsStringAsync();
            return JObject.Parse(text);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private static async Task<string> UploadFile(string filePath)
    {
        using var request = CreateRequest(HttpMethod.Post, "files");
        using var stream = File.OpenRead(filePath);
        using var formContent = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        formContent.Add(fileContent, "file", Path.GetFileName(filePath));
        request.Content = formContent;

        var response = await Http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseText = await response.Content.ReadAsStringAsync();

        JObject data;
        try { data = JObject.Parse(responseText); }
        catch (Newtonsoft.Json.JsonReaderException)
        {
            throw new InvalidOperationException("Unexpected response from VirusTotal during upload.");
        }

        return data["data"]?["id"]?.ToString()
            ?? throw new InvalidOperationException("No analysis ID returned from VirusTotal.");
    }

    private static async Task<JObject?> GetAnalysis(string analysisId)
    {
        using var request = CreateRequest(HttpMethod.Get, $"analyses/{analysisId}");
        var response = await Http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();
        try { return JObject.Parse(text); }
        catch (Newtonsoft.Json.JsonReaderException) { return null; }
    }

    private static async Task<string> ComputeSha256Async(string filePath)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = await sha.ComputeHashAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
