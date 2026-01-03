using API.Contracts.Schedules;
using API.Responses;
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

        public SchedulesController(AppDbContext context)
        {
            _context = context;
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
                Recurring = (RecurringType)request.Recurring
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
                schedule.FixedTime = request.FixedTime;

            if (request.CheckinInterval != null)
                schedule.CheckinInterval = request.CheckinInterval;

            if (request.OverdueThreshold != null)
                schedule.OverdueThreshold = request.OverdueThreshold;

            if (request.Recurring.HasValue)
                schedule.Recurring = (RecurringType)request.Recurring.Value;

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
    }
}

