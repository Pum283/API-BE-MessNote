using Domain.Enums;

namespace API.Responses
{
    public class CampaignAnalyticsResponse
    {
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public CampaignStatus Status { get; set; }

        public int TotalEvents { get; set; }
        public int MessagesSent { get; set; }
        public int MessagesFailed { get; set; }
        public int MessagesPending { get; set; }

        public int CheckinsTotal { get; set; }
        public int CheckinsSuccess { get; set; }
        public int CheckinsFailed { get; set; }

        public int OverdueEvents { get; set; }

        public DateTime? LastEventAt { get; set; }
        public DateTime? LastCheckinAt { get; set; }
    }
}


