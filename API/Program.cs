using Infrastructure.Persistence;
using API.Middlewares;
using API.Services;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "MessNote API - Hệ thống gửi tin nhắn tự động", 
        Version = "v1",
        Description = "API quản lý chiến dịch gửi tin nhắn tự động đa kênh (SMS, Email, Facebook Messenger) với chế độ điểm danh và lịch trình",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "MessNote Team"
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Application Layer
builder.Services.AddApplication();

// Add Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var issuer = jwtSettings["Issuer"] ?? "MessNoteAPI";
var audience = jwtSettings["Audience"] ?? "MessNoteClient";

// Google OAuth Settings (only ClientId needed for token validation)
var googleAuthSettings = builder.Configuration.GetSection("GoogleAuthSettings");
var googleClientId = googleAuthSettings["ClientId"] ?? throw new InvalidOperationException("Google ClientId is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        RoleClaimType = ClaimTypes.Role
    };
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EnterpriseOrAdmin", policy => policy.RequireRole("Enterprise", "Admin"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

// Add JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS must be before UseAuthorization
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Exception handling middleware
app.UseExceptionMiddleware();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
    }
}

app.Run();
