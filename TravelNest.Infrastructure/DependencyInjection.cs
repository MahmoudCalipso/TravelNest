
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelNest.Application.Interfaces;
using TravelNest.Application.Validators.Auth;
using TravelNest.Domain.Interfaces;
using TravelNest.Infrastructure.Data;
using TravelNest.Infrastructure.Repositories;
using TravelNest.Infrastructure.Services;

namespace TravelNest.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // AutoMapper
        services.AddAutoMapper(cfg => 
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

        return services;
    }
}