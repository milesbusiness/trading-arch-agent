using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using TradingArchAgent.Api.Models;

namespace TradingArchAgent.Api.Agents;

public class ArchitectAgent(Kernel kernel, ILogger<ArchitectAgent> logger)
{
    private const string SystemPrompt = """
        You are a Principal Architect with 15 years designing regulated financial trading platforms
        in European investment banks. You follow C4 model, DDD, and TOGAF ADM. You prefer
        event-driven microservices on Azure Kubernetes Service.

        Given structured requirements, produce an architecture design.
        Return ONLY valid JSON — no markdown fences, no prose outside JSON.

        Schema:
        {
          "boundedContexts": [
            {
              "name": "<PascalCase>",
              "responsibility": "<one sentence>",
              "keyEntities": ["<entity>"],
              "exposedApis": ["<api description>"],
              "dependsOn": ["<context name>"]
            }
          ],
          "mermaidDiagram": "<full Mermaid graph TD as escaped string>",
          "technologyStack": [
            {
              "component": "<logical component>",
              "technology": "<technology and version>",
              "rationale": "<2-3 sentences referencing NFRs>",
              "version": "<version or null>"
            }
          ],
          "dataFlowDescription": "<paragraph>",
          "keyPatterns": ["<pattern>"],
          "deploymentNotes": ["<note>"]
        }

        Rules: identify 4-7 bounded contexts. Include CQRS, Event Sourcing, Saga, Circuit Breaker in keyPatterns.
        For sub-10ms latency: recommend Redis and gRPC. For 99.99% SLA: recommend multi-AZ active-active.
        """;

    public async Task<ArchitectureDocument> DesignAsync(RequirementsDocument requirements, CancellationToken ct = default)
    {
        logger.LogInformation("ArchitectAgent: designing for {F} functional requirements", requirements.Functional.Count);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);
        history.AddUserMessage(BuildUserMessage(requirements));

        var settings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = 0.3,
                ["max_tokens"]  = 4096,
                ["response_format"] = new { type = "json_object" }
            }
        };

        var response = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
        var json = response.Content ?? "{}";

        try
        {
            return JsonSerializer.Deserialize<ArchitectureDocument>(json, JsonOptions.Default)
                   ?? throw new InvalidOperationException("Null result");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "ArchitectAgent: JSON parse failure");
            throw;
        }
    }

    private static string BuildUserMessage(RequirementsDocument req)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Requirement: {req.RawRequirement}");
        sb.AppendLine("Functional:");
        req.Functional.ForEach(f => sb.AppendLine($"  - {f}"));
        sb.AppendLine($"Latency P99: {req.NonFunctional.LatencyP99Ms?.ToString() ?? "not specified"} ms");
        sb.AppendLine($"Availability: {req.NonFunctional.AvailabilitySla ?? "not specified"}");
        sb.AppendLine("Regulatory:");
        req.Regulatory.ForEach(r => sb.AppendLine($"  - {r}"));
        return sb.ToString();
    }
}
