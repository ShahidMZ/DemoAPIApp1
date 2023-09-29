using API.Data;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ***** Add services to the container *****

        builder.Services.AddControllers();
        builder.Services.AddDbContext<DataContext>(opt => 
        {
            opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        builder.Services.AddCors();

        var app = builder.Build();

        // ***** Configure the HTTP request pipeline *****

        // Define the CORS policy builder
        app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

        app.MapControllers();

        app.Run();
    }
}
