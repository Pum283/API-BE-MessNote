using API.Contracts.Checkins;
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
    public class CheckinsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CheckinsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<CheckinResponse>>> GetAll([FromQuery] Guid? campaignId = null)
        {
            var query = _context.Checkins.AsQueryable();

            if (campaignId.HasValue)
                query = query.Where(c => c.CampaignId == campaignId.Value);

            var checkins = await query
                .OrderByDescending(c => c.CheckinTime)
                .Select(c => new CheckinResponse
                {
                    Id = c.Id,
                    CampaignId = c.CampaignId,
                    CheckinTime = c.CheckinTime,
                    Method = (int)c.Method,
                    Location = c.Location,
                    Success = c.Success,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(checkins);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CheckinResponse>> GetById(Guid id)
        {
            var checkin = await _context.Checkins.FindAsync(id);
            if (checkin == null)
                return NotFound();

            return Ok(new CheckinResponse
            {
                Id = checkin.Id,
                CampaignId = checkin.CampaignId,
                CheckinTime = checkin.CheckinTime,
                Method = (int)checkin.Method,
                Location = checkin.Location,
                Success = checkin.Success,
                Notes = checkin.Notes,
                CreatedAt = checkin.CreatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<CheckinResponse>> Create([FromBody] CreateCheckinRequest request)
        {
            // Verify campaign exists
            if (!await _context.Campaigns.AnyAsync(c => c.Id == request.CampaignId))
                return BadRequest("Campaign not found");

            var checkin = new Checkin
            {
                CampaignId = request.CampaignId,
                CheckinTime = DateTime.UtcNow,
                Method = (CheckinMethod)request.Method,
                Location = request.Location,
                Success = request.Success,
                Notes = request.Notes
            };

            _context.Checkins.Add(checkin);

            // Update schedule's LastCheckinTime if exists
            var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.CampaignId == request.CampaignId);
            if (schedule != null && request.Success)
            {
                schedule.LastCheckinTime = checkin.CheckinTime;
                schedule.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = checkin.Id }, new CheckinResponse
            {
                Id = checkin.Id,
                CampaignId = checkin.CampaignId,
                CheckinTime = checkin.CheckinTime,
                Method = (int)checkin.Method,
                Location = checkin.Location,
                Success = checkin.Success,
                Notes = checkin.Notes,
                CreatedAt = checkin.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CheckinResponse>> Update(Guid id, [FromBody] UpdateCheckinRequest request)
        {
            var checkin = await _context.Checkins.FindAsync(id);
            if (checkin == null)
                return NotFound();

            if (request.Method.HasValue)
                checkin.Method = (CheckinMethod)request.Method.Value;

            if (request.Location != null)
                checkin.Location = request.Location;

            if (request.Success.HasValue)
                checkin.Success = request.Success.Value;

            if (request.Notes != null)
                checkin.Notes = request.Notes;

            await _context.SaveChangesAsync();

            return Ok(new CheckinResponse
            {
                Id = checkin.Id,
                CampaignId = checkin.CampaignId,
                CheckinTime = checkin.CheckinTime,
                Method = (int)checkin.Method,
                Location = checkin.Location,
                Success = checkin.Success,
                Notes = checkin.Notes,
                CreatedAt = checkin.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var checkin = await _context.Checkins.FindAsync(id);
            if (checkin == null)
                return NotFound();

            _context.Checkins.Remove(checkin);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

