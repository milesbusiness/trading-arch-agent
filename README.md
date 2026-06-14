# Trading Architecture Assistant

> Multi-agent AI system that generates complete architecture packages for regulated trading systems in seconds.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![Semantic Kernel](https://img.shields.io/badge/Semantic_Kernel-1.21-68217A?logo=microsoft)](https://github.com/microsoft/semantic-kernel)
[![Azure OpenAI](https://img.shields.io/badge/Azure_OpenAI-GPT--4o-412991?logo=openai)](https://azure.microsoft.com/products/ai-services/openai-service)

---

## What It Does

Paste a trading system requirement in plain English. Five AI agents run in parallel and return a complete architecture package — in under 2 minutes.

**Output per request:**
- Structured requirements (functional, non-functional, regulatory)
- C4-aligned architecture with Mermaid diagram + technology stack
- Compliance checklist against MiFID II, EMIR, BAIT, DORA
- 3–5 Architecture Decision Records (Nygard format)
- Azure cost estimate (EUR, West Europe, SKU-level breakdown)
- ZIP archive of all documents

**Problem it solves:**

At Deutsche Bank and Commerzbank, architecture workshops for a new trading system take weeks — whiteboarding sessions, compliance sign-off reviews, ADR writing, cost modelling. This compresses that to a starting point in minutes that can be iterated with real stakeholders.

---

## Agent Pipeline

```
POST /api/architecture
       │
       ▼
 RequirementAgent          ← Parses free-text → structured RequirementsDocument
       │
   ┌───┴───┐
   ▼       ▼
ArchitectAgent  RiskAgent  ← Parallel: C4 design + MiFID II/EMIR/BAIT compliance
   └───┬───┘
       │
   ┌───┴───┐
   ▼       ▼
 AdrAgent  CostAgent      ← Parallel: ADR generation + Azure cost estimate
   └───┬───┘
       ▼
ArchitecturePackage        ← JSON + ZIP download
```

---

## Quick Start

```bash
# 1. Configure
cp appsettings.example.json src/TradingArchAgent.Api/appsettings.json
# Add Azure OpenAI endpoint and API key

# 2. Run
cd src/TradingArchAgent.Api
dotnet run

# 3. Try it
curl -X POST http://localhost:8080/api/architecture \
  -H "Content-Type: application/json" \
  -d '{"requirement": "Build a real-time FX options pricing engine for a German broker. Sub-10ms latency, 99.99% SLA, MiFID II compliant, 500K trades/day capacity."}'
```

---

## API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/architecture` | Generate full architecture package |
| `GET`  | `/health` | Health check |

**Request:**
```json
{
  "requirement": "Build a real-time FX options pricing engine..."
}
```

**Response:**
```json
{
  "id": "A3F7C2B1",
  "generatedAt": "2025-06-14T10:30:00Z",
  "requirements": { "functional": [...], "regulatory": [...] },
  "architecture": { "boundedContexts": [...], "mermaidDiagram": "...", "technologyStack": [...] },
  "compliance": { "items": [...], "overallRiskRating": "Medium" },
  "adrs": [{ "id": "ADR-001", "title": "...", "context": "...", "decision": "..." }],
  "costEstimate": { "monthlyTotalEur": 4850, "breakdown": [...] },
  "zipBase64": "<base64 ZIP containing all documents>"
}
```

---

## Interview Context

*"Architecture reviews at investment banks involve compliance, infrastructure cost, and ADR sign-off — three separate processes with three separate teams. I built this to demonstrate what's possible when you orchestrate specialist AI agents. The pipeline runs in parallel where possible and completes in under 90 seconds. It's been useful for generating first-draft architecture packages that then go into real review cycles."*

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| API | ASP.NET Core 10 Minimal API |
| Agent orchestration | Microsoft Semantic Kernel 1.21 |
| LLM | Azure OpenAI GPT-4o |
| Parallelism | `Task.WhenAll` — Stage 2 and 3 run in parallel |
| Output | JSON + Base64-encoded ZIP (Markdown per section) |
| Container | Docker (non-root, health check) |

---

## License

MIT
