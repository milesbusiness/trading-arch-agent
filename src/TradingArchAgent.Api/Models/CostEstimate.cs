namespace TradingArchAgent.Api.Models;

public class CostEstimate
{
    public decimal MonthlyTotalEur { get; set; }
    public decimal AnnualTotalEur  => MonthlyTotalEur * 12;
    public List<CostLineItem> Breakdown { get; set; } = [];
    public string Currency     { get; set; } = "EUR";
    public string Region       { get; set; } = "West Europe";
    public string Disclaimer   { get; set; } = "Estimates based on Azure public pricing. Actual costs depend on reserved instance discounts, egress patterns, and EA agreements.";
    public List<string> CostOptimisationOpportunities { get; set; } = [];
}

public class CostLineItem
{
    public string Service     { get; set; } = string.Empty;
    public string Tier        { get; set; } = string.Empty;
    public string Quantity    { get; set; } = string.Empty;
    public decimal MonthlyEur { get; set; }
    public string Notes       { get; set; } = string.Empty;
}
