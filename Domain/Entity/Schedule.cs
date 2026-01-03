using Domain.Enums;

namespace Domain.Entity
{
    /// <summary>
    /// Lịch trình gửi & điểm danh
    /// </summary>
    public class Schedule : Domain.Common.BaseGuidEntity
    {
        /// <summary>
        /// Liên kết đến chiến dịch
        /// </summary>
        public Guid CampaignId { get; set; }

        /// <summary>
        /// Loại lịch (1=Fixed, 2=Checkin)
        /// </summary>
        public ScheduleType ScheduleType { get; set; }

        /// <summary>
        /// Thời gian gửi cố định (chỉ dùng khi type=Fixed)
        /// </summary>
        public DateTime? FixedTime { get; set; }

        /// <summary>
        /// Khoảng cách giữa các lần điểm danh (ví dụ: '4 hours', '1 day')
        /// </summary>
        public string? CheckinInterval { get; set; }

        /// <summary>
        /// Ngưỡng quá hạn trước khi gửi tin (ví dụ: '30 minutes')
        /// </summary>
        public string? OverdueThreshold { get; set; }

        /// <summary>
        /// Lặp lại (1=None, 2=Daily, 3=Weekly, 4=Monthly)
        /// </summary>
        public RecurringType Recurring { get; set; } = RecurringType.None;

        /// <summary>
        /// Thời điểm điểm danh thành công gần nhất
        /// </summary>
        public DateTime? LastCheckinTime { get; set; }

        /// <summary>
        /// Thời điểm dự kiến gửi tin tiếp theo (tính toán tự động)
        /// </summary>
        public DateTime? NextSendTime { get; set; }

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
    }
}

