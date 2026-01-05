namespace API.Services
{
    public interface IEmailService
    {
        Task<bool> SendOTPEmailAsync(string toEmail, string otpCode, string username);
    }
}

