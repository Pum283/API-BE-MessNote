using Application.Common.Interfaces;
using Application.Interfaces;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("SourceCodeApiBaseDb"));
            }

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Common Services
            services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();

            return services;
        }
    }
}

