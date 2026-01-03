using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Đăng ký các services từ Assembly
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            
            // Có thể thêm MediatR ở đây nếu cần
            // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
            return services;
        }
    }
}

