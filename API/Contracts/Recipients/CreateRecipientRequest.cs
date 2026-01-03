using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Recipients
{
    public class CreateRecipientRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? FacebookId { get; set; }

        public int Level { get; set; } = 1;
    }
}

