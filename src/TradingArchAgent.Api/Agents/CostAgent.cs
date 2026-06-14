using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using TradingArchAgent.Api.Models;

namespace TradingArchAgent.Api.Agents;

public class CostAgent(Kernel kernel, ILogger<CostAgent> logger)
{
    private const string SystemPrompt = """
        You are a FinOps architect specialising in Azure cost estimation for financial trading
        platforms. You know Azure public pricing for West Europe as of mid-2025.

        Given an architecture document, produce a monthly cost estimate in EUR.
        Return ONLY valid JSON — no markdown fences.

        Schema:
        {
          "monthlyTotalEur": <number>,
          "breakdown": [
            {
              "service": "<Azure service>",
              "tier": "<SKU>",
              "quantity": "<e.g. '3 nodes'>",
              "monthlyEur": <number>,
              "notes": "<key assumption>"
            }
          ],
          "currency": "EUR",
          "region": "West Europe",
          "disclaimer": "Estimates based on Azure public pricing...",
          "costOptimisationOpportunities": ["<opportunity>"]
        }

        Azure West Europe pricing (2025 PAYG):
        AKS: Standard_D4s_v5 ~€140/node/month, Standard_D8s_v5 ~€280/node/month
        Azure SQL GP 4vCores ~€450/month, BC 8vCores ~€1800/month
        Redis C2 Standard ~€80/month, P1 Premium ~€200/month
        Service Bus Premium (1 MU) ~€700/month
        Event Hubs Standard 10 TU ~€200/month
        Azure Monitor + Log Analytics ~€200-500/month
        Key Vault ~€20-50/month
        Container Registry Premium ~€50/month

        Size cluster based on bounded contexts. For 99.99% SLA add 30% for redundancy.
        Include Reserved Instance savings as cost optimisation opportunity.
        """;

    public async Task<CostEstimate> EstimateAsync(ArchitectureDocument architecture, CancellationToken ct = default)
    {
        logger.LogInformation("CostAgent: estimating for {Tech} tech choices", architecture.TechnologyStack.Count);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);
        history.AddUserMessage($"Tech: {string.Join(", ", architecture.TechnologyStack.Select(t => $"{t.Component}:{t.Technology}"))}\nContexts: {string.Join(", ", architecture.BoundedContexts.Select(c => c.Name))}\nDeployment: {string.Join("; ", architecture.DeploymentNotes)}");

        var settings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = 0.2,
                ["max_tokens"]  = 2048,
                ["response_format"] = new { type = "json_object" }
            }
        };

        var response = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
        var json = response.Content ?? "{}";

        try
        {
            return JsonSerializer.Deserialize<CostEstimate>(json, JsonOptions.Default)
                   ?? throw new InvalidOperationException("Null result");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "CostAgent: JSON parse failure");
            throw;
        }
    }
}
