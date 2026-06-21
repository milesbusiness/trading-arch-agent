using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TradingArchAgent.Api.Models;

namespace TradingArchAgent.Api.Agents;

public class RequirementAgent(Kernel kernel, ILogger<RequirementAgent> logger)
{
    private const string SystemPrompt = """
        You are a senior business analyst and solution architect specialising in regulated financial
        trading systems. Parse a free-text trading system requirement and extract structured information.

        Return ONLY valid JSON matching this schema. No markdown fences, no prose outside JSON.

        Schema:
        {
          "functional": ["<requirement>", ...],
          "nonFunctional": {
            "latencyP99Ms": <integer or null>,
            "throughputPerDay": <integer or null>,
            "availabilitySla": "<string or null>",
            "rpoDuration": "<ISO 8601 duration or null>",
            "rtoDuration": "<ISO 8601 duration or null>",
            "security": ["<requirement>", ...],
            "scalability": ["<requirement>", ...],
            "other": ["<requirement>", ...]
          },
          "regulatory": ["<regulation + article>", ...],
          "constraints": ["<constraint>", ...],
          "assumptions": ["<assumption>", ...]
        }

        Always include relevant EU regulations: MiFID II Arts 25-30, EMIR Arts 4/9/11, BAIT, DORA.
        Produce at least 5 functional requirements, at least 4 regulatory items, at least 3 security items.
        """;

    public async Task<RequirementsDocument> ExtractAsync(string requirement, CancellationToken ct = default)
    {
        logger.LogInformation("RequirementAgent: extracting from {Len} chars", requirement.Length);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);
        history.AddUserMessage($"Requirement: {requirement}");

#pragma warning disable SKEXP0010
        var settings = new OpenAIPromptExecutionSettings
        {
            Temperature    = 0.2,
            MaxTokens      = 2048,
            ResponseFormat = "json_object"
        };
#pragma warning restore SKEXP0010

        var response = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
        var json = response.Content ?? "{}";

        try
        {
            var dto = JsonSerializer.Deserialize<RequirementsDto>(json, JsonOptions.Default)
                      ?? throw new InvalidOperationException("Null result");
            return MapToDocument(dto, requirement);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "RequirementAgent: JSON parse failure");
            throw new InvalidOperationException($"RequirementAgent returned invalid JSON: {ex.Message}", ex);
        }
    }

    private static RequirementsDocument MapToDocument(RequirementsDto dto, string raw) => new()
    {
        RawRequirement = raw,
        Functional     = dto.Functional ?? [],
        Regulatory     = dto.Regulatory ?? [],
        Constraints    = dto.Constraints ?? [],
        Assumptions    = dto.Assumptions ?? [],
        NonFunctional  = new NonFunctionalRequirements
        {
            LatencyP99Ms     = dto.NonFunctional?.LatencyP99Ms,
            ThroughputPerDay = dto.NonFunctional?.ThroughputPerDay,
            AvailabilitySla  = dto.NonFunctional?.AvailabilitySla,
            RpoDuration      = dto.NonFunctional?.RpoDuration,
            RtoDuration      = dto.NonFunctional?.RtoDuration,
            Security         = dto.NonFunctional?.Security ?? [],
            Scalability      = dto.NonFunctional?.Scalability ?? [],
            Other            = dto.NonFunctional?.Other ?? []
        }
    };

    private record RequirementsDto(
        List<string>? Functional,
        NonFunctionalDto? NonFunctional,
        List<string>? Regulatory,
        List<string>? Constraints,
        List<string>? Assumptions);

    private record NonFunctionalDto(
        int? LatencyP99Ms,
        int? ThroughputPerDay,
        string? AvailabilitySla,
        string? RpoDuration,
        string? RtoDuration,
        List<string>? Security,
        List<string>? Scalability,
        List<string>? Other);
}
