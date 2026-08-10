using System.ComponentModel.DataAnnotations;

namespace WalkingPatterns.Api.Models
{
    public class ProjectVersionDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        public string projectDate { get; set; } = string.Empty;

        [Required]
        public string VersionNumber { get; set; } = string.Empty;

        [Required]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        public int ClientId { get; set; }

        public Client Client { get; set; } = null!;

        public double GrandTotal { get; set; }

        public double DiscountAmount { get; set; }

        public double DiscountedTotal { get; set; }
    }
}
