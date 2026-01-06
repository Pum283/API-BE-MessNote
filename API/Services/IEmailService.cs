namespace API.Services
{
    public interface IEmailService
    {
        Task<bool> SendOTPEmailAsync(string toEmail, string otpCode, string username);
        Task<bool> SendMessageEmailAsync(string toEmail, string recipientName, string subject, string htmlContent);
    }
}

