using API.Contracts.MessageTemplates;
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
            var query = _context.MessageTemplates.AsQueryable();

            if (campaignId.HasValue)
                query = query.Where(m => m.CampaignId == campaignId.Value);

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
            // Verify campaign exists
            if (!await _context.Campaigns.AnyAsync(c => c.Id == request.CampaignId))
                return BadRequest("Campaign not found");

            var template = new MessageTemplate
            {
                CampaignId = request.CampaignId,
                Content = request.Content,
                Channel = (MessageChannel)request.Channel,
                SequenceOrder = request.SequenceOrder,
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
            var template = await _context.MessageTemplates.FindAsync(id);
            if (template == null)
                return NotFound();

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
            var template = await _context.MessageTemplates.FindAsync(id);
            if (template == null)
                return NotFound();

            _context.MessageTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

