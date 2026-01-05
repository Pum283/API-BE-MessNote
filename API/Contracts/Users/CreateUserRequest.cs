using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Users
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò người dùng (chỉ Admin mới có thể tạo User với role khác User)
        /// </summary>
        public UserRole? Role { get; set; }
    }
}

