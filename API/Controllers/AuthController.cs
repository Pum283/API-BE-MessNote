using API.Contracts.Auth;
using API.Responses;
using API.Services;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

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
                return BadRequest("Username already exists");

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already exists");

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
                return Unauthorized("Invalid username/email or password");

            // Check if account is active
            if (!user.IsActive)
                return Unauthorized("Account is deactivated");

            // Check if user has password (not OAuth-only user)
            if (string.IsNullOrEmpty(user.PasswordHash))
                return Unauthorized("This account is linked to Google. Please use Google login.");

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid username/email or password");

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
                    return BadRequest(new { error = "IdToken is required" });
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
                    return Unauthorized(new { error = "Invalid Google token. Please try again." });
                }

                var email = payload.Email;
                var googleId = payload.Subject;
                var name = payload.Name;

                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { error = "Email is missing from Google account" });
                }

                if (string.IsNullOrEmpty(googleId))
                {
                    return BadRequest(new { error = "Google ID is missing" });
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
                    return Unauthorized(new { error = "Account is deactivated" });
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
                return StatusCode(500, new { error = "An error occurred during Google login" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new { error = "Email is required" });
                }

                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                // Always return success to prevent email enumeration attacks
                // But only send email if user exists
                if (user == null)
                {
                    _logger.LogWarning($"Forgot password requested for non-existent email: {request.Email}");
                    return Ok(new { message = "If the email exists, an OTP code has been sent." });
                }

                // Check if user has password (not OAuth-only user)
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return BadRequest(new { error = "This account is linked to Google. Please use Google login." });
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
                    return StatusCode(500, new { error = "Failed to send OTP email. Please try again later." });
                }

                _logger.LogInformation($"OTP sent successfully to {request.Email}");
                return Ok(new { message = "If the email exists, an OTP code has been sent." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password: {Message}", ex.Message);
                return StatusCode(500, new { error = "An error occurred. Please try again later." });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OTPCode))
                {
                    return BadRequest(new { error = "Email and OTP code are required" });
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

                    return BadRequest(new { error = "Invalid or expired OTP code" });
                }

                // Check attempt count
                if (otp.AttemptCount >= 5)
                {
                    otp.IsUsed = true;
                    await _context.SaveChangesAsync();
                    return BadRequest(new { error = "OTP code has been blocked due to too many failed attempts" });
                }

                return Ok(new { message = "OTP verified successfully", verified = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in verify OTP: {Message}", ex.Message);
                return StatusCode(500, new { error = "An error occurred. Please try again later." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OTPCode) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { error = "Email, OTP code, and new password are required" });
                }

                if (request.NewPassword.Length < 6)
                {
                    return BadRequest(new { error = "Password must be at least 6 characters long" });
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
                    return BadRequest(new { error = "Invalid or expired OTP code" });
                }

                // Check attempt count
                if (otp.AttemptCount >= 5)
                {
                    return BadRequest(new { error = "OTP code has been blocked due to too many failed attempts" });
                }

                // Find user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                // Mark OTP as used
                otp.IsUsed = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset successfully for user: {request.Email}");
                return Ok(new { message = "Password has been reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reset password: {Message}", ex.Message);
                return StatusCode(500, new { error = "An error occurred. Please try again later." });
            }
        }
    }
}

