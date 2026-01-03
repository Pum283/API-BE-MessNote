using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Checkins
{
    public class UpdateCheckinRequest
    {
        public int? Method { get; set; }

        public string? Location { get; set; }

        public bool? Success { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}

