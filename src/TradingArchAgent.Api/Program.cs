using Microsoft.SemanticKernel;
using TradingArchAgent.Api.Agents;
using TradingArchAgent.Api.Models;
using TradingArchAgent.Api.Orchestration;

var builder = WebApplication.CreateBuilder(args);

var aoaiEndpoint   = builder.Configuration["AzureOpenAI:Endpoint"]      ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is required");
var aoaiApiKey     = builder.Configuration["AzureOpenAI:ApiKey"]         ?? throw new InvalidOperationException("AzureOpenAI:ApiKey is required");
var aoaiDeployment = builder.Configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";

builder.Services.AddSingleton<Kernel>(_ =>
    Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(aoaiDeployment, aoaiEndpoint, aoaiApiKey)
        .Build());

builder.Services.AddSingleton<RequirementAgent>();
builder.Services.AddSingleton<ArchitectAgent>();
builder.Services.AddSingleton<RiskAgent>();
builder.Services.AddSingleton<AdrAgent>();
builder.Services.AddSingleton<CostAgent>();
builder.Services.AddSingleton<AgentOrchestrator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new()
    {
        Title       = "Trading Architecture Assistant",
        Version     = "v1",
        Description = "Multi-agent system that generates complete architecture packages for trading systems. Orchestrates 5 AI agents: Requirement → (Architect ‖ Risk) → (ADR ‖ Cost)"
    }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trading Architecture Assistant v1"));

app.MapPost("/api/architecture", async (ArchitectureRequest request, AgentOrchestrator orchestrator) =>
{
    if (string.IsNullOrWhiteSpace(request.Requirement))
        return Results.BadRequest(new { error = "requirement must not be empty" });

    var package = await orchestrator.RunAsync(request.Requirement);
    return Results.Ok(package);
})
.WithName("GenerateArchitecture")
.WithSummary("Generate a complete architecture package from a trading system requirement")
.Produces<ArchitecturePackage>(200)
.ProducesProblem(400);

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();

public record ArchitectureRequest(string Requirement);
