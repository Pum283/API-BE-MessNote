namespace API.Responses
{
    public class ScheduleResponse
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public int ScheduleType { get; set; }
        public DateTime? FixedTime { get; set; }
        public string? CheckinInterval { get; set; }
        public string? OverdueThreshold { get; set; }
        public int Recurring { get; set; }
        public DateTime? LastCheckinTime { get; set; }
        public DateTime? NextSendTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

