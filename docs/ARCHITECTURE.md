# Trading Architecture Assistant — Architecture

## Overview

A multi-agent AI system that accepts a free-text trading system requirement and returns a complete architecture package: structured requirements, C4 design, MiFID II/EMIR/BAIT compliance checklist, Architecture Decision Records, and Azure cost estimate.

**Pipeline completes in ~60–90 seconds.** Stages 2 and 3 run in parallel.

---

## Agent Pipeline

```
POST /api/architecture  {"requirement": "..."}
          │
          ▼
    ┌─────────────────────────────────────────────────────┐
    │  Stage 1 (sequential)                               │
    │                                                     │
    │  RequirementAgent                                   │
    │    ├── Input:  free-text requirement                │
    │    ├── Model:  GPT-4o (temp=0.2, json_object mode) │
    │    └── Output: RequirementsDocument                 │
    │               ├── functional[]                      │
    │               ├── nonFunctional (latency, SLA, ...) │
    │               ├── regulatory[]                      │
    │               └── constraints[]                     │
    └─────────────────────────────────────────────────────┘
          │
          ▼ (passes RequirementsDocument to Stage 2)
    ┌─────────────────────────────────────────────────────┐
    │  Stage 2 (parallel — Task.WhenAll)                  │
    │                                                     │
    │  ArchitectAgent              RiskAgent              │
    │    ├── temp=0.3               ├── temp=0.1          │
    │    ├── C4 bounded contexts    ├── MiFID II Arts      │
    │    ├── Mermaid diagram        ├── EMIR Arts          │
    │    ├── Technology stack       ├── BAIT Sections      │
    │    └── Deployment notes       ├── DORA Arts          │
    │                               └── Pass/Fail/Review  │
    └─────────────────────────────────────────────────────┘
          │
          ▼ (passes ArchitectureDocument to Stage 3)
    ┌─────────────────────────────────────────────────────┐
    │  Stage 3 (parallel — Task.WhenAll)                  │
    │                                                     │
    │  AdrAgent                    CostAgent              │
    │    ├── temp=0.4               ├── temp=0.2          │
    │    ├── 3–5 ADRs               ├── Azure West Europe │
    │    ├── Nygard format          ├── SKU-level pricing │
    │    └── 2+ alternatives each   └── EUR monthly total │
    └─────────────────────────────────────────────────────┘
          │
          ▼
    ArchitecturePackage
      ├── JSON response
      └── Base64 ZIP (one Markdown file per section + ADRs)
```

---

## Why These Temperature Settings?

| Agent | Temp | Reason |
|-------|------|--------|
| RequirementAgent | 0.2 | Extraction task — deterministic parsing |
| ArchitectAgent | 0.3 | Creative but consistent architecture |
| RiskAgent | 0.1 | Compliance — lowest variance, cautious |
| AdrAgent | 0.4 | ADRs need creative alternative generation |
| CostAgent | 0.2 | Pricing — deterministic, reference-based |

---

## JSON Mode (Structured Outputs)

All agents use `response_format: { type: "json_object" }`. This eliminates markdown fence stripping, JSON repair, and partial-output handling — every agent response is directly deserializable.

Each agent has:
1. A system prompt with explicit JSON schema
2. A typed C# record for deserialization
3. A mapping function to the domain model

---

## Parallelism

```csharp
// Stage 2 — ~30s saved vs sequential
var archTask = architectAgent.DesignAsync(requirements, ct);
var riskTask = riskAgent.AssessAsync(requirements, ct);
await Task.WhenAll(archTask, riskTask);

// Stage 3 — ~25s saved vs sequential
var adrTask  = adrAgent.GenerateAsync(architecture, ct);
var costTask = costAgent.EstimateAsync(architecture, ct);
await Task.WhenAll(adrTask, costTask);
```

Total wall time ≈ Stage1 + Stage2 + Stage3 ≈ 20s + 30s + 25s = ~75s
vs sequential ≈ 20s + 30s + 25s + 20s + 25s = ~120s

---

## ZIP Output Format

The `zipBase64` field in the response contains a complete documentation package:

```
architecture-package.zip
├── 00-requirements.md   ← Structured requirements
├── 01-architecture.md   ← C4 design + Mermaid diagram + tech stack
├── 02-compliance.md     ← Compliance checklist table (✅ / ❌ / ⚠️)
├── 03-cost-estimate.md  ← Azure SKU breakdown in EUR
├── adr/
│   ├── ADR-001.md
│   ├── ADR-002.md
│   └── ADR-003.md
└── package.json         ← Full JSON (machine-readable)
```

Decode with: `[System.Convert]::FromBase64String($response.zipBase64) | Set-Content -Path package.zip -Encoding Byte`
