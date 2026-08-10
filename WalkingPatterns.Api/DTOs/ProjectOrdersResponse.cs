namespace WalkingPatterns.Api.DTOs
{
    public class ProjectOrdersResponse
    {
        public int ProjectDetailId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public List<OrderDetailResponse> Orders { get; set; } = new();
    }
}
