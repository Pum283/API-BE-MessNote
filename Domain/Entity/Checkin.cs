using Domain.Enums;

namespace Domain.Entity
{
    /// <summary>
    /// Lịch sử điểm danh
    /// </summary>
    public class Checkin : Domain.Common.BaseGuidEntity
    {
        /// <summary>
        /// Liên kết đến chiến dịch
        /// </summary>
        public Guid CampaignId { get; set; }

        /// <summary>
        /// Thời điểm thực hiện điểm danh
        /// </summary>
        public DateTime CheckinTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Phương thức điểm danh (1=Link, 2=SMS Reply, 3=Email Button, 4=App Push)
        /// </summary>
        public CheckinMethod Method { get; set; }

        /// <summary>
        /// Vị trí GPS nếu có (lưu dạng JSON text: {"lat":..., "long":...})
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Điểm danh có thành công hay không
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Ghi chú bổ sung (lý do thất bại hoặc thông tin thêm)
        /// </summary>
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Campaign Campaign { get; set; } = null!;
    }
}

