namespace API.Responses
{
    public class CheckinResponse
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public DateTime CheckinTime { get; set; }
        public int Method { get; set; }
        public string? Location { get; set; }
        public bool Success { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

