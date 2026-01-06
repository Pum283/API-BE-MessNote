using API.Contracts.Campaigns;
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
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Campaign> query = _context.Campaigns;

            // User và Enterprise chỉ thấy campaigns của chính họ, Admin thấy tất cả
            if (currentUserRole != "Admin")
            {
                query = query.Where(c => c.UserId == currentUserId);
            }

            var campaigns = await query
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
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            // User và Enterprise chỉ có thể xem campaigns của chính họ, Admin có thể xem tất cả
            if (currentUserRole != "Admin" && campaign.UserId != currentUserId)
            {
                return Forbid();
            }

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
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // Verify user exists
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return BadRequest(new { error = "USER_NOT_FOUND", message = "Không tìm thấy người dùng" });

            // User và Enterprise chỉ có thể tạo campaigns cho chính họ, Admin có thể tạo cho bất kỳ ai
            if (currentUserRole != "Admin" && request.UserId != currentUserId)
            {
                return Forbid();
            }

            // Check campaign limits for User role
            if (user.Role == UserRole.User)
            {
                var activeCampaignsCount = await _context.Campaigns
                    .CountAsync(c => c.UserId == request.UserId && c.Status == CampaignStatus.Active);

                if (activeCampaignsCount >= 3 && request.Status == (int)CampaignStatus.Active)
                {
                    return BadRequest(new
                    {
                        error = "LIMIT_EXCEEDED",
                        message = "Bạn đã đạt giới hạn 3 chiến dịch đang hoạt động. Vui lòng tạm dừng hoặc xóa một chiến dịch để tạo mới.",
                        details = new { limit = 3, current = activeCampaignsCount }
                    });
                }
            }

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
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            // User và Enterprise chỉ có thể update campaigns của chính họ
            if (currentUserRole != "Admin" && campaign.UserId != currentUserId)
            {
                return Forbid();
            }

            // Get user to check role
            var user = await _context.Users.FindAsync(campaign.UserId);
            if (user == null)
                return BadRequest(new { error = "USER_NOT_FOUND", message = "Không tìm thấy người dùng" });

            // Check campaign limits for User role when setting status to Active
            if (user.Role == UserRole.User && request.Status.HasValue && request.Status.Value == (int)CampaignStatus.Active)
            {
                var activeCampaignsCount = await _context.Campaigns
                    .CountAsync(c => c.UserId == campaign.UserId && c.Status == CampaignStatus.Active && c.Id != id);

                if (activeCampaignsCount >= 3)
                {
                    return BadRequest(new
                    {
                        error = "LIMIT_EXCEEDED",
                        message = "Bạn đã đạt giới hạn 3 chiến dịch đang hoạt động. Vui lòng tạm dừng một chiến dịch khác trước.",
                        details = new { limit = 3, current = activeCampaignsCount }
                    });
                }
            }

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
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            // User và Enterprise chỉ có thể xóa campaigns của chính họ
            if (currentUserRole != "Admin" && campaign.UserId != currentUserId)
            {
                return Forbid();
            }

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Tạm dừng campaign
        /// </summary>
        [HttpPost("{id}/pause")]
        public async Task<ActionResult<CampaignResponse>> Pause(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            if (currentUserRole != "Admin" && campaign.UserId != currentUserId)
            {
                return Forbid();
            }

            campaign.Status = CampaignStatus.Paused;
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

        /// <summary>
        /// Tiếp tục campaign
        /// </summary>
        [HttpPost("{id}/resume")]
        public async Task<ActionResult<CampaignResponse>> Resume(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            if (currentUserRole != "Admin" && campaign.UserId != currentUserId)
            {
                return Forbid();
            }

            // Check campaign limits for User role
            var user = await _context.Users.FindAsync(campaign.UserId);
            if (user != null && user.Role == UserRole.User)
            {
                var activeCampaignsCount = await _context.Campaigns
                    .CountAsync(c => c.UserId == campaign.UserId && c.Status == CampaignStatus.Active && c.Id != id);

                if (activeCampaignsCount >= 3)
                {
                    return BadRequest(new
                    {
                        error = "LIMIT_EXCEEDED",
                        message = "Bạn đã đạt giới hạn 3 chiến dịch đang hoạt động. Vui lòng tạm dừng một chiến dịch khác trước.",
                        details = new { limit = 3, current = activeCampaignsCount }
                    });
                }
            }

            campaign.Status = CampaignStatus.Active;
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
    }
}

