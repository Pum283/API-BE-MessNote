using Domain.Enums;

namespace API.Services
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string username, string email, UserRole role);
    }
}

