using Domain.Enums;

namespace Domain.Entity
{
    /// <summary>
    /// Lịch sử sự kiện (gửi tin, điểm danh, quá hạn)
    /// </summary>
    public class History : Domain.Common.BaseGuidEntity
    {
        /// <summary>
        /// Liên kết đến chiến dịch
        /// </summary>
        public Guid CampaignId { get; set; }

        /// <summary>
        /// Loại sự kiện (1=SendMessage, 2=Checkin, 3=Overdue)
        /// </summary>
        public HistoryEventType EventType { get; set; }

        /// <summary>
        /// Thời gian xảy ra sự kiện
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Người nhận liên quan (nếu sự kiện gửi tin)
        /// </summary>
        public Guid? RecipientId { get; set; }

        /// <summary>
        /// Trạng thái xử lý (1=Success, 2=Failed, 3=Pending)
        /// </summary>
        public HistoryStatus Status { get; set; }

        /// <summary>
        /// Chi tiết bổ sung (lỗi API, nội dung gửi, lưu dạng JSON text)
        /// </summary>
        public string? Details { get; set; }

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
        public virtual Recipient? Recipient { get; set; }
    }
}

