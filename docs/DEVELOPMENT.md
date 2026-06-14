# Development Guide

## Prerequisites

- .NET 10 SDK
- Azure OpenAI resource with GPT-4o deployed

## Setup

```bash
git clone https://github.com/milesbusiness/trading-arch-agent
cd trading-arch-agent/src/TradingArchAgent.Api
```

Edit `appsettings.json`:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-openai.openai.azure.com",
    "ApiKey": "your-key",
    "DeploymentName": "gpt-4o"
  }
}
```

## Run

```bash
dotnet run
# Swagger UI: http://localhost:5000/swagger
```

## Try It

```bash
curl -X POST http://localhost:5000/api/architecture \
  -H "Content-Type: application/json" \
  -d '{
    "requirement": "Build a real-time FX options pricing engine for a German broker. Sub-10ms latency, 99.99% SLA, MiFID II compliant, 500K trades/day, deployed on Azure."
  }'
```

Expected response time: 60–90 seconds (5 GPT-4o calls, 2 in parallel).

## Project Structure

```
trading-arch-agent/
├── src/TradingArchAgent.Api/
│   ├── Agents/
│   │   ├── RequirementAgent.cs   ← Stage 1
│   │   ├── ArchitectAgent.cs     ← Stage 2a (parallel)
│   │   ├── RiskAgent.cs          ← Stage 2b (parallel)
│   │   ├── AdrAgent.cs           ← Stage 3a (parallel)
│   │   └── CostAgent.cs          ← Stage 3b (parallel)
│   ├── Orchestration/
│   │   └── AgentOrchestrator.cs  ← Pipeline coordination
│   ├── Models/                   ← Domain models (one file per concept)
│   ├── JsonOptions.cs            ← Shared JSON serialisation settings
│   └── Program.cs                ← DI + minimal API endpoints
└── docs/
    ├── ARCHITECTURE.md
    └── DEVELOPMENT.md
```

## Adding a New Agent

1. Create `Agents/MyAgent.cs` with `MyAgent(Kernel kernel, ILogger<MyAgent> logger)`
2. Add system prompt as `private const string SystemPrompt = """..."""`
3. Call `kernel.GetRequiredService<IChatCompletionService>()` and set `response_format: json_object`
4. Deserialize response to your model
5. Register in `Program.cs`: `builder.Services.AddSingleton<MyAgent>()`
6. Add to `AgentOrchestrator` in the appropriate stage

## Running with Docker

```bash
docker build -t trading-arch-agent .
docker run -p 8080:8080 \
  -e AzureOpenAI__Endpoint=https://... \
  -e AzureOpenAI__ApiKey=... \
  trading-arch-agent
```
