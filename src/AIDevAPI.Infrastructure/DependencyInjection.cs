using AIDevAPI.Application.Interfaces.Repositories;
using AIDevAPI.Application.Interfaces.Services;
using AIDevAPI.Domain.Entities;
using AIDevAPI.Infrastructure.Persistence;
using AIDevAPI.Infrastructure.Persistence.Repositories;
using AIDevAPI.Infrastructure.Services;
using AIDevAPI.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIDevAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Şifrə siyasəti
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            // Lockout (bloklama) siyasəti
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // İstifadəçi siyasəti
            options.User.RequireUniqueEmail = true;

            // Email təsdiqi giriş üçün məcburi deyil (istəyə görə true edilə bilər)
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

        services.AddHttpContextAccessor();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
