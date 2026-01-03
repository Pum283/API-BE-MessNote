using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Schedules
{
    public class CreateScheduleRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public int ScheduleType { get; set; } // ScheduleType enum

        public DateTime? FixedTime { get; set; }

        [StringLength(50)]
        public string? CheckinInterval { get; set; }

        [StringLength(50)]
        public string? OverdueThreshold { get; set; }

        public int Recurring { get; set; } = 1; // RecurringType.None
    }
}

