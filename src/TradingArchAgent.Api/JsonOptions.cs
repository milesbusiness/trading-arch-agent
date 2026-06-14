using System.Text.Json;
using System.Text.Json.Serialization;

namespace TradingArchAgent.Api;

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        Converters                  = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull
    };
}
