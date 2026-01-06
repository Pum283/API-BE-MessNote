using API.Services;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(
            IEmailService emailService,
            AppDbContext context,
            ILogger<TestController> logger)
        {
            _emailService = emailService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> TestSendEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var success = await _emailService.SendMessageEmailAsync(
                    request.Email,
                    request.RecipientName ?? "Test User",
                    request.Subject ?? "Test Email from MessNote",
                    request.Content ?? "<p>This is a test email.</p>"
                );

                if (success)
                {
                    return Ok(new { message = "Email sent successfully", email = request.Email });
                }
                else
                {
                    return BadRequest(new { error = "Failed to send email", email = request.Email });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("check-schedules")]
        public async Task<IActionResult> CheckSchedules()
        {
            try
            {
                var now = DateTime.UtcNow;
                var schedules = await _context.Schedules
                    .Include(s => s.Campaign)
                        .ThenInclude(c => c.MessageTemplates)
                    .Include(s => s.Campaign)
                        .ThenInclude(c => c.Recipients)
                    .Where(s => s.Campaign.Status == CampaignStatus.Active)
                    .Select(s => new
                    {
                        ScheduleId = s.Id,
                        CampaignId = s.CampaignId,
                        CampaignName = s.Campaign.Name,
                        ScheduleType = s.ScheduleType,
                        FixedTime = s.FixedTime,
                        NextSendTime = s.NextSendTime,
                        Recurring = s.Recurring,
                        HasEmailTemplates = s.Campaign.MessageTemplates.Any(t => t.Channel == MessageChannel.Email || t.Channel == MessageChannel.All),
                        EmailRecipientsCount = s.Campaign.Recipients.Count(r => !string.IsNullOrEmpty(r.Email)),
                        ShouldSendNow = s.NextSendTime.HasValue && s.NextSendTime.Value <= now && s.NextSendTime.Value > now.AddMinutes(-5),
                        CurrentTime = now
                    })
                    .ToListAsync();

                return Ok(new
                {
                    currentTime = now,
                    schedules = schedules
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking schedules");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class TestEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? RecipientName { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }
}

