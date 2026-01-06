using API.Contracts.Auth;
using API.Responses;
using API.Services;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IJwtService jwtService, IEmailService emailService, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Tên người dùng đã tồn tại");

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email đã tồn tại");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email, user.Role);

            return Ok(new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                Token = token
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // Find user by username or email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

            if (user == null)
                return Unauthorized("Tên đăng nhập/email hoặc mật khẩu không hợp lệ");

            // Check if account is active
            if (!user.IsActive)
                return Unauthorized("Tài khoản đã bị vô hiệu hóa");

            // Check if user has password (not OAuth-only user)
            if (string.IsNullOrEmpty(user.PasswordHash))
                return Unauthorized("Tài khoản này được liên kết với Google. Vui lòng sử dụng đăng nhập Google.");

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Tên đăng nhập/email hoặc mật khẩu không hợp lệ");

            var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email, user.Role);

            return Ok(new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                Token = token
            });
        }

        [HttpPost("google")]
        public async Task<ActionResult<LoginResponse>> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest(new { error = "IdToken là bắt buộc" });
                }

                var googleClientId = _configuration["GoogleAuthSettings:ClientId"] 
                    ?? throw new InvalidOperationException("Google ClientId is not configured");

                // Validate Google id_token
                GoogleJsonWebSignature.Payload payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { googleClientId }
                    });
                }
                catch (InvalidJwtException ex)
                {
                    _logger.LogError(ex, "Invalid Google JWT token");
                    return Unauthorized(new { error = "Token Google không hợp lệ. Vui lòng thử lại." });
                }

                var email = payload.Email;
                var googleId = payload.Subject;
                var name = payload.Name;

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { error = "Email bị thiếu từ tài khoản Google" });
                }

                if (string.IsNullOrEmpty(googleId))
                {
                    return BadRequest(new { error = "Google ID bị thiếu" });
                }

                _logger.LogInformation("Google login attempt for email: {Email}, GoogleId: {GoogleId}", email, googleId);

                // Find user by GoogleId first, then by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

                if (user == null)
                {
                    // Try to find by email (might be existing user)
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    
                    if (user != null)
                    {
                        // Link Google account to existing user
                        user.GoogleId = googleId;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Linked Google account to existing user: {Email}", email);
                    }
                    else
                    {
                        // Create new user from Google account
                        var username = email.Split('@')[0]; // Use email prefix as username
                        // Ensure username is unique
                        var baseUsername = username;
                        var counter = 1;
                        while (await _context.Users.AnyAsync(u => u.Username == username))
                        {
                            username = $"{baseUsername}{counter}";
                            counter++;
                        }

                        user = new User
                        {
                            Username = username,
                            Email = email,
                            GoogleId = googleId,
                            PasswordHash = null, // No password for OAuth users
                            IsActive = true
                        };

                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Created new user from Google account: {Email}, Username: {Username}", email, username);
                    }
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    return Unauthorized(new { error = "Tài khoản đã bị vô hiệu hóa" });
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email, user.Role);

                return Ok(new LoginResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Google login: {Message}", ex.Message);
                return StatusCode(500, new { error = "Đã xảy ra lỗi trong quá trình đăng nhập Google" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new { error = "Email là bắt buộc" });
                }

                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                // Check if user exists
                if (user == null)
                {
                    _logger.LogWarning($"Forgot password requested for non-existent email: {request.Email}");
                    return BadRequest(new { error = "Email_Not_Found", message = "Email không tồn tại trong hệ thống" });
                }

                // Check if user has password (not OAuth-only user)
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return BadRequest(new { error = "Tài khoản này được liên kết với Google. Vui lòng sử dụng đăng nhập Google." });
                }

                // Generate 6-digit OTP
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                // Invalidate previous OTPs for this email
                var previousOTPs = await _context.PasswordResetOTPs
                    .Where(o => o.Email == request.Email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var previousOtp in previousOTPs)
                {
                    previousOtp.IsUsed = true;
                }

                // Create new OTP
                var otp = new PasswordResetOTP
                {
                    Email = request.Email,
                    OTPCode = otpCode,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10), // OTP expires in 10 minutes
                    IsUsed = false,
                    AttemptCount = 0
                };

                _context.PasswordResetOTPs.Add(otp);
                await _context.SaveChangesAsync();

                // Send OTP email
                var emailSent = await _emailService.SendOTPEmailAsync(request.Email, otpCode, user.Username);

                if (!emailSent)
                {
                    _logger.LogError($"Failed to send OTP email to {request.Email}");
                    return StatusCode(500, new { error = "Không thể gửi email OTP. Vui lòng thử lại sau." });
                }

                _logger.LogInformation($"OTP sent successfully to {request.Email}");
                return Ok(new { message = "Nếu email tồn tại, mã OTP đã được gửi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password: {Message}", ex.Message);
                return StatusCode(500, new { error = "Đã xảy ra lỗi. Vui lòng thử lại sau." });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OTPCode))
                {
                    return BadRequest(new { error = "Email và mã OTP là bắt buộc" });
                }

                // Find valid OTP
                var otp = await _context.PasswordResetOTPs
                    .Where(o => o.Email == request.Email 
                        && o.OTPCode == request.OTPCode 
                        && !o.IsUsed 
                        && o.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    // Increment attempt count for recent OTPs
                    var recentOTPs = await _context.PasswordResetOTPs
                        .Where(o => o.Email == request.Email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                        .ToListAsync();

                    foreach (var recentOtp in recentOTPs)
                    {
                        recentOtp.AttemptCount++;
                        if (recentOtp.AttemptCount >= 5)
                        {
                            recentOtp.IsUsed = true; // Block after 5 failed attempts
                        }
                    }
                    await _context.SaveChangesAsync();

                    return BadRequest(new { error = "Mã OTP không hợp lệ hoặc đã hết hạn" });
                }

                // Check attempt count
                if (otp.AttemptCount >= 5)
                {
                    otp.IsUsed = true;
                    await _context.SaveChangesAsync();
                    return BadRequest(new { error = "Mã OTP đã bị chặn do quá nhiều lần thử sai" });
                }

                return Ok(new { message = "Xác minh OTP thành công", verified = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in verify OTP: {Message}", ex.Message);
                return StatusCode(500, new { error = "Đã xảy ra lỗi. Vui lòng thử lại sau." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OTPCode) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { error = "Email, mã OTP và mật khẩu mới là bắt buộc" });
                }

                if (request.NewPassword.Length < 6)
                {
                    return BadRequest(new { error = "Mật khẩu phải có ít nhất 6 ký tự" });
                }

                // Find valid OTP
                var otp = await _context.PasswordResetOTPs
                    .Where(o => o.Email == request.Email 
                        && o.OTPCode == request.OTPCode 
                        && !o.IsUsed 
                        && o.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    return BadRequest(new { error = "Mã OTP không hợp lệ hoặc đã hết hạn" });
                }

                // Check attempt count
                if (otp.AttemptCount >= 5)
                {
                    return BadRequest(new { error = "Mã OTP đã bị chặn do quá nhiều lần thử sai" });
                }

                // Find user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return BadRequest(new { error = "Không tìm thấy người dùng" });
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                // Mark OTP as used
                otp.IsUsed = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset successfully for user: {request.Email}");
                return Ok(new { message = "Đặt lại mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reset password: {Message}", ex.Message);
                return StatusCode(500, new { error = "Đã xảy ra lỗi. Vui lòng thử lại sau." });
            }
        }

        /// <summary>
        /// Đổi mật khẩu (yêu cầu đăng nhập)
        /// </summary>
        [HttpPost("change-password")]
        [Authorize(Policy = "Authenticated")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { error = "Mật khẩu hiện tại và mật khẩu mới là bắt buộc" });
                }

                if (request.NewPassword.Length < 6)
                {
                    return BadRequest(new { error = "Mật khẩu mới phải có ít nhất 6 ký tự" });
                }

                // Get current user ID from token
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "Không thể xác định người dùng" });
                }

                // Find user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "Không tìm thấy người dùng" });
                }

                // Check if account is linked to Google
                if (!string.IsNullOrEmpty(user.GoogleId))
                {
                    return BadRequest(new { error = "Tài khoản này được liên kết với Google. Không thể đổi mật khẩu." });
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return Unauthorized(new { error = "Mật khẩu hiện tại không đúng" });
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password changed successfully for user: {user.Email}");
                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in change password: {Message}", ex.Message);
                return StatusCode(500, new { error = "Đã xảy ra lỗi. Vui lòng thử lại sau." });
            }
        }
    }
}

