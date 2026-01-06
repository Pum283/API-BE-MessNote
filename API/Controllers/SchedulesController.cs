using API.Contracts.Schedules;
using API.Responses;
using API.Services;
using Domain.Entity;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<SchedulesController> _logger;

        public SchedulesController(
            AppDbContext context,
            IEmailService emailService,
            ILogger<SchedulesController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ScheduleResponse>>> GetAll([FromQuery] Guid? campaignId = null)
        {
            var query = _context.Schedules.AsQueryable();

            if (campaignId.HasValue)
                query = query.Where(s => s.CampaignId == campaignId.Value);

            var schedules = await query
                .Select(s => new ScheduleResponse
                {
                    Id = s.Id,
                    CampaignId = s.CampaignId,
                    ScheduleType = (int)s.ScheduleType,
                    FixedTime = s.FixedTime,
                    CheckinInterval = s.CheckinInterval,
                    OverdueThreshold = s.OverdueThreshold,
                    Recurring = (int)s.Recurring,
                    LastCheckinTime = s.LastCheckinTime,
                    NextSendTime = s.NextSendTime,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleResponse>> GetById(Guid id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return NotFound();

            return Ok(new ScheduleResponse
            {
                Id = schedule.Id,
                CampaignId = schedule.CampaignId,
                ScheduleType = (int)schedule.ScheduleType,
                FixedTime = schedule.FixedTime,
                CheckinInterval = schedule.CheckinInterval,
                OverdueThreshold = schedule.OverdueThreshold,
                Recurring = (int)schedule.Recurring,
                LastCheckinTime = schedule.LastCheckinTime,
                NextSendTime = schedule.NextSendTime,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<ScheduleResponse>> Create([FromBody] CreateScheduleRequest request)
        {
            // Verify campaign exists
            if (!await _context.Campaigns.AnyAsync(c => c.Id == request.CampaignId))
                return BadRequest("Campaign not found");

            // Check if schedule already exists for this campaign
            if (await _context.Schedules.AnyAsync(s => s.CampaignId == request.CampaignId))
                return BadRequest("Schedule already exists for this campaign");

            var schedule = new Schedule
            {
                CampaignId = request.CampaignId,
                ScheduleType = (ScheduleType)request.ScheduleType,
                FixedTime = request.FixedTime,
                CheckinInterval = request.CheckinInterval,
                OverdueThreshold = request.OverdueThreshold,
                Recurring = (RecurringType)request.Recurring,
                NextSendTime = request.FixedTime // Set NextSendTime to FixedTime for fixed schedules
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, new ScheduleResponse
            {
                Id = schedule.Id,
                CampaignId = schedule.CampaignId,
                ScheduleType = (int)schedule.ScheduleType,
                FixedTime = schedule.FixedTime,
                CheckinInterval = schedule.CheckinInterval,
                OverdueThreshold = schedule.OverdueThreshold,
                Recurring = (int)schedule.Recurring,
                LastCheckinTime = schedule.LastCheckinTime,
                NextSendTime = schedule.NextSendTime,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ScheduleResponse>> Update(Guid id, [FromBody] UpdateScheduleRequest request)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return NotFound();

            if (request.ScheduleType.HasValue)
                schedule.ScheduleType = (ScheduleType)request.ScheduleType.Value;

            if (request.FixedTime.HasValue)
            {
                schedule.FixedTime = request.FixedTime;
                // Update NextSendTime when FixedTime is updated
                schedule.NextSendTime = request.FixedTime;
            }

            if (request.CheckinInterval != null)
                schedule.CheckinInterval = request.CheckinInterval;

            if (request.OverdueThreshold != null)
                schedule.OverdueThreshold = request.OverdueThreshold;

            if (request.Recurring.HasValue)
            {
                schedule.Recurring = (RecurringType)request.Recurring.Value;
                // If FixedTime exists and recurring changed, recalculate NextSendTime
                if (schedule.FixedTime.HasValue)
                {
                    schedule.NextSendTime = schedule.FixedTime.Value;
                }
            }

            if (request.LastCheckinTime.HasValue)
                schedule.LastCheckinTime = request.LastCheckinTime;

            if (request.NextSendTime.HasValue)
                schedule.NextSendTime = request.NextSendTime;

            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ScheduleResponse
            {
                Id = schedule.Id,
                CampaignId = schedule.CampaignId,
                ScheduleType = (int)schedule.ScheduleType,
                FixedTime = schedule.FixedTime,
                CheckinInterval = schedule.CheckinInterval,
                OverdueThreshold = schedule.OverdueThreshold,
                Recurring = (int)schedule.Recurring,
                LastCheckinTime = schedule.LastCheckinTime,
                NextSendTime = schedule.NextSendTime,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return NotFound();

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/send-now")]
        public async Task<IActionResult> SendNow(Guid id)
        {
            try
            {
                var schedule = await _context.Schedules
                    .Include(s => s.Campaign)
                        .ThenInclude(c => c.MessageTemplates)
                    .Include(s => s.Campaign)
                        .ThenInclude(c => c.Recipients)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (schedule == null)
                    return NotFound();

                _logger.LogInformation(
                    $"SendNow requested for schedule {id}, campaign {schedule.CampaignId}. " +
                    $"Campaign has {schedule.Campaign.MessageTemplates.Count} templates total");

                if (schedule.Campaign.Status != CampaignStatus.Active)
                    return BadRequest("Campaign is not active");

                var campaign = schedule.Campaign;
                var now = DateTime.UtcNow;

                // Get email templates
                var emailTemplates = campaign.MessageTemplates
                    .Where(t => t.Channel == MessageChannel.Email || t.Channel == MessageChannel.All)
                    .OrderBy(t => t.SequenceOrder)
                    .ToList();

                if (!emailTemplates.Any())
                {
                    var allTemplates = campaign.MessageTemplates.ToList();
                    
                    // If no templates at all, try to create one from campaign description or name
                    if (allTemplates.Count == 0)
                    {
                        _logger.LogWarning(
                            $"No templates found for campaign {campaign.Id}. " +
                            $"Creating a temporary email template from campaign description.");
                        
                        // Create a temporary email template
                        var tempTemplate = new MessageTemplate
                        {
                            CampaignId = campaign.Id,
                            Content = string.IsNullOrEmpty(campaign.Description) 
                                ? $"Xin chào, đây là tin nhắn từ {campaign.Name}" 
                                : campaign.Description,
                            Channel = MessageChannel.Email,
                            SequenceOrder = 1
                        };
                        
                        _context.MessageTemplates.Add(tempTemplate);
                        await _context.SaveChangesAsync();
                        
                        emailTemplates = new List<MessageTemplate> { tempTemplate };
                        _logger.LogInformation($"Created temporary email template {tempTemplate.Id} for campaign {campaign.Id}");
                    }
                    else
                    {
                        var templateChannels = string.Join(", ", allTemplates.Select(t => $"Channel: {t.Channel}"));
                        _logger.LogWarning(
                            $"No email templates found for campaign {campaign.Id}. " +
                            $"Total templates: {allTemplates.Count}. " +
                            $"Channels: {templateChannels}");
                        
                        return BadRequest(new
                        {
                            error = "Không tìm thấy mẫu tin nhắn email cho chiến dịch này",
                            message = $"Chiến dịch có {allTemplates.Count} mẫu tin nhắn nhưng không có mẫu nào với kênh Email hoặc Tất cả. " +
                                     $"Các kênh hiện có: {templateChannels}. " +
                                     "Vui lòng tạo mẫu tin nhắn với kênh 'Email' hoặc 'Tất cả' để có thể gửi email.",
                            totalTemplates = allTemplates.Count,
                            availableChannels = allTemplates.Select(t => t.Channel.ToString()).Distinct().ToList()
                        });
                    }
                }

                // Get recipients with email
                var recipients = campaign.Recipients
                    .Where(r => !string.IsNullOrEmpty(r.Email))
                    .OrderBy(r => r.Level)
                    .ToList();

                if (!recipients.Any())
                {
                    var allRecipients = campaign.Recipients.ToList();
                    _logger.LogWarning(
                        $"No recipients with email found for campaign {campaign.Id}. " +
                        $"Total recipients: {allRecipients.Count}");
                    
                    return BadRequest(new
                    {
                        error = "Không tìm thấy người nhận có email",
                        message = $"Chiến dịch có {allRecipients.Count} người nhận nhưng không có người nhận nào có địa chỉ email. " +
                                 "Vui lòng thêm địa chỉ email cho người nhận để có thể gửi email.",
                        totalRecipients = allRecipients.Count
                    });
                }

                var successCount = 0;
                var failCount = 0;

                // Send email to each recipient
                foreach (var recipient in recipients)
                {
                    foreach (var template in emailTemplates)
                    {
                        try
                        {
                            // Replace placeholders
                            var content = ReplacePlaceholders(template.Content, recipient, now);
                            var subject = $"MessNote: {campaign.Name}";

                            var success = await _emailService.SendMessageEmailAsync(
                                recipient.Email!,
                                recipient.Name ?? "Bạn",
                                subject,
                                content
                            );

                            // Log to history
                            var history = new History
                            {
                                CampaignId = campaign.Id,
                                RecipientId = recipient.Id,
                                EventType = HistoryEventType.SendMessage,
                                Timestamp = now,
                                Status = success ? HistoryStatus.Success : HistoryStatus.Failed,
                                Details = $"Email sent manually via template {template.Id}. Channel: {template.Channel}"
                            };

                            _context.Histories.Add(history);

                            if (success)
                            {
                                successCount++;
                                _logger.LogInformation(
                                    $"Email sent successfully to {recipient.Email} for campaign {campaign.Id} using template {template.Id}");
                            }
                            else
                            {
                                failCount++;
                                _logger.LogWarning(
                                    $"Failed to send email to {recipient.Email} for campaign {campaign.Id} using template {template.Id}");
                            }
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            _logger.LogError(ex,
                                $"Error sending email to {recipient.Email} for campaign {campaign.Id} using template {template.Id}");

                            var history = new History
                            {
                                CampaignId = campaign.Id,
                                RecipientId = recipient.Id,
                                EventType = HistoryEventType.SendMessage,
                                Timestamp = now,
                                Status = HistoryStatus.Failed,
                                Details = $"Error sending email: {ex.Message}"
                            };
                            _context.Histories.Add(history);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Emails sent",
                    successCount,
                    failCount,
                    totalRecipients = recipients.Count,
                    totalTemplates = emailTemplates.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending emails for schedule {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string ReplacePlaceholders(string content, Recipient recipient, DateTime now)
        {
            var result = content;
            result = result.Replace("{name}", recipient.Name ?? "Bạn");
            result = result.Replace("{time}", now.ToString("dd/MM/yyyy HH:mm"));
            result = result.Replace("{date}", now.ToString("dd/MM/yyyy"));
            result = result.Replace("{email}", recipient.Email ?? "");
            return result;
        }
    }
}

