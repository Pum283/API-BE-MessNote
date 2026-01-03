using Domain.Enums;

namespace Domain.Entity
{
    /// <summary>
    /// Mẫu tin nhắn (hỗ trợ chuỗi)
    /// </summary>
    public class MessageTemplate : Domain.Common.BaseGuidEntity
    {
        /// <summary>
        /// Liên kết đến chiến dịch sử dụng mẫu này
        /// </summary>
        public Guid CampaignId { get; set; }

        /// <summary>
        /// Nội dung tin nhắn đầy đủ, hỗ trợ placeholder ({name}, {time}, {location}...)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Kênh gửi (1=SMS, 2=Email, 3=Facebook, 4=All)
        /// </summary>
        public MessageChannel Channel { get; set; }

        /// <summary>
        /// Thứ tự trong chuỗi tin nhắn (1=tin đầu, 2=cảnh báo thứ hai...)
        /// </summary>
        public int SequenceOrder { get; set; } = 1;

        /// <summary>
        /// Dữ liệu placeholder tùy chỉnh (lưu dạng JSON text để linh hoạt)
        /// </summary>
        public string? Placeholders { get; set; }

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
    }
}

