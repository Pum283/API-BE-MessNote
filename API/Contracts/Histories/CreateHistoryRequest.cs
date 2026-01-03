using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Histories
{
    public class CreateHistoryRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public int EventType { get; set; } // HistoryEventType enum

        public Guid? RecipientId { get; set; }

        [Required]
        public int Status { get; set; } // HistoryStatus enum

        public string? Details { get; set; }
    }
}

