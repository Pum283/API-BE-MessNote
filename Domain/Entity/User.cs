using Domain.Enums;

namespace Domain.Entity
{
    /// <summary>
    /// Người dùng hệ thống (người thiết lập chiến dịch)
    /// </summary>
    public class User : Domain.Common.BaseGuidEntity
    {
        /// <summary>
        /// Tên đăng nhập duy nhất để xác thực
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email duy nhất, dùng để đăng nhập, khôi phục mật khẩu và nhận thông báo
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu đã được hash để bảo mật cao (nullable cho OAuth users)
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Google ID cho OAuth authentication (nullable)
        /// </summary>
        public string? GoogleId { get; set; }

        /// <summary>
        /// Vai trò người dùng (User, Enterprise, Admin)
        /// </summary>
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// Trạng thái tài khoản còn hoạt động hay đã khóa
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    }
}

