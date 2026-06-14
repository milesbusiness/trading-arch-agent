namespace TradingArchAgent.Api.Models;

public class ComplianceChecklist
{
    public List<ComplianceItem> Items { get; set; } = [];
    public string OverallRiskRating  { get; set; } = string.Empty;
    public List<string> CriticalGaps { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
}

public class ComplianceItem
{
    public string Regulation   { get; set; } = string.Empty;
    public string Article      { get; set; } = string.Empty;
    public ComplianceStatus Status { get; set; }
    public string Notes        { get; set; } = string.Empty;
    public List<string> RequiredControls { get; set; } = [];
}

public enum ComplianceStatus { Pass, Fail, NeedsReview }
