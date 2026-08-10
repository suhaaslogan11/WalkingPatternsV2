using System.ComponentModel.DataAnnotations;

namespace WalkingPatterns.Api.Models;

public class HDSPriceDetails
{
    [Key] public int Id { get; set; }
    public string? Parent { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? Depth { get; set; }
    public string? Materials { get; set; }
    public string? UtilityName { get; set; }
    public string? UtilityNameOld { get; set; }
    public string? AdditionalItemsAmounts { get; set; }
    public string? AdditionalItemsQuantities { get; set; }
    public string? ProjectName { get; set; }
    public double MaterialTotal { get; set; }
    public string? AdditionalItemName { get; set; }
    public double AdditionalItemsTotal { get; set; }
    public double TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
