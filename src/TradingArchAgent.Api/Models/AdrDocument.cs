namespace TradingArchAgent.Api.Models;

public class AdrDocument
{
    public string Id           { get; set; } = string.Empty;
    public string Title        { get; set; } = string.Empty;
    public string Status       { get; set; } = "Accepted";
    public string Context      { get; set; } = string.Empty;
    public string Decision     { get; set; } = string.Empty;
    public string Consequences { get; set; } = string.Empty;
    public List<AlternativeConsidered> AlternativesConsidered { get; set; } = [];
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public string? SupersededBy { get; set; }

    public string ToMarkdown()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {Id}: {Title}");
        sb.AppendLine();
        sb.AppendLine($"**Status:** {Status}  ");
        sb.AppendLine($"**Date:** {CreatedAt:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine("## Context");
        sb.AppendLine(Context);
        sb.AppendLine();
        sb.AppendLine("## Decision");
        sb.AppendLine(Decision);
        sb.AppendLine();
        sb.AppendLine("## Consequences");
        sb.AppendLine(Consequences);
        sb.AppendLine();
        sb.AppendLine("## Alternatives Considered");
        foreach (var alt in AlternativesConsidered)
        {
            sb.AppendLine($"### {alt.Name}");
            sb.AppendLine($"**Why rejected:** {alt.ReasonRejected}");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}

public class AlternativeConsidered
{
    public string Name           { get; set; } = string.Empty;
    public string ReasonRejected { get; set; } = string.Empty;
}
