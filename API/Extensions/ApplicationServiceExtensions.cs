using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DataContext>(opt => 
        {
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });

        // Add CORS Policy
        services.AddCors();

        // Add services using AddScoped, which will make sure the service is running till the HTTP request is being processed.
        // Other options are AddTransient, which will dispose the service as soon as it is used, and AddSingleton, which will run the service till the end of the session.
        // AddSingleton is useful when services are cached. Any new request can first check the cache if the required service is already running instead of repeating the process.
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Add the AutoMapper
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // Add the Cloudinary settings and photo service.
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
        services.AddScoped<IPhotoService, PhotoService>();

        return services;
    }
}
