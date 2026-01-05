using API.Contracts.Recipients;
using API.Responses;
using Domain.Entity;
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
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var query = _context.Recipients.AsQueryable();

            if (campaignId.HasValue)
            {
                // Verify campaign ownership for User/Enterprise
                var campaign = await _context.Campaigns.FindAsync(campaignId.Value);
                if (campaign == null)
                    return NotFound();

                if (currentUserRole != "Admin" && campaign.UserId != currentUserId)
                    return Forbid();

                query = query.Where(r => r.CampaignId == campaignId.Value);
            }
            else
            {
                // If no campaignId, filter by user's campaigns (unless Admin)
                if (currentUserRole != "Admin")
                {
                    var userCampaignIds = await _context.Campaigns
                        .Where(c => c.UserId == currentUserId)
                        .Select(c => c.Id)
                        .ToListAsync();
                    query = query.Where(r => userCampaignIds.Contains(r.CampaignId));
                }
            }

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
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // Verify campaign exists and user has permission
            var campaign = await _context.Campaigns.FindAsync(request.CampaignId);
            if (campaign == null)
                return BadRequest(new { error = "CAMPAIGN_NOT_FOUND", message = "Campaign not found" });

            if (currentUserRole != "Admin" && campaign.UserId != currentUserId)
                return Forbid();

            var recipient = new Recipient
            {
                CampaignId = request.CampaignId,
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                FacebookId = request.FacebookId,
                Level = request.Level // Default level 1 is set in CreateRecipientRequest
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
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var recipient = await _context.Recipients
                .Include(r => r.Campaign)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (recipient == null)
                return NotFound();

            // Verify campaign ownership
            if (currentUserRole != "Admin" && recipient.Campaign.UserId != currentUserId)
                return Forbid();

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
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var recipient = await _context.Recipients
                .Include(r => r.Campaign)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (recipient == null)
                return NotFound();

            // Verify campaign ownership
            if (currentUserRole != "Admin" && recipient.Campaign.UserId != currentUserId)
                return Forbid();

            _context.Recipients.Remove(recipient);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

