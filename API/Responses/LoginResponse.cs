using Domain.Enums;

namespace API.Responses
{
    public class LoginResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}

