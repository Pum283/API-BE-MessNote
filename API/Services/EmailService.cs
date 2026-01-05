using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOTPEmailAsync(string toEmail, string otpCode, string username)
        {
            try
            {
                var mailSettings = _configuration.GetSection("MailSettings");
                var smtpHost = mailSettings["Host"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(mailSettings["Port"] ?? "587");
                var smtpMail = mailSettings["Mail"];
                var smtpPassword = mailSettings["Password"];
                var smtpFromName = mailSettings["DisplayName"] ?? "MessNote";

                if (string.IsNullOrEmpty(smtpMail) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("Mail settings not configured");
                    return false;
                }

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpMail, smtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpMail, smtpFromName),
                        Subject = "Mã OTP khôi phục mật khẩu - MessNote",
                        Body = GenerateOTPEmailBody(username, otpCode),
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"OTP email sent successfully to {toEmail}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {toEmail}");
                return false;
            }
        }

        private string GenerateOTPEmailBody(string username, string otpCode)
        {
            // SVG Logo inline - tone trắng đen
            var logoSvg = @"<svg viewBox='0 0 200 200' width='120' height='120' xmlns='http://www.w3.org/2000/svg' style='display: block; margin: 0 auto;'>
                <path d='M 20 15 Q 15 12 18 20 L 18 30 Q 15 35 20 40 L 30 40 Q 35 45 40 40 L 50 40 Q 55 35 60 40 L 140 40 Q 145 35 150 40 L 160 40 Q 165 35 170 40 L 180 40 Q 185 35 182 30 L 182 20 Q 185 12 180 15 L 170 15 Q 165 10 160 15 L 40 15 Q 35 10 30 15 Z M 20 15 L 20 185 Q 15 188 18 180 L 18 170 Q 15 165 20 160 L 30 160 Q 35 155 40 160 L 160 160 Q 165 155 170 160 L 180 160 Q 185 165 182 170 L 182 180 Q 185 188 180 185 L 20 185' fill='none' stroke='#000000' stroke-width='8' stroke-linecap='round' stroke-linejoin='round'/>
                <path d='M 40 60 Q 45 55 50 60 Q 55 58 60 60 Q 65 57 70 60 Q 75 59 80 60 Q 85 56 90 60 Q 95 58 100 60 Q 105 59 110 60 Q 115 57 120 60 Q 125 59 130 60 Q 135 58 140 60 Q 145 59 150 60 Q 155 57 160 60 L 160 140 Q 155 143 150 140 Q 145 142 140 140 Q 135 143 130 140 Q 125 141 120 140 Q 115 143 110 140 Q 105 142 100 140 Q 95 143 90 140 Q 85 141 80 140 Q 75 143 70 140 Q 65 142 60 140 Q 55 143 50 140 Q 45 141 40 140 Z' fill='#000000' opacity='0.95'/>
                <path d='M 60 75 Q 65 80 70 75' stroke='rgba(255,255,255,0.3)' stroke-width='1.5' fill='none' stroke-linecap='round'/>
                <path d='M 90 85 Q 95 90 100 85' stroke='rgba(255,255,255,0.3)' stroke-width='1.5' fill='none' stroke-linecap='round'/>
                <path d='M 120 70 Q 125 75 130 70' stroke='rgba(255,255,255,0.3)' stroke-width='1.5' fill='none' stroke-linecap='round'/>
                <path d='M 60 100 C 65 95 70 100 75 95 C 80 100 85 95 90 100 C 95 105 90 110 85 105 C 80 110 75 105 70 110 C 65 115 60 110 65 105 Z' fill='#FFFFFF'/>
                <path d='M 110 95 L 110 115 M 110 95 L 130 115 M 130 95 L 130 115' stroke='#FFFFFF' stroke-width='6' stroke-linecap='round' stroke-linejoin='round'/>
            </svg>";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen', 'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue', sans-serif;
            line-height: 1.6; 
            color: #1a1a1a; 
            background-color: #f5f5f5;
            margin: 0;
            padding: 0;
        }}
        .email-wrapper {{
            background-color: #f5f5f5;
            padding: 40px 20px;
        }}
        .container {{ 
            max-width: 600px; 
            margin: 0 auto; 
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}
        .header {{ 
            background-color: #ffffff; 
            padding: 40px 30px 30px 30px; 
            text-align: center; 
            border-bottom: 2px solid #e5e5e5;
        }}
        .logo-container {{
            margin-bottom: 20px;
        }}
        .header h1 {{
            color: #000000;
            font-size: 32px;
            font-weight: 700;
            margin: 0 0 8px 0;
            letter-spacing: -0.5px;
        }}
        .header p {{
            color: #666666;
            font-size: 14px;
            margin: 0;
        }}
        .content {{ 
            background-color: #ffffff; 
            padding: 40px 30px; 
        }}
        .content h2 {{
            color: #000000;
            font-size: 24px;
            font-weight: 600;
            margin: 0 0 20px 0;
        }}
        .content p {{
            color: #333333;
            font-size: 16px;
            line-height: 1.6;
            margin: 0 0 16px 0;
        }}
        .otp-box {{ 
            background-color: #f9f9f9; 
            border: 2px solid #000000; 
            padding: 30px 20px; 
            text-align: center; 
            margin: 30px 0; 
            border-radius: 8px;
        }}
        .otp-code {{ 
            font-size: 36px; 
            font-weight: 700; 
            color: #000000; 
            letter-spacing: 8px; 
            font-family: 'Courier New', monospace;
        }}
        .warning {{ 
            background-color: #f5f5f5; 
            border-left: 4px solid #000000; 
            padding: 20px; 
            margin: 30px 0; 
            border-radius: 4px;
        }}
        .warning strong {{
            color: #000000;
            font-size: 16px;
            display: block;
            margin-bottom: 12px;
        }}
        .warning ul {{
            margin: 0;
            padding-left: 20px;
            color: #333333;
        }}
        .warning li {{
            margin-bottom: 8px;
            line-height: 1.5;
        }}
        .footer {{ 
            background-color: #f9f9f9;
            text-align: center; 
            padding: 30px;
            border-top: 1px solid #e5e5e5;
            color: #666666; 
            font-size: 12px; 
        }}
        .footer p {{
            margin: 4px 0;
            color: #666666;
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='container'>
            <div class='header'>
                <div class='logo-container'>
                    {logoSvg}
                </div>
                <h1>MessNote</h1>
                <p>Hệ thống gửi tin nhắn tự động</p>
            </div>
            <div class='content'>
                <h2>Xin chào {username},</h2>
                <p>Bạn đã yêu cầu khôi phục mật khẩu cho tài khoản MessNote của mình.</p>
                <p>Vui lòng sử dụng mã OTP sau để xác nhận:</p>
                
                <div class='otp-box'>
                    <div class='otp-code'>{otpCode}</div>
                </div>
                
                <div class='warning'>
                    <strong>⚠️ Lưu ý quan trọng:</strong>
                    <ul>
                        <li>Mã OTP này có hiệu lực trong <strong style='color: #000000;'>10 phút</strong></li>
                        <li>Mã chỉ có thể sử dụng <strong style='color: #000000;'>một lần</strong></li>
                        <li>Không chia sẻ mã này với bất kỳ ai</li>
                        <li>Nếu bạn không yêu cầu khôi phục mật khẩu, vui lòng bỏ qua email này</li>
                    </ul>
                </div>
            </div>
            <div class='footer'>
                <p><strong style='color: #000000;'>© 2024 MessNote</strong></p>
                <p>All rights reserved.</p>
                <p style='margin-top: 12px;'>Email này được gửi tự động, vui lòng không trả lời.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}

