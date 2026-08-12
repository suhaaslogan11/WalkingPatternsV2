namespace WalkingPatterns.Api.DTOs;

public class KitchenPricingResponse
{
    public Dictionary<string, double> Materials { get; set; } = new();
    public Dictionary<string, List<KitchenAccessoryOption>> ParentOptions { get; set; } = new();
}

public class KitchenAccessoryOption
{
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
}
