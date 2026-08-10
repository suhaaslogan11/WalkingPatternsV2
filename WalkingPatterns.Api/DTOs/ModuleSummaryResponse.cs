namespace WalkingPatterns.Api.DTOs
{
    public class ModuleSummaryResponse
    {
        public int ProjectDetailId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public decimal Woodwork { get; set; }
        public decimal Accessories { get; set; }
        public decimal Services { get; set; }
        public decimal Total { get; set; }
    }
}
