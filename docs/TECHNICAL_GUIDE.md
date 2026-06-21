# Technical Guide — Trading Architecture Assistant

> This guide explains every technology used, how to learn it, how to install the project, what every file does line by line, and how to see the output.

---

## Table of Contents

1. [Technologies Used](#1-technologies-used)
2. [Where to Learn Each Technology](#2-where-to-learn-each-technology)
3. [Installation — Step by Step](#3-installation--step-by-step)
4. [Project File Structure](#4-project-file-structure)
5. [Code Walkthrough — Every File Explained](#5-code-walkthrough--every-file-explained)
6. [How to Run and View Output](#6-how-to-run-and-view-output)

---

## 1. Technologies Used

| Technology | Version | What it is | Why it is used here |
|-----------|---------|-----------|-------------------|
| **.NET 10** | 10.0 | Microsoft's cross-platform runtime for C# | The API host — fast, modern, minimal API pattern |
| **ASP.NET Core Minimal API** | 10.0 | HTTP server framework for .NET | Exposes a single `/api/architecture` REST endpoint with minimal boilerplate |
| **Microsoft Semantic Kernel** | 1.30.0 | Microsoft's SDK for building AI orchestration in .NET/Python | Creates and manages the Kernel (the AI engine), connects to Azure OpenAI, manages ChatHistory and prompts |
| **Azure OpenAI** | API 2025-01 | Microsoft-hosted GPT models with enterprise SLA | Runs GPT-4o to power all 5 agents |
| **GPT-4o** | — | OpenAI's flagship model | Large context window, excellent structured JSON output, fast enough for parallel agent calls |
| **OpenAIPromptExecutionSettings** | SK 1.30 | Semantic Kernel class for configuring model parameters | Controls Temperature, MaxTokens, ResponseFormat="json_object" per agent |
| **Scalar** | 2.5.6 | OpenAPI UI for .NET 10 | Interactive API documentation at `/scalar/v1` (replaces Swagger for .NET 10) |
| **System.IO.Compression** | Built in | .NET ZIP library | Packages all 9 output documents into a base64-encoded ZIP |
| **System.Text.Json** | Built in | High-performance JSON serialiser | Deserialises the JSON that GPT-4o returns into C# model objects |
| **C# Records** | C# 12 | Immutable value types for DTOs | Clean, concise data transfer objects for AI response parsing |

**Official Links:**
- .NET 10: https://dotnet.microsoft.com/download/dotnet/10.0
- Semantic Kernel: https://learn.microsoft.com/semantic-kernel/overview
- Semantic Kernel GitHub: https://github.com/microsoft/semantic-kernel
- Azure OpenAI: https://learn.microsoft.com/azure/ai-services/openai/overview
- Scalar: https://scalar.com/

---

## 2. Where to Learn Each Technology

### .NET and C#

**Official:**
- https://learn.microsoft.com/dotnet/csharp/ — C# language documentation
- https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis — Minimal API guide
- https://dotnet.microsoft.com/learn — Free official learning path

**YouTube:**
- "ASP.NET Core Minimal APIs" by Nick Chapsas — https://www.youtube.com/@nickchapsas
- ".NET 8 / 10 tutorials" by Tim Corey — https://www.youtube.com/@IAmTimCorey
- "C# tutorial for beginners" by Mosh Hamedani — https://www.youtube.com/@programmingwithmosh

**What to focus on first:**
1. C# records and primary constructors (used everywhere in this project)
2. async/await and Task
3. Dependency Injection (AddSingleton, constructor injection)
4. Minimal API pattern (MapPost, MapGet, Results.Ok)

### Microsoft Semantic Kernel

**Official:**
- https://learn.microsoft.com/semantic-kernel/get-started/quick-start-guide — 5-minute quickstart
- https://learn.microsoft.com/semantic-kernel/concepts/ai-services/chat-completion — Chat completion
- https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples — 40+ code samples

**YouTube:**
- "Microsoft Semantic Kernel" by Microsoft Developer — https://www.youtube.com/@MicrosoftDeveloper (search "Semantic Kernel")
- "Build AI agents with Semantic Kernel" — search on YouTube

**What to focus on first:**
1. `Kernel.CreateBuilder().AddAzureOpenAIChatCompletion().Build()`
2. `IChatCompletionService` and `ChatHistory`
3. `OpenAIPromptExecutionSettings` with `ResponseFormat`

### Azure OpenAI

**Official:**
- https://learn.microsoft.com/azure/ai-services/openai/quickstart — Quickstart
- https://learn.microsoft.com/azure/ai-services/openai/concepts/models — Available models
- https://oai.azure.com/ — Azure AI Foundry portal (where you deploy models)

---

## 3. Installation — Step by Step

### Step 1 — Install .NET 10 SDK

```powershell
winget install Microsoft.DotNet.SDK.10
# Verify:
dotnet --version
# Should show: 10.0.xxx
```

Official download: https://dotnet.microsoft.com/download/dotnet/10.0

### Step 2 — Clone the Repository

```powershell
git clone https://github.com/milesbusiness/trading-arch-agent
cd trading-arch-agent
```

### Step 3 — Set Up Azure OpenAI

You need an Azure OpenAI resource with GPT-4o deployed. If you don't have one:

1. Go to https://oai.azure.com/ (Azure AI Foundry)
2. Create a resource in any region that has GPT-4o available (e.g., East US)
3. Deploy the `gpt-4o` model
4. Copy: **Endpoint URL** and **API Key** from the Keys & Endpoint page

### Step 4 — Create Credentials File

In `src/TradingArchAgent.Api/` create a file named `appsettings.Development.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "ApiKey": "YOUR-KEY-HERE",
    "DeploymentName": "gpt-4o"
  }
}
```

**This file is in `.gitignore` — it will never be committed to Git.**

### Step 5 — Run

```powershell
cd src/TradingArchAgent.Api
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

---

## 4. Project File Structure

```
trading-arch-agent/
└── src/
    └── TradingArchAgent.Api/
        ├── Agents/                       ← One file per AI agent
        │   ├── RequirementAgent.cs       ← Extracts structured requirements from free text
        │   ├── ArchitectAgent.cs         ← Designs C4 architecture
        │   ├── RiskAgent.cs              ← Checks regulatory compliance
        │   ├── AdrAgent.cs               ← Writes Architecture Decision Records
        │   └── CostAgent.cs              ← Estimates Azure costs in EUR
        ├── Models/                       ← C# classes that hold the data
        │   ├── ArchitecturePackage.cs    ← The complete output (all 5 agents combined)
        │   ├── RequirementsDocument.cs   ← RequirementAgent's output shape
        │   ├── ArchitectureDocument.cs   ← ArchitectAgent's output shape
        │   ├── ComplianceChecklist.cs    ← RiskAgent's output shape
        │   ├── AdrDocument.cs            ← AdrAgent's output shape (one per ADR)
        │   └── CostEstimate.cs           ← CostAgent's output shape
        ├── Orchestration/
        │   └── AgentOrchestrator.cs      ← Runs the 3-stage pipeline, builds ZIP
        ├── Program.cs                    ← Entry point: DI setup, HTTP endpoint, startup
        ├── JsonOptions.cs                ← Shared JSON serialiser settings
        ├── appsettings.json              ← Default config (no secrets)
        ├── appsettings.Development.json  ← YOUR Azure credentials (gitignored)
        └── TradingArchAgent.Api.csproj   ← Package references (NuGet)
```

---

## 5. Code Walkthrough — Every File Explained

### `Program.cs` — The Entry Point

```csharp
var builder = WebApplication.CreateBuilder(args);
```
Creates the ASP.NET Core application builder. This is always line 1 of a .NET Minimal API.

```csharp
var aoaiEndpoint = builder.Configuration["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is required");
```
Reads credentials from `appsettings.Development.json`. The `??` operator throws immediately if the value is missing — fail-fast on startup rather than cryptic errors at request time.

```csharp
builder.Services.AddSingleton<Kernel>(_ =>
    Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(aoaiDeployment, aoaiEndpoint, aoaiApiKey)
        .Build());
```
Creates one Semantic Kernel instance shared across the entire application lifetime (`AddSingleton`). The Kernel is the AI engine — it holds the connection to Azure OpenAI.

```csharp
builder.Services.AddSingleton<RequirementAgent>();
builder.Services.AddSingleton<ArchitectAgent>();
// ... all 5 agents and the orchestrator
```
Registers all agents with the dependency injection container. When the orchestrator is created, .NET automatically injects the Kernel into each agent's constructor.

```csharp
app.MapPost("/api/architecture", async (ArchitectureRequest request, AgentOrchestrator orchestrator) =>
{
    var package = await orchestrator.RunAsync(request.Requirement);
    return Results.Ok(package);
})
```
The single REST endpoint. When `POST /api/architecture` is called, .NET deserialises the JSON body into `ArchitectureRequest`, calls the orchestrator, and returns the result as JSON.

```csharp
public record ArchitectureRequest(string Requirement);
```
A C# record — immutable, single-line data class. `Requirement` is the user's text input.

---

### `Orchestration/AgentOrchestrator.cs` — The Pipeline Manager

This is the most important file. It controls the 3-stage parallel pipeline.

```csharp
public class AgentOrchestrator(
    RequirementAgent requirementAgent,
    ArchitectAgent   architectAgent,
    RiskAgent        riskAgent,
    AdrAgent         adrAgent,
    CostAgent        costAgent,
    ILogger<AgentOrchestrator> logger)
```
Primary constructor (C# 12 syntax). .NET automatically provides all 5 agents and a logger via dependency injection.

```csharp
var requirements = await requirementAgent.ExtractAsync(requirement, ct);
```
**Stage 1** — Sequential. Every other agent needs the requirements first, so this runs alone.

```csharp
var archTask = architectAgent.DesignAsync(requirements, ct);
var riskTask = riskAgent.AssessAsync(requirements, ct);
await Task.WhenAll(archTask, riskTask);
```
**Stage 2** — Parallel. Both agents receive the same requirements and run simultaneously. `Task.WhenAll` waits for both to finish. This saves ~30 seconds vs running them sequentially.

```csharp
var adrTask  = adrAgent.GenerateAsync(architecture, ct);
var costTask = costAgent.EstimateAsync(architecture, ct);
await Task.WhenAll(adrTask, costTask);
```
**Stage 3** — Parallel. Both receive the architecture document and run simultaneously.

```csharp
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
    // ... adds each document as a file inside the ZIP
    return Convert.ToBase64String(ms.ToArray());
}
```
Creates a ZIP file entirely in memory (no disk writes), converts it to base64 string so it can be included in the JSON response.

---

### `Agents/RequirementAgent.cs` — Stage 1 Agent

```csharp
private const string SystemPrompt = """
    You are a senior business analyst and solution architect specialising in regulated financial
    trading systems. Parse a free-text trading system requirement and extract structured information.
    Return ONLY valid JSON matching this schema. No markdown fences, no prose outside JSON.
    ...
    """;
```
The system prompt defines the agent's persona and expertise. The critical instruction is "Return ONLY valid JSON" — this makes the GPT response parseable by `JsonSerializer.Deserialize`.

```csharp
var settings = new OpenAIPromptExecutionSettings
{
    Temperature    = 0.2,     // Low = consistent, predictable output
    MaxTokens      = 2048,
    ResponseFormat = "json_object"   // Forces GPT to output valid JSON
};
```
`ResponseFormat = "json_object"` is the key setting — it instructs the GPT model to always produce valid JSON (no accidental prose). Temperature 0.2 gives deterministic-ish output for analysis tasks.

```csharp
var response = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
var json = response.Content ?? "{}";
var dto = JsonSerializer.Deserialize<RequirementsDto>(json, JsonOptions.Default);
return MapToDocument(dto, requirement);
```
Sends the prompt to GPT-4o, gets the JSON response string back, deserialises it into a C# record, then maps it to the domain model (`RequirementsDocument`).

**Why separate DTO from domain model?** The DTO (`RequirementsDto`) exactly mirrors what GPT returns (nullable fields, simple types). The domain model (`RequirementsDocument`) has non-nullable, validated data. The `MapToDocument` method acts as the translation layer.

---

### `Agents/ArchitectAgent.cs` — Architecture Design Agent

Temperature: 0.3 (slightly more creative than RequirementAgent — architecture needs some exploration).

The system prompt gives the agent a specific persona: *"Principal Architect with 15 years designing regulated financial trading platforms in European investment banks."*

The `BuildUserMessage` method formats the requirements into a structured prompt:
```csharp
private static string BuildUserMessage(RequirementsDocument req)
{
    var sb = new StringBuilder();
    sb.AppendLine($"Requirement: {req.RawRequirement}");
    sb.AppendLine("Functional:");
    req.Functional.ForEach(f => sb.AppendLine($"  - {f}"));
    sb.AppendLine($"Latency P99: {req.NonFunctional.LatencyP99Ms} ms");
    // ...
}
```
Feeding structured data (not just the original text) produces much more consistent architecture designs.

---

### `Agents/RiskAgent.cs` — Regulatory Compliance Agent

Temperature: **0.1** — the lowest of all agents. Regulatory assessment must be conservative and consistent. Higher temperature would produce creative but potentially wrong compliance judgements.

The schema includes a `status` field with only three values: `"Pass" | "Fail" | "NeedsReview"`. The RiskAgent maps these to a C# enum:
```csharp
Status = i.Status?.ToLower() switch {
    "pass" => ComplianceStatus.Pass,
    "fail" => ComplianceStatus.Fail,
    _      => ComplianceStatus.NeedsReview
}
```
Any unexpected value defaults to `NeedsReview` — the safer option.

---

### `Agents/AdrAgent.cs` — Architecture Decision Records Agent

Temperature: **0.4** — the highest of all agents. ADRs need to explore creative alternatives and consider non-obvious trade-offs. Higher temperature produces more varied, exploratory thinking.

The Nygard ADR format (Michael Nygard, 2011) is a standard in software architecture:
- **Context**: What is the situation and what forces are at play?
- **Decision**: What was decided and why?
- **Consequences**: What happens as a result — positive and negative?
- **Alternatives Considered**: What else was evaluated and why was it rejected?

---

### `Agents/CostAgent.cs` — Azure Cost Estimation Agent

The system prompt includes actual Azure West Europe pricing as of mid-2025 (hardcoded):
```
AKS: Standard_D4s_v5 ~€140/node/month
Azure SQL GP 4vCores ~€450/month
Redis C2 Standard ~€80/month
```

This is critical — without real pricing in the prompt, GPT would hallucinate costs. By providing the actual price list, the agent can produce accurate EUR estimates that can be cross-checked against the Azure pricing calculator.

---

### `Models/ArchitecturePackage.cs` — The Complete Output

```csharp
public class ArchitecturePackage
{
    public string Id                  { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
    public DateTime GeneratedAt       { get; set; } = DateTime.UtcNow;
    public string OriginalRequirement { get; set; } = string.Empty;

    public RequirementsDocument Requirements { get; set; } = new();
    public ArchitectureDocument Architecture { get; set; } = new();
    public ComplianceChecklist  Compliance   { get; set; } = new();
    public List<AdrDocument>    Adrs         { get; set; } = [];
    public CostEstimate         CostEstimate { get; set; } = new();

    public string? ZipBase64 { get; set; }  // base64-encoded ZIP of all documents
}
```
The `Id` field uses a Guid formatted as 8 uppercase hex characters (e.g., `A3F7B291`). `ZipBase64` is `null` until `BuildZip()` is called at the end of the orchestrator pipeline.

---

## 6. How to Run and View Output

### Start the Server

```powershell
cd src/TradingArchAgent.Api
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

### Option 1: Interactive API Documentation (Recommended)

Open in your browser: **http://localhost:5000/scalar/v1**

You will see the Scalar API UI. Click the `/api/architecture` endpoint, click "Try it", paste a requirement, and click "Send".

### Option 2: PowerShell Command

```powershell
$result = Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5000/api/architecture" `
  -Headers @{"Content-Type"="application/json"} `
  -Body '{"requirement": "Build a real-time FX options pricing engine for a German broker. Sub-10ms latency, MiFID II compliant, 500K trades per day, deployed on Azure."}'

# View the requirements extracted by the AI
$result.requirements.functional

# View the architecture overview
$result.architecture.boundedContexts | Format-Table name, responsibility

# View the compliance checklist
$result.compliance.items | Format-Table regulation, article, status

# View the cost estimate
$result.costEstimate.breakdown | Format-Table service, tier, monthlyEur

# View the ADRs
$result.adrs | Format-Table id, title, status
```

### Option 3: Extract the ZIP to Files

```powershell
# Save the base64 ZIP to a file and extract it
[System.Convert]::FromBase64String($result.zipBase64) |
    Set-Content -Path "$env:TEMP\arch-output.zip" -Encoding Byte

Expand-Archive "$env:TEMP\arch-output.zip" -DestinationPath "$env:TEMP\arch-output" -Force

# Open the output folder in Explorer
explorer "$env:TEMP\arch-output"
```

Inside the folder you will find:
```
arch-output/
├── 00-requirements.md     ← Structured requirements
├── 01-architecture.md     ← C4 design with Mermaid diagram
├── 02-compliance.md       ← Regulatory checklist
├── 03-cost.md             ← Azure EUR cost breakdown
└── adr/
    ├── ADR-001.md
    ├── ADR-002.md
    ├── ADR-003.md
    ├── ADR-004.md
    └── ADR-005.md
```

### Sample Output (from actual run)

Input: `"Build a real-time FX options pricing engine for a German broker. Sub-10ms latency, MiFID II compliant, 500K trades per day, deployed on Azure."`

```
Requirements: 8 functional, 6 NFRs, 12 regulatory items
Architecture: 6 bounded contexts, Mermaid C4 diagram, 8-item tech stack
Compliance:   24 regulatory checks (MiFID II, EMIR, BAIT, DORA)
ADRs:         5 Architecture Decision Records
Cost:         €11,190/month Azure West Europe
Time:         ~75 seconds total
```

### Health Check

```powershell
Invoke-RestMethod http://localhost:5000/health
# Returns: { "status": "healthy", "timestamp": "2026-06-22T..." }
```

---

## Common Issues

| Problem | Cause | Fix |
|---------|-------|-----|
| `AzureOpenAI:Endpoint is required` at startup | `appsettings.Development.json` not found | Ensure file exists AND `$env:ASPNETCORE_ENVIRONMENT="Development"` was set before `dotnet run` |
| `HTTP 401 Unauthorized` | Wrong API key | Double-check the key in `appsettings.Development.json` |
| `HTTP 404 Not Found` for model | Deployment name mismatch | Ensure `DeploymentName` matches exactly what you named it in Azure AI Foundry |
| Build error `SKEXP0010` | SK experimental feature pragma | Already handled with `#pragma warning disable SKEXP0010` |
| Long response time (>3 min) | Azure OpenAI rate limiting | Free tier has very low TPM limits; check Azure AI Foundry quotas |
