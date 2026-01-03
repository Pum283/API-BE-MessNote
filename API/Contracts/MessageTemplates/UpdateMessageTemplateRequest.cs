using System.ComponentModel.DataAnnotations;

namespace API.Contracts.MessageTemplates
{
    public class UpdateMessageTemplateRequest
    {
        public string? Content { get; set; }

        public int? Channel { get; set; }

        public int? SequenceOrder { get; set; }

        public string? Placeholders { get; set; }
    }
}

