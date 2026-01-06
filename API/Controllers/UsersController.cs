using API.Contracts.Users;
using API.Responses;
using Domain.Entity;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Authenticated")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng (chỉ Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            var users = await _context.Users
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Lấy thông tin người dùng theo ID (Admin có thể xem tất cả, User chỉ xem được chính mình)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetById(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // User chỉ có thể xem thông tin của chính mình, Admin có thể xem tất cả
            if (currentUserRole != "Admin" && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }

        /// <summary>
        /// Tạo người dùng mới (chỉ Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
        {
            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Tên người dùng đã tồn tại");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email đã tồn tại");

            // Chỉ Admin mới có thể tạo User với role khác User
            var role = request.Role ?? UserRole.User;

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = role,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }

        /// <summary>
        /// Cập nhật thông tin người dùng (Admin có thể cập nhật tất cả, User chỉ cập nhật được chính mình nhưng không đổi role)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // User chỉ có thể cập nhật thông tin của chính mình, Admin có thể cập nhật tất cả
            if (currentUserRole != "Admin" && currentUserId != id)
            {
                return Forbid();
            }

            // Chỉ Admin mới có thể thay đổi Role
            if (request.Role.HasValue && currentUserRole != "Admin")
            {
                return Forbid("Chỉ Admin mới có thể thay đổi vai trò người dùng");
            }

            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                    return BadRequest("Tên người dùng đã tồn tại");
                user.Username = request.Username;
            }

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                    return BadRequest("Email đã tồn tại");
                user.Email = request.Email;
            }

            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            if (request.Role.HasValue)
                user.Role = request.Role.Value;

            if (request.IsActive.HasValue)
            {
                // Chỉ Admin mới có thể thay đổi trạng thái IsActive
                if (currentUserRole != "Admin")
                {
                    return Forbid("Chỉ Admin mới có thể thay đổi trạng thái tài khoản");
                }
                user.IsActive = request.IsActive.Value;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }

        /// <summary>
        /// Xóa người dùng (chỉ Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}

