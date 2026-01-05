using API.Contracts.MessageTemplates;
using API.Responses;
using Domain.Entity;
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
    public class MessageTemplatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessageTemplatesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<MessageTemplateResponse>>> GetAll([FromQuery] Guid? campaignId = null)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var query = _context.MessageTemplates
                .Include(m => m.Campaign)
                .AsQueryable();

            if (campaignId.HasValue)
            {
                var campaign = await _context.Campaigns.FindAsync(campaignId.Value);
                if (campaign == null)
                    return NotFound();

                if (currentUserRole != "Admin" && campaign.UserId != currentUserId)
                    return Forbid();

                query = query.Where(m => m.CampaignId == campaignId.Value);
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
                    query = query.Where(m => userCampaignIds.Contains(m.CampaignId));
                }
            }

            var templates = await query
                .OrderBy(m => m.SequenceOrder)
                .Select(m => new MessageTemplateResponse
                {
                    Id = m.Id,
                    CampaignId = m.CampaignId,
                    Content = m.Content,
                    Channel = (int)m.Channel,
                    SequenceOrder = m.SequenceOrder,
                    Placeholders = m.Placeholders,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(templates);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageTemplateResponse>> GetById(Guid id)
        {
            var template = await _context.MessageTemplates.FindAsync(id);
            if (template == null)
                return NotFound();

            return Ok(new MessageTemplateResponse
            {
                Id = template.Id,
                CampaignId = template.CampaignId,
                Content = template.Content,
                Channel = (int)template.Channel,
                SequenceOrder = template.SequenceOrder,
                Placeholders = template.Placeholders,
                CreatedAt = template.CreatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<MessageTemplateResponse>> Create([FromBody] CreateMessageTemplateRequest request)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // Verify campaign exists and user has permission
            var campaign = await _context.Campaigns
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == request.CampaignId);
            
            if (campaign == null)
                return BadRequest(new { error = "CAMPAIGN_NOT_FOUND", message = "Campaign not found" });

            if (currentUserRole != "Admin" && campaign.UserId != currentUserId)
                return Forbid();

            // Check template limits for User role
            if (campaign.User.Role == UserRole.User)
            {
                var userTemplatesCount = await _context.MessageTemplates
                    .Include(m => m.Campaign)
                    .CountAsync(m => m.Campaign.UserId == campaign.UserId);

                if (userTemplatesCount >= 10)
                {
                    return BadRequest(new
                    {
                        error = "LIMIT_EXCEEDED",
                        message = "Bạn đã đạt giới hạn 10 template. Vui lòng xóa một template để tạo mới.",
                        details = new { limit = 10, current = userTemplatesCount }
                    });
                }

                // Check sequence level limit (max 2 for User)
                if (request.SequenceOrder > 2)
                {
                    return BadRequest(new
                    {
                        error = "SEQUENCE_LIMIT_EXCEEDED",
                        message = "Gói của bạn chỉ cho phép tối đa 2 tin nhắn trong chuỗi khẩn cấp.",
                        details = new { maxSequence = 2, requested = request.SequenceOrder }
                    });
                }
            }

            var template = new MessageTemplate
            {
                CampaignId = request.CampaignId,
                Content = request.Content,
                Channel = (MessageChannel)request.Channel,
                SequenceOrder = request.SequenceOrder, // Default value 1 is set in CreateMessageTemplateRequest
                Placeholders = request.Placeholders
            };

            _context.MessageTemplates.Add(template);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = template.Id }, new MessageTemplateResponse
            {
                Id = template.Id,
                CampaignId = template.CampaignId,
                Content = template.Content,
                Channel = (int)template.Channel,
                SequenceOrder = template.SequenceOrder,
                Placeholders = template.Placeholders,
                CreatedAt = template.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MessageTemplateResponse>> Update(Guid id, [FromBody] UpdateMessageTemplateRequest request)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var template = await _context.MessageTemplates
                .Include(m => m.Campaign)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (template == null)
                return NotFound();

            // Verify campaign ownership
            if (currentUserRole != "Admin" && template.Campaign.UserId != currentUserId)
                return Forbid();

            // Check sequence level limit for User role
            if (template.Campaign.User.Role == UserRole.User && request.SequenceOrder.HasValue && request.SequenceOrder.Value > 2)
            {
                return BadRequest(new
                {
                    error = "SEQUENCE_LIMIT_EXCEEDED",
                    message = "Gói của bạn chỉ cho phép tối đa 2 tin nhắn trong chuỗi khẩn cấp.",
                    details = new { maxSequence = 2, requested = request.SequenceOrder.Value }
                });
            }

            if (request.Content != null)
                template.Content = request.Content;

            if (request.Channel.HasValue)
                template.Channel = (MessageChannel)request.Channel.Value;

            if (request.SequenceOrder.HasValue)
                template.SequenceOrder = request.SequenceOrder.Value;

            if (request.Placeholders != null)
                template.Placeholders = request.Placeholders;

            await _context.SaveChangesAsync();

            return Ok(new MessageTemplateResponse
            {
                Id = template.Id,
                CampaignId = template.CampaignId,
                Content = template.Content,
                Channel = (int)template.Channel,
                SequenceOrder = template.SequenceOrder,
                Placeholders = template.Placeholders,
                CreatedAt = template.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var template = await _context.MessageTemplates
                .Include(m => m.Campaign)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (template == null)
                return NotFound();

            // Verify campaign ownership
            if (currentUserRole != "Admin" && template.Campaign.UserId != currentUserId)
                return Forbid();

            _context.MessageTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

