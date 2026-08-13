namespace WalkingPatterns.Api.DTOs;

public class BedroomPricingResponse
{
    public Dictionary<string, Dictionary<string, Dictionary<string, double>>> PricingData { get; set; } = new();
}

public class BedroomItemRequest
{
    public string? Parent { get; set; }
    public string? UtilityName { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? Depth { get; set; }
    public string? CoreMaterial { get; set; }
    public string? ShutterMaterial { get; set; }
    public List<BedroomAdditionalItemRequest> AdditionalItems { get; set; } = new();
    public string? UtilityNameOld { get; set; }
}

public class BedroomAdditionalItemRequest
{
    public string? Name { get; set; }
    public double Amount { get; set; }
    public int Quantity { get; set; }
}

public class BedroomItemResponse
{
    public int Id { get; set; }
    public string Source { get; set; } = "Bedroom";
    public int ProjectId { get; set; }
    public string? Parent { get; set; }
    public string? UtilityName { get; set; }
    public string? UtilityNameOld { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? Depth { get; set; }
    public string? Materials { get; set; }
    public string? AdditionalItems { get; set; }
    public double MaterialTotal { get; set; }
    public double AccessoriesTotal { get; set; }
    public double AdditionalItemsTotal { get; set; }
    public double TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
