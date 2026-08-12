namespace WalkingPatterns.Api.DTOs;

public class KitchenItemRequest
{
    public string? Parent { get; set; }
    public string? UtilityName { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? Depth { get; set; }
    public string? Materials { get; set; }
    public List<string> Accessories { get; set; } = new();
    public List<string> Quantities { get; set; } = new();
    public List<KitchenAdditionalItemRequest> AdditionalItems { get; set; } = new();
    public string? UtilityNameOld { get; set; }
}

public class KitchenAdditionalItemRequest
{
    public string? Name { get; set; }
    public double Amount { get; set; }
    public int Quantity { get; set; }
}
