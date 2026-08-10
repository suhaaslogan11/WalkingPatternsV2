using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalkingPatterns.Api.Models
{
    public class ProjectDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public ProjectVersionDetails Project { get; set; } = null!;

        [Required]
        public string RoomName { get; set; } = string.Empty;

        [Required]
        public string Woodwork { get; set; } = string.Empty;

        [Required]
        public string Accessories { get; set; } = string.Empty;

        [Required]
        public string Services { get; set; } = string.Empty;

        [Required]
        public string Total { get; set; } = string.Empty;

        [Required]
        public string Width { get; set; } = string.Empty;

        [Required]
        public string Height { get; set; } = string.Empty;

        [Required]
        public string Depth { get; set; } = string.Empty;
    }
}
