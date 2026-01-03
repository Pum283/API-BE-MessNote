namespace Domain.Entity
{
    /// <summary>
    /// Người nhận tin nhắn
    /// </summary>
    public class Recipient : Domain.Common.BaseGuidEntity
    {
        /// <summary>
        /// Liên kết đến chiến dịch mà người nhận thuộc về
        /// </summary>
        public Guid CampaignId { get; set; }

        /// <summary>
        /// Tên người nhận để cá nhân hóa tin nhắn (placeholder {name})
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Số điện thoại để gửi SMS
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Địa chỉ email để gửi mail
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// ID Facebook để gửi Messenger
        /// </summary>
        public string? FacebookId { get; set; }

        /// <summary>
        /// Cấp độ khẩn cấp (1=cơ bản, 2=gia đình, 3=cứu hộ...) để gửi theo thứ tự ưu tiên
        /// </summary>
        public int Level { get; set; } = 1;

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
        public virtual ICollection<History> Histories { get; set; } = new List<History>();
    }
}

