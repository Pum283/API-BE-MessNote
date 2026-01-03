using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Schedules
{
    public class UpdateScheduleRequest
    {
        public int? ScheduleType { get; set; }

        public DateTime? FixedTime { get; set; }

        [StringLength(50)]
        public string? CheckinInterval { get; set; }

        [StringLength(50)]
        public string? OverdueThreshold { get; set; }

        public int? Recurring { get; set; }

        public DateTime? LastCheckinTime { get; set; }

        public DateTime? NextSendTime { get; set; }
    }
}

