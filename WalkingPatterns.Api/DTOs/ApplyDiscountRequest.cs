using System.ComponentModel.DataAnnotations;

namespace WalkingPatterns.Api.DTOs
{
    public class ApplyDiscountRequest
    {
        [Range(0, double.MaxValue)]
        public double DiscountAmount { get; set; }
    }
}
