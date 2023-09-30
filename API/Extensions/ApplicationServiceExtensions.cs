using API.Data;
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
        services.AddCors();

        // Add TokenService using AddScoped, which will make sure the service is running till the HTTP request is being processed.
        // Other options are AddTransient, which will dispose the service as soon as it is used, and AddSingleton, which will run the service even after the request is processed.
        // AddSingleton is useful when services are cached. Any new request can first check the cache if the required service is already running instead of repeating the process.
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
