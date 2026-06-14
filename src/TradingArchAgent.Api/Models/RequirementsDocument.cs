namespace TradingArchAgent.Api.Models;

public class RequirementsDocument
{
    public List<string> Functional { get; set; } = [];
    public NonFunctionalRequirements NonFunctional { get; set; } = new();
    public List<string> Regulatory { get; set; } = [];
    public List<string> Constraints { get; set; } = [];
    public List<string> Assumptions { get; set; } = [];
    public string RawRequirement { get; set; } = string.Empty;
}

public class NonFunctionalRequirements
{
    public int? LatencyP99Ms       { get; set; }
    public int? ThroughputPerDay   { get; set; }
    public string? AvailabilitySla { get; set; }
    public string? RpoDuration     { get; set; }
    public string? RtoDuration     { get; set; }
    public List<string> Security   { get; set; } = [];
    public List<string> Scalability { get; set; } = [];
    public List<string> Other      { get; set; } = [];
}
