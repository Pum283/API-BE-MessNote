namespace API.Responses
{
    public class MessageTemplateResponse
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Channel { get; set; }
        public int SequenceOrder { get; set; }
        public string? Placeholders { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

