using API.Contracts.Recipients;
using API.Responses;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipientsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecipientsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<RecipientResponse>>> GetAll([FromQuery] Guid? campaignId = null)
        {
            var query = _context.Recipients.AsQueryable();

            if (campaignId.HasValue)
                query = query.Where(r => r.CampaignId == campaignId.Value);

            var recipients = await query
                .Select(r => new RecipientResponse
                {
                    Id = r.Id,
                    CampaignId = r.CampaignId,
                    Name = r.Name,
                    Phone = r.Phone,
                    Email = r.Email,
                    FacebookId = r.FacebookId,
                    Level = r.Level,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(recipients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecipientResponse>> GetById(Guid id)
        {
            var recipient = await _context.Recipients.FindAsync(id);
            if (recipient == null)
                return NotFound();

            return Ok(new RecipientResponse
            {
                Id = recipient.Id,
                CampaignId = recipient.CampaignId,
                Name = recipient.Name,
                Phone = recipient.Phone,
                Email = recipient.Email,
                FacebookId = recipient.FacebookId,
                Level = recipient.Level,
                CreatedAt = recipient.CreatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<RecipientResponse>> Create([FromBody] CreateRecipientRequest request)
        {
            // Verify campaign exists
            if (!await _context.Campaigns.AnyAsync(c => c.Id == request.CampaignId))
                return BadRequest("Campaign not found");

            var recipient = new Recipient
            {
                CampaignId = request.CampaignId,
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                FacebookId = request.FacebookId,
                Level = request.Level
            };

            _context.Recipients.Add(recipient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = recipient.Id }, new RecipientResponse
            {
                Id = recipient.Id,
                CampaignId = recipient.CampaignId,
                Name = recipient.Name,
                Phone = recipient.Phone,
                Email = recipient.Email,
                FacebookId = recipient.FacebookId,
                Level = recipient.Level,
                CreatedAt = recipient.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RecipientResponse>> Update(Guid id, [FromBody] UpdateRecipientRequest request)
        {
            var recipient = await _context.Recipients.FindAsync(id);
            if (recipient == null)
                return NotFound();

            if (request.Name != null)
                recipient.Name = request.Name;

            if (request.Phone != null)
                recipient.Phone = request.Phone;

            if (request.Email != null)
                recipient.Email = request.Email;

            if (request.FacebookId != null)
                recipient.FacebookId = request.FacebookId;

            if (request.Level.HasValue)
                recipient.Level = request.Level.Value;

            await _context.SaveChangesAsync();

            return Ok(new RecipientResponse
            {
                Id = recipient.Id,
                CampaignId = recipient.CampaignId,
                Name = recipient.Name,
                Phone = recipient.Phone,
                Email = recipient.Email,
                FacebookId = recipient.FacebookId,
                Level = recipient.Level,
                CreatedAt = recipient.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var recipient = await _context.Recipients.FindAsync(id);
            if (recipient == null)
                return NotFound();

            _context.Recipients.Remove(recipient);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

