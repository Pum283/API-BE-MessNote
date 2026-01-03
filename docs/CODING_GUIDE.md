# Hướng Dẫn Code - Coding Guide

Tài liệu này mô tả chi tiết các nguyên tắc, quy tắc đặt tên, vị trí file và best practices khi phát triển dự án **SourceCodeApiBase**.

## 📋 Mục Lục

1. [Nguyên Tắc Clean Architecture](#nguyên-tắc-clean-architecture)
2. [Quy Tắc Đặt Tên](#quy-tắc-đặt-tên)
3. [Vị Trí và Cấu Trúc File](#vị-trí-và-cấu-trúc-file)
4. [Dependency Rules](#dependency-rules)
5. [Code Style Guidelines](#code-style-guidelines)
6. [Best Practices](#best-practices)
7. [Examples](#examples)

---

## 🏗️ Nguyên Tắc Clean Architecture

### Kiến Trúc Layers

```
API Layer (Presentation)
    ↓
Infrastructure Layer (Implementation)
    ↓
Application Layer (Business Logic)
    ↓
Domain Layer (Core - No Dependencies)
```

### Quy Tắc Dependency

1. **Domain Layer**: Không được phụ thuộc vào bất kỳ layer nào
   - ❌ Không import từ Application, Infrastructure, API
   - ✅ Chỉ chứa domain logic thuần túy

2. **Application Layer**: Chỉ phụ thuộc Domain
   - ❌ Không import từ Infrastructure, API
   - ✅ Import từ Domain

3. **Infrastructure Layer**: Phụ thuộc Application và Domain
   - ❌ Không import từ API
   - ✅ Import từ Application và Domain

4. **API Layer**: Phụ thuộc tất cả layers
   - ✅ Có thể import từ Application, Infrastructure, Domain

---

## 📝 Quy Tắc Đặt Tên

### 1. Classes và Interfaces

#### **PascalCase** - Luôn viết hoa chữ cái đầu mỗi từ

**Classes:**
```csharp
// ✅ Đúng
public class UserService { }
public class ProductRepository { }
public class OrderController { }
public class CreateUserDto { }

// ❌ Sai
public class userService { }
public class product_repository { }
public class OrderControllerBase { }  // Tránh Base trong tên class cụ thể
```

**Interfaces:**
```csharp
// ✅ Đúng - Bắt đầu bằng chữ I
public interface IUserService { }
public interface IRepository<T> { }
public interface IUnitOfWork { }

// ❌ Sai
public interface UserService { }  // Thiếu I
public interface iUserService { }  // I phải viết hoa
```

### 2. Methods và Properties

#### **PascalCase** cho public members

```csharp
// ✅ Đúng
public string GetUserName() { }
public int UserId { get; set; }
public async Task<List<User>> GetAllUsersAsync() { }

// ❌ Sai
public string get_user_name() { }
public int userId { get; set; }  // Private field thì dùng camelCase
```

### 3. Parameters và Local Variables

#### **camelCase** - Chữ cái đầu viết thường

```csharp
// ✅ Đúng
public void CreateUser(string userName, int userId)
{
    var user = new User();
    string fullName = userName + " " + lastName;
}

// ❌ Sai
public void CreateUser(string UserName, int UserId) { }
```

### 4. Private Fields

#### **camelCase** với prefix `_` (underscore)

```csharp
// ✅ Đúng
private readonly IUserRepository _userRepository;
private readonly ILogger<UserService> _logger;

// ❌ Sai
private readonly IUserRepository userRepository;
private readonly IUserRepository _UserRepository;
```

### 5. Constants và Static Fields

#### **PascalCase** hoặc **UPPER_CASE**

```csharp
// ✅ Đúng
public const int MaxRetryCount = 3;
public const string DEFAULT_CONNECTION = "DefaultConnection";
public static readonly string ApiVersion = "v1";

// ❌ Sai
public const int maxRetryCount = 3;
public const string default_connection = "DefaultConnection";
```

### 6. Enums

#### **PascalCase** cho enum name và values

```csharp
// ✅ Đúng
public enum UserRole
{
    Admin,
    User,
    Guest
}

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

// ❌ Sai
public enum user_role
{
    admin,
    USER,
    guest
}
```

### 7. Namespaces

#### **PascalCase** - Theo cấu trúc folder

```csharp
// ✅ Đúng
namespace Domain.Entity
namespace Application.Interfaces.Repositories
namespace Infrastructure.Persistence
namespace API.Controllers

// ❌ Sai
namespace domain.entity
namespace Application.Interfaces.repositories
```

---

## 📁 Vị Trí và Cấu Trúc File

### Domain Layer

#### **Domain/Entity/**
Nơi đặt tất cả domain entities.

```
Domain/Entity/
├── User.cs                    # User entity
├── Product.cs                 # Product entity
└── Order.cs                   # Order entity
```

**Quy tắc:**
- Mỗi file = 1 class
- File name = Class name
- Kế thừa từ `BaseEntity` hoặc `BaseAuditableEntity`

**Example:**
```csharp
// Domain/Entity/User.cs
namespace Domain.Entity
{
    public class User : BaseAuditableEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
```

#### **Domain/Enum/**
Nơi đặt tất cả enums.

```
Domain/Enum/
├── UserRole.cs                # UserRole enum
├── OrderStatus.cs             # OrderStatus enum
└── PaymentMethod.cs           # PaymentMethod enum
```

**Quy tắc:**
- Mỗi file = 1 enum
- File name = Enum name

#### **Domain/Common/**
Các base classes và interfaces chung.

```
Domain/Common/
├── BaseEntity.cs              # Base entity với Id, CreatedDate, UpdatedDate
├── BaseAuditableEntity.cs     # Base entity với audit fields
├── IAuditableEntity.cs        # Interface cho audit
└── ISoftDelete.cs             # Interface cho soft delete
```

### Application Layer

#### **Application/DTOs/**
Data Transfer Objects - Request/Response models.

```
Application/DTOs/
├── User/
│   ├── CreateUserDto.cs       # DTO cho create user
│   ├── UpdateUserDto.cs       # DTO cho update user
│   ├── UserDto.cs             # DTO cho response
│   └── UserListDto.cs         # DTO cho list response
├── Product/
│   ├── CreateProductDto.cs
│   └── ProductDto.cs
```

**Quy tắc đặt tên DTO:**
- `Create{Entity}Dto` - Cho create request
- `Update{Entity}Dto` - Cho update request
- `{Entity}Dto` - Cho response (single)
- `{Entity}ListDto` - Cho list response (nếu cần)
- `{Action}{Entity}Dto` - Cho các action khác (ví dụ: `DeleteUserDto`)

**Example:**
```csharp
// Application/DTOs/User/CreateUserDto.cs
namespace Application.DTOs.User
{
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
```

#### **Application/Interfaces/Repositories/**
Repository interfaces.

```
Application/Interfaces/Repositories/
├── IRepository.cs             # Generic repository interface
├── IUserRepository.cs         # User repository interface
└── IProductRepository.cs      # Product repository interface
```

**Quy tắc:**
- Bắt đầu bằng `I`
- Kết thúc bằng `Repository`
- Kế thừa từ `IRepository<T>` nếu cần generic methods

**Example:**
```csharp
// Application/Interfaces/Repositories/IUserRepository.cs
namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
```

#### **Application/Interfaces/Services/**
Service interfaces.

```
Application/Interfaces/Services/
├── IUserService.cs            # User service interface
├── IProductService.cs         # Product service interface
└── IEmailService.cs           # Email service interface
```

**Quy tắc:**
- Bắt đầu bằng `I`
- Kết thúc bằng `Service`
- Chứa business logic contracts

**Example:**
```csharp
// Application/Interfaces/Services/IUserService.cs
namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<OperationResult<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<OperationResult<List<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
```

#### **Application/Service/**
Service implementations (nếu không dùng MediatR).

```
Application/Service/
├── UserService.cs             # User service implementation
└── ProductService.cs          # Product service implementation
```

**Quy tắc:**
- File name = Class name (không có I)
- Implement interface tương ứng

**Example:**
```csharp
// Application/Service/UserService.cs
namespace Application.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Implementation...
    }
}
```

**Lưu ý:** Nếu dùng MediatR, không cần folder này, thay vào đó dùng `Application/Features/`.

#### **Application/Common/**
Shared utilities và helpers.

```
Application/Common/
├── OperationResult.cs         # Generic result wrapper
├── PagedQuery.cs              # Base class cho paged queries
├── PagedResponse.cs           # Paged response model
└── Interfaces/
    ├── ICurrentUserService.cs
    └── IDateTimeProvider.cs
```

### Infrastructure Layer

#### **Infrastructure/Repositories/**
Repository implementations.

```
Infrastructure/Repositories/
├── Repository.cs              # Generic repository
├── UserRepository.cs          # User repository implementation
└── ProductRepository.cs       # Product repository implementation
```

**Quy tắc:**
- File name = Class name (không có I)
- Kế thừa từ `Repository<T>` nếu cần generic methods
- Implement interface từ Application layer

**Example:**
```csharp
// Infrastructure/Repositories/UserRepository.cs
namespace Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
    }
}
```

#### **Infrastructure/Services/**
Service implementations (external services).

```
Infrastructure/Services/
├── EmailService.cs            # Email service implementation
├── FileService.cs             # File service implementation
└── PaymentService.cs          # Payment service implementation
```

#### **Infrastructure/Persistence/**
Database related files.

```
Infrastructure/Persistence/
├── AppDbContext.cs            # EF Core DbContext
├── AppDbContextFactory.cs     # Factory cho migrations
├── UnitOfWork.cs              # Unit of Work implementation
├── Configurations/
│   ├── UserConfiguration.cs   # EF Core configuration cho User
│   └── ProductConfiguration.cs
├── Migrations/
│   └── (EF Core migrations - tự động generate)
└── Interceptors/
    └── (EF Core interceptors nếu cần)
```

**Quy tắc Configurations:**
- File name = `{Entity}Configuration.cs`
- Implement `IEntityTypeConfiguration<T>`

**Example:**
```csharp
// Infrastructure/Persistence/Configurations/UserConfiguration.cs
namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}
```

### API Layer

#### **API/Controllers/**
API Controllers.

```
API/Controllers/
├── ApiController.cs           # Base controller
├── UserController.cs          # User endpoints
└── ProductController.cs       # Product endpoints
```

**Quy tắc:**
- File name = `{Entity}Controller.cs`
- Kế thừa từ `ApiController`
- Route = `api/{controller}` (bỏ Controller)

**Example:**
```csharp
// API/Controllers/UserController.cs
namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var result = await _userService.CreateAsync(dto);
            return HandleResult(result);
        }
    }
}
```

#### **API/Contracts/**
API request/response models (nếu khác với DTOs).

```
API/Contracts/
├── User/
│   ├── CreateUserRequest.cs
│   └── UserResponse.cs
```

**Lưu ý:** Thông thường dùng DTOs từ Application layer, chỉ tạo Contracts nếu cần format khác cho API.

#### **API/Middlewares/**
Custom middlewares.

```
API/Middlewares/
└── ExceptionMiddleware.cs     # Global exception handling
```

#### **API/Attributes/**
Custom attributes.

```
API/Attributes/
└── CustomAuthorizeAttribute.cs
```

---

## 🔗 Dependency Rules

### Quy Tắc Import và Reference

#### ❌ KHÔNG ĐƯỢC:

1. **Domain layer import từ layer khác:**
```csharp
// ❌ SAI - Domain/Entity/User.cs
using Application.DTOs;  // SAI!
using Infrastructure.Persistence;  // SAI!
```

2. **Application layer import từ Infrastructure hoặc API:**
```csharp
// ❌ SAI - Application/Service/UserService.cs
using Infrastructure.Repositories;  // SAI! Phải dùng interface
using API.Controllers;  // SAI!
```

3. **Infrastructure layer import từ API:**
```csharp
// ❌ SAI - Infrastructure/Repositories/UserRepository.cs
using API.Controllers;  // SAI!
```

#### ✅ ĐƯỢC PHÉP:

1. **Application layer chỉ import từ Domain:**
```csharp
// ✅ ĐÚNG - Application/Service/UserService.cs
using Domain.Entity;  // OK
using Application.Interfaces.Repositories;  // OK (cùng layer)
```

2. **Infrastructure layer import từ Application và Domain:**
```csharp
// ✅ ĐÚNG - Infrastructure/Repositories/UserRepository.cs
using Domain.Entity;  // OK
using Application.Interfaces.Repositories;  // OK
using Infrastructure.Persistence;  // OK (cùng layer)
```

3. **API layer có thể import từ tất cả:**
```csharp
// ✅ ĐÚNG - API/Controllers/UserController.cs
using Domain.Entity;  // OK
using Application.DTOs;  // OK
using Application.Interfaces.Services;  // OK
```

---

## 💻 Code Style Guidelines

### 1. File Organization

#### Thứ tự trong file:

```csharp
// 1. Using statements (nhóm theo namespace)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Domain.Entity;
using Application.Interfaces.Repositories;

// 2. Namespace
namespace Infrastructure.Repositories
{
    // 3. Class declaration
    public class UserRepository : Repository<User>, IUserRepository
    {
        // 4. Private fields
        private readonly AppDbContext _context;

        // 5. Constructor
        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // 6. Public methods
        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // Implementation
        }

        // 7. Private methods
        private void ValidateUser(User user)
        {
            // Implementation
        }
    }
}
```

### 2. Async/Await

#### Luôn dùng async/await cho I/O operations:

```csharp
// ✅ Đúng
public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
    return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
}

// ❌ Sai - Blocking call
public User? GetById(int id)
{
    return _dbSet.Find(id);  // Blocking!
}
```

#### Async method naming:
- Luôn kết thúc bằng `Async`
- Return type: `Task` hoặc `Task<T>`

```csharp
// ✅ Đúng
public async Task<User> CreateAsync(User user)
public async Task<bool> DeleteAsync(int id)
public async Task<List<User>> GetAllAsync()

// ❌ Sai
public async Task<User> Create(User user)  // Thiếu Async
public async void Delete(int id)  // void với async - chỉ dùng cho event handlers
```

### 3. Nullable Reference Types

#### Luôn xử lý null an toàn:

```csharp
// ✅ Đúng
public string Username { get; set; } = string.Empty;  // Non-nullable
public string? Description { get; set; }  // Nullable

// Với return types
public async Task<User?> GetByIdAsync(int id)  // Có thể null
{
    return await _dbSet.FindAsync(id);
}

// Null checking
if (user == null)
{
    return NotFound();
}
```

### 4. LINQ và Database Queries

#### Ưu tiên dùng LINQ methods:

```csharp
// ✅ Đúng - Asynchronous LINQ
var users = await _dbSet
    .Where(u => u.IsActive)
    .OrderBy(u => u.CreatedDate)
    .ToListAsync(cancellationToken);

// ❌ Sai - Synchronous LINQ với database
var users = _dbSet
    .Where(u => u.IsActive)
    .OrderBy(u => u.CreatedDate)
    .ToList();  // Blocking!
```

#### Include related entities:

```csharp
// ✅ Đúng
var user = await _dbSet
    .Include(u => u.Orders)
    .ThenInclude(o => o.Items)
    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
```

### 5. Error Handling

#### Sử dụng OperationResult:

```csharp
// ✅ Đúng
public async Task<OperationResult<UserDto>> CreateAsync(CreateUserDto dto)
{
    if (await _userRepository.ExistsByEmailAsync(dto.Email))
    {
        return OperationResult<UserDto>.Failure("Email already exists");
    }

    var user = new User
    {
        Email = dto.Email,
        Username = dto.Username
    };

    await _userRepository.AddAsync(user);
    await _unitOfWork.SaveChangesAsync();

    var userDto = new UserDto
    {
        Id = user.Id,
        Email = user.Email
    };

    return OperationResult<UserDto>.Success(userDto);
}

// ❌ Sai - Throw exception trực tiếp
public async Task<UserDto> CreateAsync(CreateUserDto dto)
{
    if (await _userRepository.ExistsByEmailAsync(dto.Email))
    {
        throw new Exception("Email exists");  // Không control được error response
    }
    // ...
}
```

#### Domain Exceptions:

```csharp
// ✅ Đúng - Dùng domain exceptions
if (user == null)
{
    throw new NotFoundException(nameof(User), id);
}

if (user.IsDeleted)
{
    throw new BusinessException("User has been deleted");
}
```

### 6. Dependency Injection

#### Constructor Injection:

```csharp
// ✅ Đúng
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}

// ❌ Sai - Property injection hoặc service locator
public class UserService
{
    public IUserRepository UserRepository { get; set; }  // SAI!
}
```

---

## ✅ Best Practices

### 1. Entity Design

```csharp
// ✅ Đúng
public class User : BaseAuditableEntity, ISoftDelete
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDate { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

// ❌ Sai - Không có base class, không có default values
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }  // Có thể null
    public DateTime CreatedDate { get; set; }  // Nên dùng base class
}
```

### 2. Repository Pattern

```csharp
// ✅ Đúng - Generic repository + specific methods
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
}

// ❌ Sai - Tất cả methods trong interface riêng
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
    // ... Quá nhiều methods generic
}
```

### 3. Service Layer

```csharp
// ✅ Đúng - Business logic trong service
public async Task<OperationResult<UserDto>> CreateAsync(CreateUserDto dto)
{
    // Validation
    if (string.IsNullOrWhiteSpace(dto.Email))
    {
        return OperationResult<UserDto>.Failure("Email is required");
    }

    // Business rule
    if (await _userRepository.ExistsByEmailAsync(dto.Email))
    {
        return OperationResult<UserDto>.Failure("Email already exists");
    }

    // Create entity
    var user = new User
    {
        Email = dto.Email,
        Username = dto.Username
    };

    // Save
    await _userRepository.AddAsync(user);
    await _unitOfWork.SaveChangesAsync();

    // Return DTO
    return OperationResult<UserDto>.Success(MapToDto(user));
}
```

### 4. Controller Design

```csharp
// ✅ Đúng - Thin controllers
[ApiController]
[Route("api/[controller]")]
public class UserController : ApiController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return HandleResult(result);
    }
}

// ❌ Sai - Fat controllers với business logic
[ApiController]
public class UserController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        // Business logic trong controller - SAI!
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest("Email exists");
        }
        // ...
    }
}
```

### 5. Configuration

```csharp
// ✅ Đúng - Separate configuration files
// Infrastructure/Persistence/Configurations/UserConfiguration.cs
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();
    }
}

// ❌ Sai - Configuration trong DbContext OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(u => u.Id);
        // ... Quá nhiều config ở đây
    });
}
```

---

## 📚 Examples

### Complete Example: User Entity với CRUD

#### 1. Domain Entity

```csharp
// Domain/Entity/User.cs
using Domain.Common;

namespace Domain.Entity
{
    public class User : BaseAuditableEntity, ISoftDelete
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
```

#### 2. DTOs

```csharp
// Application/DTOs/User/CreateUserDto.cs
namespace Application.DTOs.User
{
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}

// Application/DTOs/User/UserDto.cs
namespace Application.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
```

#### 3. Repository Interface

```csharp
// Application/Interfaces/Repositories/IUserRepository.cs
using Domain.Entity;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
```

#### 4. Repository Implementation

```csharp
// Infrastructure/Repositories/UserRepository.cs
using Domain.Entity;
using Application.Interfaces.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(u => u.Username == username, cancellationToken);
        }
    }
}
```

#### 5. Service Interface

```csharp
// Application/Interfaces/Services/IUserService.cs
using Application.Common;
using Application.DTOs.User;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<OperationResult<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
        Task<OperationResult<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<OperationResult<List<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
```

#### 6. Service Implementation

```csharp
// Application/Service/UserService.cs
using Application.Common;
using Application.DTOs.User;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entity;
using Domain.Exceptions;

namespace Application.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email, cancellationToken))
            {
                return OperationResult<UserDto>.Failure("Email already exists");
            }

            if (await _userRepository.ExistsByUsernameAsync(dto.Username, cancellationToken))
            {
                return OperationResult<UserDto>.Failure("Username already exists");
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                CreatedDate = user.CreatedDate
            };

            return OperationResult<UserDto>.Success(userDto);
        }

        public async Task<OperationResult<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);

            if (user == null)
            {
                return OperationResult<UserDto>.Failure("User not found");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                CreatedDate = user.CreatedDate
            };

            return OperationResult<UserDto>.Success(userDto);
        }

        public async Task<OperationResult<List<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                CreatedDate = u.CreatedDate
            }).ToList();

            return OperationResult<List<UserDto>>.Success(userDtos);
        }
    }
}
```

#### 7. Controller

```csharp
// API/Controllers/UserController.cs
using Application.Common;
using Application.DTOs.User;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var result = await _userService.CreateAsync(dto);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return HandleResult(result);
        }
    }
}
```

#### 8. Configuration

```csharp
// Infrastructure/Persistence/Configurations/UserConfiguration.cs
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.FullName)
                .HasMaxLength(200);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
```

#### 9. Dependency Injection

```csharp
// Infrastructure/DependencyInjection.cs
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Service;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Services
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
```

---

## 📌 Checklist Khi Code Mới Feature

- [ ] Tạo Entity trong `Domain/Entity/` (kế thừa BaseEntity/BaseAuditableEntity)
- [ ] Tạo Configuration trong `Infrastructure/Persistence/Configurations/` (nếu cần)
- [ ] Tạo DTOs trong `Application/DTOs/{Entity}/`
- [ ] Tạo Repository Interface trong `Application/Interfaces/Repositories/`
- [ ] Implement Repository trong `Infrastructure/Repositories/`
- [ ] Tạo Service Interface trong `Application/Interfaces/Services/`
- [ ] Implement Service trong `Application/Service/` (hoặc Features nếu dùng MediatR)
- [ ] Tạo Controller trong `API/Controllers/`
- [ ] Register dependencies trong `Infrastructure/DependencyInjection.cs`
- [ ] Tạo migration: `dotnet ef migrations add {MigrationName} --project Infrastructure`
- [ ] Test API endpoints qua Swagger

---

## 🔍 Code Review Checklist

- [ ] Tuân thủ naming conventions
- [ ] File đúng vị trí theo layer
- [ ] Không vi phạm dependency rules
- [ ] Dùng async/await cho I/O operations
- [ ] Xử lý null an toàn
- [ ] Dùng OperationResult cho error handling
- [ ] Controller thin (không có business logic)
- [ ] Service có validation và business logic
- [ ] Repository chỉ có data access logic
- [ ] Có CancellationToken cho async methods
- [ ] Code có comments cho logic phức tạp
- [ ] Follow SOLID principles

---

## 📖 Tài Liệu Tham Khảo

- [README.md](../README.md) - Tổng quan về project structure
- [README.md](../README.md) - Tổng quan về project structure

---

**Lưu ý:** Tài liệu này sẽ được cập nhật thường xuyên. Nếu có thắc mắc hoặc đề xuất cải thiện, vui lòng liên hệ team lead hoặc tạo issue.

