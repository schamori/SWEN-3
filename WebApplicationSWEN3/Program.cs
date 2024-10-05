using Microsoft.EntityFrameworkCore;
using sws.Models;

namespace WebApplicationSWEN3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddControllers();

            // Configure CORS to allow requests from all origins
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Configure in-memory database
            builder.Services.AddDbContext<DocumentContext>(options =>
                options.UseInMemoryDatabase("MyInMemoryDatabase"));

            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            // Enable CORS
            app.UseCors();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // Development-specific settings
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Map only API controllers, no static files served by this server
            app.MapControllers();

            app.Run();
        }
    }
}
