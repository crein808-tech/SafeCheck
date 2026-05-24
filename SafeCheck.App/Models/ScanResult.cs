namespace SafeCheck.Models;

public class LookupLink
{
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
}

public class ScanResult
{
    public string Title { get; set; } = "";
    public string Input { get; set; } = "";
    public RiskLevel Risk { get; set; } = RiskLevel.Unknown;
    public int RiskPercent { get; set; }
    public List<string> Findings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public string Summary { get; set; } = "";
    public string? GoogleSearchUrl { get; set; }
    public List<LookupLink> LookupLinks { get; set; } = new();
    public DateTime ScannedAt { get; set; } = DateTime.Now;

    public bool HasLookupLinks => LookupLinks.Count > 0;

    public static RiskLevel PercentToRisk(int percent) => percent switch
    {
        <= 20 => RiskLevel.Safe,
        <= 60 => RiskLevel.Caution,
        <= 85 => RiskLevel.Warning,
        _ => RiskLevel.Danger
    };
}
