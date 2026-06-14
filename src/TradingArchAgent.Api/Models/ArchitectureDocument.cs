namespace TradingArchAgent.Api.Models;

public class ArchitectureDocument
{
    public List<BoundedContext> BoundedContexts { get; set; } = [];
    public string MermaidDiagram { get; set; } = string.Empty;
    public List<TechnologyChoice> TechnologyStack { get; set; } = [];
    public string DataFlowDescription { get; set; } = string.Empty;
    public List<string> KeyPatterns { get; set; } = [];
    public List<string> DeploymentNotes { get; set; } = [];
}

public class BoundedContext
{
    public string Name           { get; set; } = string.Empty;
    public string Responsibility { get; set; } = string.Empty;
    public List<string> KeyEntities { get; set; } = [];
    public List<string> ExposedApis { get; set; } = [];
    public List<string> DependsOn   { get; set; } = [];
}

public class TechnologyChoice
{
    public string Component  { get; set; } = string.Empty;
    public string Technology { get; set; } = string.Empty;
    public string Rationale  { get; set; } = string.Empty;
    public string? Version   { get; set; }
}
