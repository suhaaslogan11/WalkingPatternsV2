using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalkingPatterns.Api.Models
{
    public class OrderDetails
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string ProjectName { get; set; } = string.Empty;

        public string? Parent { get; set; }
        public string? UtilityName { get; set; }
        public string? Materials { get; set; }
        public string? Accessories { get; set; }
        public string? Quantities { get; set; }
        public string? AdditionalItemName { get; set; }
        public string? AdditionalItemsAmounts { get; set; }
        public string? AdditionalItemsQuantities { get; set; }
        public double MaterialTotal { get; set; }
        public double AccessoriesTotal { get; set; }
        public double AdditionalItemsTotal { get; set; }
        public double TotalPrice { get; set; }

        public int ProjectId { get; set; }
        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? Depth { get; set; }
        public double GrandTotal { get; set; }
        public DateTime OrderDate { get; set; }
        public string? UtilityNameOld { get; set; }

        [Required]
        public int ProjectVersionDetailsId { get; set; }

        [ForeignKey(nameof(ProjectVersionDetailsId))]
        public ProjectVersionDetails ProjectVersionDetails { get; set; } = null!;
    }
}
