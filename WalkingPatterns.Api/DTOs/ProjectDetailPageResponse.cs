namespace WalkingPatterns.Api.DTOs
{
    public class ProjectDetailPageResponse
    {
        public int ProjectId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string projectDate { get; set; } = string.Empty;
        public string VersionNumber { get; set; } = string.Empty;
        public List<ModuleSummaryResponse> Modules { get; set; } = new();
    }
}
