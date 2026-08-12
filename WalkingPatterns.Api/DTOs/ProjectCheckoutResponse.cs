namespace WalkingPatterns.Api.DTOs;

public class ProjectCheckoutResponse
{
    public int CheckedOutItemCount { get; set; }
    public double CartTotal { get; set; }
    public double GrandTotal { get; set; }
    public double DiscountAmount { get; set; }
    public double DiscountedTotal { get; set; }
    public string VersionNumber { get; set; } = string.Empty;
}
