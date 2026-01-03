using API.Contracts.Histories;
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
    public class HistoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<HistoryResponse>>> GetAll([FromQuery] Guid? campaignId = null, [FromQuery] Guid? recipientId = null)
        {
            var query = _context.Histories.AsQueryable();

            if (campaignId.HasValue)
                query = query.Where(h => h.CampaignId == campaignId.Value);

            if (recipientId.HasValue)
                query = query.Where(h => h.RecipientId == recipientId.Value);

            var histories = await query
                .OrderByDescending(h => h.Timestamp)
                .Select(h => new HistoryResponse
                {
                    Id = h.Id,
                    CampaignId = h.CampaignId,
                    EventType = (int)h.EventType,
                    Timestamp = h.Timestamp,
                    RecipientId = h.RecipientId,
                    Status = (int)h.Status,
                    Details = h.Details,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            return Ok(histories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HistoryResponse>> GetById(Guid id)
        {
            var history = await _context.Histories.FindAsync(id);
            if (history == null)
                return NotFound();

            return Ok(new HistoryResponse
            {
                Id = history.Id,
                CampaignId = history.CampaignId,
                EventType = (int)history.EventType,
                Timestamp = history.Timestamp,
                RecipientId = history.RecipientId,
                Status = (int)history.Status,
                Details = history.Details,
                CreatedAt = history.CreatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<HistoryResponse>> Create([FromBody] CreateHistoryRequest request)
        {
            // Verify campaign exists
            if (!await _context.Campaigns.AnyAsync(c => c.Id == request.CampaignId))
                return BadRequest("Campaign not found");

            // Verify recipient exists if provided
            if (request.RecipientId.HasValue && !await _context.Recipients.AnyAsync(r => r.Id == request.RecipientId.Value))
                return BadRequest("Recipient not found");

            var history = new History
            {
                CampaignId = request.CampaignId,
                EventType = (HistoryEventType)request.EventType,
                Timestamp = DateTime.UtcNow,
                RecipientId = request.RecipientId,
                Status = (HistoryStatus)request.Status,
                Details = request.Details
            };

            _context.Histories.Add(history);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = history.Id }, new HistoryResponse
            {
                Id = history.Id,
                CampaignId = history.CampaignId,
                EventType = (int)history.EventType,
                Timestamp = history.Timestamp,
                RecipientId = history.RecipientId,
                Status = (int)history.Status,
                Details = history.Details,
                CreatedAt = history.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var history = await _context.Histories.FindAsync(id);
            if (history == null)
                return NotFound();

            _context.Histories.Remove(history);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

