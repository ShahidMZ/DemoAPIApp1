using API.Extensions;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ***** Add services to the container *****

        builder.Services.AddControllers();

        // Call extension methods from API.Extensions
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddIdentityServices(builder.Configuration);        

        var app = builder.Build();

        // ***** Configure the HTTP request pipeline *****

        // Define the CORS policy builder
        app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

        // Authentication middleware to be added after UserCors and before MapControllers
        app.UseAuthentication();    // Checks if a token is present.
        app.UseAuthorization();     // Authorizes the user if the token is valid.

        app.MapControllers();

        app.Run();
    }
}
