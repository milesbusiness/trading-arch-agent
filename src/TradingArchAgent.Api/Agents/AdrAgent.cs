using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TradingArchAgent.Api.Models;

namespace TradingArchAgent.Api.Agents;

public class AdrAgent(Kernel kernel, ILogger<AdrAgent> logger)
{
    private const string SystemPrompt = """
        You are a Principal Architect who has authored over 200 Architecture Decision Records for
        trading systems at European investment banks. Follow the Nygard ADR format.

        Given an architecture design, identify the 3-5 most significant architectural decisions
        and write an ADR for each. Focus on decisions where rationale is non-obvious.

        Return ONLY valid JSON — no markdown fences.

        Schema:
        {
          "adrs": [
            {
              "id": "ADR-001",
              "title": "<imperative statement>",
              "status": "Accepted",
              "context": "<2-4 sentences: forces and constraints>",
              "decision": "<2-4 sentences: what was decided and primary reason>",
              "consequences": "<positive and negative trade-offs>",
              "alternativesConsidered": [
                { "name": "<alternative>", "reasonRejected": "<specific reason>" }
              ]
            }
          ]
        }

        Good topics: message broker choice, sync vs async inter-service, DB technology for time-series,
        event sourcing vs CRUD, service mesh, distributed transactions.
        Minimum 2 alternatives per ADR. Include honest negative consequences.
        """;

    public async Task<List<AdrDocument>> GenerateAsync(ArchitectureDocument architecture, CancellationToken ct = default)
    {
        logger.LogInformation("AdrAgent: generating ADRs for {BC} bounded contexts", architecture.BoundedContexts.Count);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);
        history.AddUserMessage($"Tech stack: {string.Join(", ", architecture.TechnologyStack.Select(t => $"{t.Component}:{t.Technology}"))}\nBounded contexts: {string.Join(", ", architecture.BoundedContexts.Select(c => c.Name))}\nKey patterns: {string.Join(", ", architecture.KeyPatterns)}");

#pragma warning disable SKEXP0010
        var settings = new OpenAIPromptExecutionSettings
        {
            Temperature    = 0.4,
            MaxTokens      = 4096,
            ResponseFormat = "json_object"
        };
#pragma warning restore SKEXP0010

        var response = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
        var json = response.Content ?? "{}";

        try
        {
            var wrapper = JsonSerializer.Deserialize<AdrWrapper>(json, JsonOptions.Default)
                          ?? throw new InvalidOperationException("Null result");

            return (wrapper.Adrs ?? []).Select(dto => new AdrDocument
            {
                Id           = dto.Id ?? "ADR-000",
                Title        = dto.Title ?? string.Empty,
                Status       = dto.Status ?? "Accepted",
                Context      = dto.Context ?? string.Empty,
                Decision     = dto.Decision ?? string.Empty,
                Consequences = dto.Consequences ?? string.Empty,
                AlternativesConsidered = (dto.AlternativesConsidered ?? [])
                    .Select(a => new AlternativeConsidered { Name = a.Name ?? string.Empty, ReasonRejected = a.ReasonRejected ?? string.Empty })
                    .ToList(),
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "AdrAgent: JSON parse failure");
            throw;
        }
    }

    private record AdrWrapper(List<AdrDto>? Adrs);
    private record AdrDto(string? Id, string? Title, string? Status, string? Context, string? Decision, string? Consequences, List<AltDto>? AlternativesConsidered);
    private record AltDto(string? Name, string? ReasonRejected);
}
