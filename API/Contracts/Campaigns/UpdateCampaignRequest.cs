using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Campaigns
{
    public class UpdateCampaignRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public int? Status { get; set; }
    }
}

