namespace API.Responses
{
    public class HistoryResponse
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public int EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? RecipientId { get; set; }
        public int Status { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

