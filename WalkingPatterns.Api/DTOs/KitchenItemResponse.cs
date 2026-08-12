namespace WalkingPatterns.Api.DTOs;

public class KitchenItemResponse
{
    public int Id { get; set; }
    public string Source { get; set; } = "Kitchen";
    public int ProjectId { get; set; }
    public string? Parent { get; set; }
    public string? UtilityName { get; set; }
    public string? UtilityNameOld { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? Depth { get; set; }
    public string? Materials { get; set; }
    public string? Accessories { get; set; }
    public string? Quantities { get; set; }
    public string? AdditionalItems { get; set; }
    public double MaterialTotal { get; set; }
    public double AccessoriesTotal { get; set; }
    public double AdditionalItemsTotal { get; set; }
    public double TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
