using API.Responses;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Authenticated")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tổng quan hiệu quả theo campaign cho dashboard
        /// </summary>
        [HttpGet("campaigns")]
        public async Task<ActionResult<List<CampaignAnalyticsResponse>>> GetCampaignAnalytics()
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // Lấy danh sách campaign theo quyền
            var campaignsQuery = _context.Campaigns.AsQueryable();

            if (currentUserRole != "Admin")
            {
                campaignsQuery = campaignsQuery.Where(c => c.UserId == currentUserId);
            }

            var campaigns = await campaignsQuery.ToListAsync();
            var campaignIds = campaigns.Select(c => c.Id).ToList();

            // Lấy histories & checkins tương ứng
            var histories = await _context.Histories
                .Where(h => campaignIds.Contains(h.CampaignId))
                .ToListAsync();

            var checkins = await _context.Checkins
                .Where(c => campaignIds.Contains(c.CampaignId))
                .ToListAsync();

            var result = new List<CampaignAnalyticsResponse>();

            foreach (var campaign in campaigns)
            {
                var campaignHistories = histories.Where(h => h.CampaignId == campaign.Id).ToList();
                var campaignCheckins = checkins.Where(c => c.CampaignId == campaign.Id).ToList();

                var messagesSent = campaignHistories.Count(h => h.EventType == HistoryEventType.SendMessage && h.Status == HistoryStatus.Success);
                var messagesFailed = campaignHistories.Count(h => h.EventType == HistoryEventType.SendMessage && h.Status == HistoryStatus.Failed);
                var messagesPending = campaignHistories.Count(h => h.EventType == HistoryEventType.SendMessage && h.Status == HistoryStatus.Pending);

                var overdueEvents = campaignHistories.Count(h => h.EventType == HistoryEventType.Overdue);

                var totalCheckins = campaignCheckins.Count;
                var checkinsSuccess = campaignCheckins.Count(c => c.Success);
                var checkinsFailed = campaignCheckins.Count(c => !c.Success);

                var lastEventAt = campaignHistories.OrderByDescending(h => h.Timestamp).FirstOrDefault()?.Timestamp;
                var lastCheckinAt = campaignCheckins.OrderByDescending(c => c.CheckinTime).FirstOrDefault()?.CheckinTime;

                result.Add(new CampaignAnalyticsResponse
                {
                    CampaignId = campaign.Id,
                    CampaignName = campaign.Name,
                    Status = campaign.Status,
                    TotalEvents = campaignHistories.Count,
                    MessagesSent = messagesSent,
                    MessagesFailed = messagesFailed,
                    MessagesPending = messagesPending,
                    CheckinsTotal = totalCheckins,
                    CheckinsSuccess = checkinsSuccess,
                    CheckinsFailed = checkinsFailed,
                    OverdueEvents = overdueEvents,
                    LastEventAt = lastEventAt,
                    LastCheckinAt = lastCheckinAt
                });
            }

            return Ok(result
                .OrderByDescending(r => r.LastEventAt ?? r.LastCheckinAt ?? DateTime.MinValue)
                .ToList());
        }
    }
}


