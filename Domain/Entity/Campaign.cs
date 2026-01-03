using Domain.Enums;

namespace Domain.Entity
{
    /// <summary>
    /// Chiến dịch gửi tin nhắn tự động
    /// </summary>
    public class Campaign : Domain.Common.BaseGuidEntity
    {
        /// <summary>
        /// Liên kết đến người dùng tạo và quản lý chiến dịch
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên chiến dịch để dễ nhận diện (ví dụ: "Nhắc nhở an toàn hàng ngày")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết mục đích và cách hoạt động của chiến dịch
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái chiến dịch (1=Active, 2=Paused, 3=Completed, 4=Deleted)
        /// </summary>
        public CampaignStatus Status { get; set; } = CampaignStatus.Active;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Recipient> Recipients { get; set; } = new List<Recipient>();
        public virtual ICollection<MessageTemplate> MessageTemplates { get; set; } = new List<MessageTemplate>();
        public virtual Schedule? Schedule { get; set; }
        public virtual ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();
        public virtual ICollection<History> Histories { get; set; } = new List<History>();
    }
}

