namespace WalkingPatterns.Api.DTOs
{
    public class ProjectResponse
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string projectDate { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string VersionNumber { get; set; } = string.Empty;
        public double GrandTotal { get; set; }
        public double DiscountAmount { get; set; }
        public double DiscountedTotal { get; set; }
    }
}
