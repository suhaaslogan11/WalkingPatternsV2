using System.ComponentModel.DataAnnotations;

namespace WalkingPatterns.Api.DTOs
{
    public class AddProjectRequest
    {
        [Required]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        public string projectDate { get; set; } = string.Empty;

        public string? VersionNumber { get; set; }
    }
}
