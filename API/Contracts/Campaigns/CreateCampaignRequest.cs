using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Campaigns
{
    public class CreateCampaignRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Status { get; set; } = 1; // CampaignStatus.Active
    }
}

