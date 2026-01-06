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
        
        // Google OAuth fields
        public string? GoogleId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? Picture { get; set; }
        public string? Locale { get; set; }
        public bool? EmailVerified { get; set; }
    }
}

