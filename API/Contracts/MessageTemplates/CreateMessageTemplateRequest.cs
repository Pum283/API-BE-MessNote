using System.ComponentModel.DataAnnotations;

namespace API.Contracts.MessageTemplates
{
    public class CreateMessageTemplateRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int Channel { get; set; } // MessageChannel enum

        public int SequenceOrder { get; set; } = 1;

        public string? Placeholders { get; set; }
    }
}

