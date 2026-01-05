using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Users
{
    public class UpdateUserRequest
    {
        [StringLength(50)]
        public string? Username { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        /// <summary>
        /// Vai trò người dùng (chỉ Admin mới có thể thay đổi)
        /// </summary>
        public UserRole? Role { get; set; }

        public bool? IsActive { get; set; }
    }
}

