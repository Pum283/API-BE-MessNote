using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Checkins
{
    public class CreateCheckinRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public int Method { get; set; } // CheckinMethod enum

        public string? Location { get; set; }

        public bool Success { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}

