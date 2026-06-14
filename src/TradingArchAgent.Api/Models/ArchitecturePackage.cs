namespace TradingArchAgent.Api.Models;

public class ArchitecturePackage
{
    public string Id                  { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
    public DateTime GeneratedAt       { get; set; } = DateTime.UtcNow;
    public string OriginalRequirement { get; set; } = string.Empty;

    public RequirementsDocument Requirements { get; set; } = new();
    public ArchitectureDocument Architecture { get; set; } = new();
    public ComplianceChecklist  Compliance   { get; set; } = new();
    public List<AdrDocument>    Adrs         { get; set; } = [];
    public CostEstimate         CostEstimate { get; set; } = new();

    public string? ZipBase64 { get; set; }
}
