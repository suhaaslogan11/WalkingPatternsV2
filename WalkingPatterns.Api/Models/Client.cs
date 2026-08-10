using System.ComponentModel.DataAnnotations;

namespace WalkingPatterns.Api.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ClientName { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string? Address { get; set; }

        public ICollection<ProjectVersionDetails> ProjectVersions { get; set; } = new List<ProjectVersionDetails>();
    }
}
