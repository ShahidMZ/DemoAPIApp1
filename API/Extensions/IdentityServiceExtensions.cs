using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        // Create and add an authentication service that authenticates the JWT tokens based on the options specified.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => 
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,    // Validate the signing key stored in the config file.
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                ValidateIssuer = false,             // No way to validate API server, which is the issuer, presently.
                ValidateAudience = false
            };
        });

        return services;
    }
}
