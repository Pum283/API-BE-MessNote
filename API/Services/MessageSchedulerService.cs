using Domain.Entity;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Services
{
    public class MessageSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageSchedulerService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

        public MessageSchedulerService(
            IServiceProvider serviceProvider,
            ILogger<MessageSchedulerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Message Scheduler Service started at {Time} UTC", DateTime.UtcNow);

            // Wait a bit before first check to let the app fully start
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessScheduledMessagesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing scheduled messages");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Message Scheduler Service stopped");
        }

        private async Task ProcessScheduledMessagesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now = DateTime.UtcNow;
            _logger.LogDebug($"Checking scheduled messages at {now:yyyy-MM-dd HH:mm:ss} UTC");

            // Get all active campaigns with schedules that need to send messages
            // Check if NextSendTime has arrived (within 10 minutes tolerance to catch missed ones)
            var allSchedules = await context.Schedules
                .Include(s => s.Campaign)
                    .ThenInclude(c => c.MessageTemplates)
                .Include(s => s.Campaign)
                    .ThenInclude(c => c.Recipients)
                .Where(s => s.Campaign.Status == CampaignStatus.Active)
                .ToListAsync();

            _logger.LogDebug($"Total active schedules: {allSchedules.Count}");
            foreach (var s in allSchedules)
            {
                var shouldProcess = s.NextSendTime.HasValue && 
                    s.NextSendTime.Value <= now && 
                    s.NextSendTime.Value > now.AddMinutes(-10);
                
                _logger.LogDebug(
                    $"Schedule {s.Id} (Campaign: {s.CampaignId}): " +
                    $"NextSendTime={s.NextSendTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "NULL"} UTC, " +
                    $"FixedTime={s.FixedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "NULL"} UTC, " +
                    $"Now={now:yyyy-MM-dd HH:mm:ss} UTC, " +
                    $"ShouldProcess={shouldProcess}, " +
                    $"Templates={s.Campaign.MessageTemplates.Count}, " +
                    $"Recipients={s.Campaign.Recipients.Count(r => !string.IsNullOrEmpty(r.Email))}");
            }

            var schedulesToProcess = allSchedules
                .Where(s => 
                    s.NextSendTime.HasValue &&
                    s.NextSendTime.Value <= now &&
                    s.NextSendTime.Value > now.AddMinutes(-10) // Process if within last 10 minutes to catch missed ones
                )
                .ToList();
            
            if (schedulesToProcess.Count > 0)
            {
                _logger.LogInformation($"Found {schedulesToProcess.Count} schedules ready to send");
            }

            _logger.LogInformation($"Found {schedulesToProcess.Count} schedules to process");

            foreach (var schedule in schedulesToProcess)
            {
                try
                {
                    _logger.LogInformation(
                        $"Processing schedule {schedule.Id} for campaign {schedule.CampaignId}. " +
                        $"NextSendTime: {schedule.NextSendTime:yyyy-MM-dd HH:mm:ss} UTC, " +
                        $"CurrentTime: {now:yyyy-MM-dd HH:mm:ss} UTC");
                    
                    await ProcessScheduleAsync(context, emailService, schedule, now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing schedule {schedule.Id} for campaign {schedule.CampaignId}");
                }
            }
        }

        private async Task ProcessScheduleAsync(
            AppDbContext context,
            IEmailService emailService,
            Schedule schedule,
            DateTime now)
        {
            var campaign = schedule.Campaign;
            
            _logger.LogInformation(
                $"Processing campaign {campaign.Id} ({campaign.Name}). " +
                $"Templates: {campaign.MessageTemplates.Count}, Recipients: {campaign.Recipients.Count}");
            
            // Get message templates for this campaign (filter by Email or All channel)
            var emailTemplates = campaign.MessageTemplates
                .Where(t => t.Channel == MessageChannel.Email || t.Channel == MessageChannel.All)
                .OrderBy(t => t.SequenceOrder)
                .ToList();

            if (!emailTemplates.Any())
            {
                _logger.LogWarning($"No email templates found for campaign {campaign.Id}. Available templates: {string.Join(", ", campaign.MessageTemplates.Select(t => $"ID:{t.Id}, Channel:{t.Channel}"))}");
                return;
            }

            _logger.LogInformation($"Found {emailTemplates.Count} email templates for campaign {campaign.Id}");

            // Get recipients with email addresses
            var recipients = campaign.Recipients
                .Where(r => !string.IsNullOrEmpty(r.Email))
                .OrderBy(r => r.Level)
                .ToList();

            if (!recipients.Any())
            {
                _logger.LogWarning($"No recipients with email found for campaign {campaign.Id}. Total recipients: {campaign.Recipients.Count}");
                return;
            }

            _logger.LogInformation($"Found {recipients.Count} recipients with email for campaign {campaign.Id}");

            // Send email to each recipient
            foreach (var recipient in recipients)
            {
                foreach (var template in emailTemplates)
                {
                    try
                    {
                        // Replace placeholders in content
                        var content = ReplacePlaceholders(template.Content, recipient, now);
                        var subject = $"MessNote: {campaign.Name}";

                        var success = await emailService.SendMessageEmailAsync(
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
                            Details = $"Email sent via template {template.Id}. Channel: {template.Channel}"
                        };

                        context.Histories.Add(history);

                        if (success)
                        {
                            _logger.LogInformation(
                                $"Email sent successfully to {recipient.Email} for campaign {campaign.Id} using template {template.Id}");
                        }
                        else
                        {
                            _logger.LogWarning(
                                $"Failed to send email to {recipient.Email} for campaign {campaign.Id} using template {template.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, 
                            $"Error sending email to {recipient.Email} for campaign {campaign.Id} using template {template.Id}");

                        // Log failure to history
                        var history = new History
                        {
                            CampaignId = campaign.Id,
                            RecipientId = recipient.Id,
                            EventType = HistoryEventType.SendMessage,
                            Timestamp = now,
                            Status = HistoryStatus.Failed,
                            Details = $"Error sending email: {ex.Message}"
                        };
                        context.Histories.Add(history);
                    }
                }
            }

            // Update schedule based on type
            if (schedule.ScheduleType == ScheduleType.Fixed)
            {
                // For fixed schedules, check if recurring
                if (schedule.Recurring == RecurringType.None)
                {
                    // One-time: mark as completed by setting NextSendTime far in future
                    schedule.NextSendTime = DateTime.UtcNow.AddYears(100);
                }
                else
                {
                    // Recurring: calculate next send time based on FixedTime
                    schedule.NextSendTime = CalculateNextSendTime(schedule, now);
                }
            }
            else if (schedule.ScheduleType == ScheduleType.Checkin)
            {
                // For checkin schedules, NextSendTime is updated when checkin happens
                // Here we just mark that we've processed it by setting it far in future
                // It will be recalculated when next checkin happens
                schedule.NextSendTime = DateTime.UtcNow.AddYears(100);
            }

            schedule.UpdatedAt = now;
            await context.SaveChangesAsync();
        }

        private string ReplacePlaceholders(string content, Recipient recipient, DateTime now)
        {
            var result = content;

            // Replace common placeholders
            result = result.Replace("{name}", recipient.Name ?? "Bạn");
            result = result.Replace("{time}", now.ToString("dd/MM/yyyy HH:mm"));
            result = result.Replace("{date}", now.ToString("dd/MM/yyyy"));
            result = result.Replace("{email}", recipient.Email ?? "");

            return result;
        }

        private DateTime CalculateNextSendTime(Schedule schedule, DateTime currentTime)
        {
            if (!schedule.FixedTime.HasValue)
                return currentTime.AddDays(1);

            var fixedTime = schedule.FixedTime.Value;
            var nextTime = fixedTime;

            // Find the next occurrence after current time
            while (nextTime <= currentTime)
            {
                switch (schedule.Recurring)
                {
                    case RecurringType.Daily:
                        nextTime = nextTime.AddDays(1);
                        break;
                    case RecurringType.Weekly:
                        nextTime = nextTime.AddDays(7);
                        break;
                    case RecurringType.Monthly:
                        nextTime = nextTime.AddMonths(1);
                        break;
                    default:
                        return DateTime.UtcNow.AddYears(100); // Never send again
                }
            }

            return nextTime;
        }
    }
}

