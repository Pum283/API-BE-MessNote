using API.Contracts.Campaigns;
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
    public class CampaignsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CampaignsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<CampaignResponse>>> GetAll()
        {
            var campaigns = await _context.Campaigns
                .Select(c => new CampaignResponse
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Name = c.Name,
                    Description = c.Description,
                    Status = (int)c.Status,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(campaigns);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CampaignResponse>> GetById(Guid id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            return Ok(new CampaignResponse
            {
                Id = campaign.Id,
                UserId = campaign.UserId,
                Name = campaign.Name,
                Description = campaign.Description,
                Status = (int)campaign.Status,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<CampaignResponse>> Create([FromBody] CreateCampaignRequest request)
        {
            // Verify user exists
            if (!await _context.Users.AnyAsync(u => u.Id == request.UserId))
                return BadRequest("User not found");

            var campaign = new Campaign
            {
                UserId = request.UserId,
                Name = request.Name,
                Description = request.Description,
                Status = (CampaignStatus)request.Status
            };

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = campaign.Id }, new CampaignResponse
            {
                Id = campaign.Id,
                UserId = campaign.UserId,
                Name = campaign.Name,
                Description = campaign.Description,
                Status = (int)campaign.Status,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CampaignResponse>> Update(Guid id, [FromBody] UpdateCampaignRequest request)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            if (!string.IsNullOrEmpty(request.Name))
                campaign.Name = request.Name;

            if (request.Description != null)
                campaign.Description = request.Description;

            if (request.Status.HasValue)
                campaign.Status = (CampaignStatus)request.Status.Value;

            campaign.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new CampaignResponse
            {
                Id = campaign.Id,
                UserId = campaign.UserId,
                Name = campaign.Name,
                Description = campaign.Description,
                Status = (int)campaign.Status,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

