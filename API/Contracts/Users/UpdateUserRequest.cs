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

        public bool? IsActive { get; set; }
    }
}

