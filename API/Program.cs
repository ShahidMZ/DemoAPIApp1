using API.Data;
using API.Extensions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ***** Add services to the container *****

        builder.Services.AddControllers();

        // Call extension methods from API.Extensions
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddIdentityServices(builder.Configuration);        

        var app = builder.Build();

        // ***** Configure the HTTP request pipeline *****

        // Exception Handling
        app.UseMiddleware<ExceptionMiddleware>();

        // Exception Handling for .NET 5 and below
        // if (builder.Environment.IsDevelopment())
        // {
        //     app.UseDeveloperExceptionPage();
        // }

        // Define the CORS policy builder
        app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

        // Authentication middleware to be added after UserCors and before MapControllers
        app.UseAuthentication();    // Checks if a token is present.
        app.UseAuthorization();     // Authorizes the user if the token is valid.

        app.MapControllers();

        // Seed Database.
        await SeedDatabase(app);

        app.Run();
    }

    public static async Task SeedDatabase(WebApplication app)
    {
        // Gives access to all the services present in the Program class.
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<DataContext>();
            await context.Database.MigrateAsync();
            await Seed.SeedUsers(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred during migration.");
        }
    }
}
