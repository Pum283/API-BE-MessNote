namespace API.Responses
{
    public class RecipientResponse
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? FacebookId { get; set; }
        public int Level { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

