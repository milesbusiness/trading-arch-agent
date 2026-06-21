using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TradingArchAgent.Api.Models;

namespace TradingArchAgent.Api.Agents;

public class RiskAgent(Kernel kernel, ILogger<RiskAgent> logger)
{
    private const string SystemPrompt = """
        You are a Regulatory Compliance Architect specialising in MiFID II, EMIR, BAIT, and DORA.
        Analyse a trading system requirements document for regulatory gaps.

        Return ONLY valid JSON — no markdown fences.

        Schema:
        {
          "items": [
            {
              "regulation": "<name>",
              "article": "<article reference>",
              "status": "Pass" | "Fail" | "NeedsReview",
              "notes": "<specific finding>",
              "requiredControls": ["<control>"]
            }
          ],
          "overallRiskRating": "Low" | "Medium" | "High" | "Critical",
          "criticalGaps": ["<gap>"],
          "recommendations": ["<actionable recommendation>"]
        }

        Assess ALL: MiFID II Arts 17/25/26/27/31, EMIR Arts 4/9/11, BAIT Sections 1/4/5/8/10/11,
        DORA Arts 5-9/10-14/17-23. Mark NeedsReview when insufficient info; Fail only when clearly absent.
        """;

    public async Task<ComplianceChecklist> AssessAsync(RequirementsDocument requirements, CancellationToken ct = default)
    {
        logger.LogInformation("RiskAgent: assessing {R} regulatory requirements", requirements.Regulatory.Count);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);
        history.AddUserMessage($"Requirement: {requirements.RawRequirement}\nFunctional: {string.Join("; ", requirements.Functional)}\nRegulatory stated: {string.Join("; ", requirements.Regulatory)}");

#pragma warning disable SKEXP0010
        var settings = new OpenAIPromptExecutionSettings
        {
            Temperature    = 0.1,
            MaxTokens      = 3000,
            ResponseFormat = "json_object"
        };
#pragma warning restore SKEXP0010

        var response = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
        var json = response.Content ?? "{}";

        try
        {
            var dto = JsonSerializer.Deserialize<ComplianceDto>(json, JsonOptions.Default)
                      ?? throw new InvalidOperationException("Null result");

            return new ComplianceChecklist
            {
                OverallRiskRating = dto.OverallRiskRating ?? "Medium",
                CriticalGaps      = dto.CriticalGaps ?? [],
                Recommendations   = dto.Recommendations ?? [],
                Items = (dto.Items ?? []).Select(i => new ComplianceItem
                {
                    Regulation       = i.Regulation ?? string.Empty,
                    Article          = i.Article ?? string.Empty,
                    Status           = i.Status?.ToLower() switch { "pass" => ComplianceStatus.Pass, "fail" => ComplianceStatus.Fail, _ => ComplianceStatus.NeedsReview },
                    Notes            = i.Notes ?? string.Empty,
                    RequiredControls = i.RequiredControls ?? []
                }).ToList()
            };
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "RiskAgent: JSON parse failure");
            throw;
        }
    }

    private record ComplianceDto(List<ItemDto>? Items, string? OverallRiskRating, List<string>? CriticalGaps, List<string>? Recommendations);
    private record ItemDto(string? Regulation, string? Article, string? Status, string? Notes, List<string>? RequiredControls);
}
