using System.IO.Compression;
using System.Text;
using System.Text.Json;
using TradingArchAgent.Api.Agents;
using TradingArchAgent.Api.Models;

namespace TradingArchAgent.Api.Orchestration;

public class AgentOrchestrator(
    RequirementAgent requirementAgent,
    ArchitectAgent   architectAgent,
    RiskAgent        riskAgent,
    AdrAgent         adrAgent,
    CostAgent        costAgent,
    ILogger<AgentOrchestrator> logger)
{
    public async Task<ArchitecturePackage> RunAsync(string requirement, CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        logger.LogInformation("Orchestrator: starting pipeline");

        // Stage 1: Requirements (sequential — all others depend on it)
        var requirements = await requirementAgent.ExtractAsync(requirement, ct);
        logger.LogInformation("Stage 1 complete: {F} functional, {R} regulatory", requirements.Functional.Count, requirements.Regulatory.Count);

        // Stage 2: Architecture + Compliance (parallel)
        var archTask = architectAgent.DesignAsync(requirements, ct);
        var riskTask = riskAgent.AssessAsync(requirements, ct);
        await Task.WhenAll(archTask, riskTask);
        var architecture = await archTask;
        var compliance   = await riskTask;
        logger.LogInformation("Stage 2 complete: {BC} contexts, {CI} compliance items", architecture.BoundedContexts.Count, compliance.Items.Count);

        // Stage 3: ADRs + Cost (parallel, depend on architecture)
        var adrTask  = adrAgent.GenerateAsync(architecture, ct);
        var costTask = costAgent.EstimateAsync(architecture, ct);
        await Task.WhenAll(adrTask, costTask);
        var adrs = await adrTask;
        var cost = await costTask;
        logger.LogInformation("Stage 3 complete: {A} ADRs, €{C:N0}/month", adrs.Count, cost.MonthlyTotalEur);

        var package = new ArchitecturePackage
        {
            OriginalRequirement = requirement,
            Requirements = requirements,
            Architecture = architecture,
            Compliance   = compliance,
            Adrs         = adrs,
            CostEstimate = cost
        };

        package.ZipBase64 = BuildZip(package);
        sw.Stop();
        logger.LogInformation("Pipeline complete in {Ms:N0}ms", sw.ElapsedMilliseconds);
        return package;
    }

    private static string BuildZip(ArchitecturePackage package)
    {
        using var ms      = new MemoryStream();
        using var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true);

        void AddEntry(string name, string content)
        {
            var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
            using var w = new StreamWriter(entry.Open(), Encoding.UTF8);
            w.Write(content);
        }

        AddEntry("00-requirements.md", $"# Requirements\n\nOriginal: {package.OriginalRequirement}\n\n## Functional\n{string.Join("\n", package.Requirements.Functional.Select(f => $"- {f}"))}");
        AddEntry("01-architecture.md", $"# Architecture\n\n## Mermaid\n```mermaid\n{package.Architecture.MermaidDiagram}\n```\n\n## Tech Stack\n{string.Join("\n", package.Architecture.TechnologyStack.Select(t => $"- **{t.Component}**: {t.Technology} — {t.Rationale}"))}");
        AddEntry("02-compliance.md", $"# Compliance\n\n**Risk Rating:** {package.Compliance.OverallRiskRating}\n\n{string.Join("\n", package.Compliance.Items.Select(i => $"- [{i.Status}] {i.Regulation} {i.Article}: {i.Notes}"))}");
        AddEntry("03-cost.md", $"# Azure Cost Estimate\n\n**Monthly:** €{package.CostEstimate.MonthlyTotalEur:N0}\n**Annual:** €{package.CostEstimate.AnnualTotalEur:N0}\n\n{string.Join("\n", package.CostEstimate.Breakdown.Select(b => $"- {b.Service} ({b.Tier}): €{b.MonthlyEur:N0}/month"))}");
        for (int i = 0; i < package.Adrs.Count; i++)
            AddEntry($"adr/{package.Adrs[i].Id}.md", package.Adrs[i].ToMarkdown());

        archive.Dispose();
        return Convert.ToBase64String(ms.ToArray());
    }
}
