# Trading Architecture Assistant

> **Five specialised AI agents that turn a single sentence into a complete, regulation-compliant trading system architecture package — in 75 seconds.**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![Semantic Kernel](https://img.shields.io/badge/Semantic_Kernel-1.30-512BD4?logo=microsoft)](https://learn.microsoft.com/semantic-kernel)
[![Azure OpenAI](https://img.shields.io/badge/Azure_OpenAI-GPT--4o-0089D6?logo=microsoft-azure)](https://azure.microsoft.com/products/ai-services/openai-service)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## The Problem

Designing a trading system architecture for a regulated financial institution typically requires:

- **8–14 weeks** of senior architect time
- **€75,000–€140,000** in consultant or contractor fees
- Deep expertise in MiFID II, EMIR, BAIT (BaFin), and DORA — rarely held by a single person
- Architecture Decision Records that are routinely skipped under time pressure
- Azure cost estimates that take days and are often inaccurate

Regulatory gaps discovered late — during audit or regulator examination — cost an average of **€2 million per MiFID II enforcement action** to remediate.

## The Solution

A five-agent AI pipeline that produces a complete, professional architecture package from a single plain-text requirement statement. Each agent is a domain specialist, running in parallel where dependencies allow.

**Input:** One sentence or paragraph.
**Output:** Nine professional documents in a ZIP file. Time: ~75 seconds.

---

## Live Demo

```bash
# Start the server
dotnet run

# Generate a complete architecture package
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/architecture" `
  -Headers @{"Content-Type"="application/json"} `
  -Body '{"requirement": "Build a real-time FX options pricing engine for a German broker. Sub-10ms latency, MiFID II compliant, 500K trades per day, deployed on Azure."}'
```

**Sample output (75 seconds):**
- Requirements: 8 functional, 6 NFRs, 12 regulatory items
- Architecture: 6 bounded contexts, Mermaid C4 diagram, 8-item tech stack
- Compliance: 24 regulatory checks across MiFID II, EMIR, BAIT, DORA
- ADRs: 5 Nygard-format Architecture Decision Records
- Cost: **€11,190/month** Azure West Europe estimate

---

## The Five Agents

| Agent | Role | Temperature | Output |
|-------|------|-------------|--------|
| RequirementAgent | Senior Business Analyst | 0.2 | Structured requirements (functional, NFR, regulatory) |
| ArchitectAgent | Principal Architect (15yr trading) | 0.3 | C4 design, Mermaid diagram, tech stack |
| RiskAgent | Regulatory Compliance Architect | 0.1 | MiFID II/EMIR/BAIT/DORA checklist (Pass/Fail/Review) |
| AdrAgent | Principal Architect (200+ ADRs) | 0.4 | 3–5 Nygard ADRs with alternatives |
| CostAgent | FinOps Architect (Azure WE) | 0.2 | EUR monthly estimate, SKU-level |

**Temperature rationale:**
- 0.1 for RiskAgent — regulatory assessment must be conservative and consistent
- 0.2–0.3 for analysis agents — structured but accurate
- 0.4 for AdrAgent — needs creative exploration of alternatives

---

## Pipeline Design

```
POST /api/architecture {"requirement": "..."}
              │
     Stage 1  ▼  (~20 seconds)
    ┌─────────────────────────┐
    │    RequirementAgent     │  Parses free-text → structured JSON
    └─────────────────────────┘
              │
     Stage 2  ▼  (~30 seconds, PARALLEL)
    ┌──────────────┐  ┌──────────────┐
    │ArchitectAgent│  │  RiskAgent   │  Both receive RequirementsDocument
    │C4 + Tech     │  │Reg. checklist│  Both run simultaneously
    └──────┬───────┘  └──────┬───────┘  30 seconds saved vs sequential
           └────────┬─────────┘
     Stage 3  ▼  (~25 seconds, PARALLEL)
    ┌──────────────┐  ┌──────────────┐
    │   AdrAgent   │  │  CostAgent   │  Both receive ArchitectureDocument
    │  5 ADRs      │  │  EUR estimate│  Both run simultaneously
    └──────┬───────┘  └──────┬───────┘  25 seconds saved vs sequential
           └────────┬─────────┘
                    ▼
         ArchitecturePackage
         ├── Full JSON response
         └── Base64 ZIP (9 Markdown documents)

Total: ~75 seconds  |  Sequential equivalent: ~170 seconds
Manual equivalent:   8–14 weeks
```

---

## Output Package

```
architecture-package.zip
├── 00-requirements.md        ← Structured requirements (functional/NFR/regulatory)
├── 01-architecture.md        ← C4 design, Mermaid diagram, technology stack, patterns
├── 02-compliance.md          ← Regulatory checklist (Pass ✅ / Fail ❌ / Review ⚠️)
├── 03-cost-estimate.md       ← Azure EUR breakdown with SKUs and optimisation tips
├── adr/
│   ├── ADR-001.md            ← Nygard format: context, decision, consequences, alternatives
│   ├── ADR-002.md
│   ├── ADR-003.md
│   ├── ADR-004.md
│   └── ADR-005.md
└── package.json              ← Machine-readable full response
```

### Decode the ZIP
```powershell
$r = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/architecture" `
  -Headers @{"Content-Type"="application/json"} `
  -Body '{"requirement": "..."}'

[System.Convert]::FromBase64String($r.zipBase64) | Set-Content -Path arch.zip -Encoding Byte
Expand-Archive arch.zip -DestinationPath arch-output
explorer arch-output
```

---

## Technology Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| API Framework | ASP.NET Core (.NET 10) Minimal API | REST endpoint, DI, middleware |
| AI Orchestration | Microsoft Semantic Kernel 1.30 | Agent management, prompt templates, kernel |
| Structured Output | JSON Object mode (GPT-4o) | Reliable, parseable agent responses |
| Parallelism | Task.WhenAll (.NET async/await) | Stages 2 and 3 run in parallel |
| Compression | System.IO.Compression | ZIP generation for document package |
| API Docs | Scalar (OpenAPI 10.0) | Interactive API documentation |
| Language Model | Azure OpenAI GPT-4o | All 5 agents |

---

## Business Value

| Metric | Traditional | This System |
|--------|------------|-------------|
| Time to complete package | 8–14 weeks | 75 seconds |
| Senior architect cost | €75K–€140K | < €1 API cost |
| Regulatory coverage | Depends on architect | Comprehensive (all EU regs) |
| ADR documentation | Routinely skipped | Always produced |
| Cost estimate accuracy | Variable | Consistent reference pricing |
| Iteration speed | One per project | Unlimited in minutes |

---

## Getting Started

### Prerequisites
- .NET 10 SDK (`winget install Microsoft.DotNet.SDK.10`)
- Azure OpenAI resource with gpt-4o deployed

### Setup
```bash
git clone https://github.com/milesbusiness/trading-arch-agent
cd trading-arch-agent/src/TradingArchAgent.Api
```

Create `appsettings.Development.json`:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4o"
  }
}
```

```bash
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run

# Interactive API docs at:
# http://localhost:5000/scalar/v1
```

---

## Documentation

| Document | Description |
|----------|-------------|
| [Executive Summary](docs/EXECUTIVE_SUMMARY.md) | Business case, ROI, stakeholder overview |
| [Architecture Guide](docs/ARCHITECTURE.md) | Agent pipeline, temperature decisions, parallelism |
| [Development Guide](docs/DEVELOPMENT.md) | Setup, configuration, adding new agents |

---

## About

Built to demonstrate multi-agent AI orchestration for regulated financial services architecture, targeting Principal Architect and AI Solution Architect roles at European financial institutions.

**Author:** Dilip Kumar Jena | **AI:** Semantic Kernel + GPT-4o | **Regulation:** MiFID II, EMIR, BAIT, DORA
