using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DAL.Persistence;
using FluentValidation.AspNetCore;
using DAL.Validators;

namespace WebApplicationSWEN3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(basePath, "./", "appsettings.json");
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(folderPath, false, true) // add as content / copy-always
                .Build();
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddFluentValidation(fv =>
                fv.RegisterValidatorsFromAssemblyContaining<DocumentValidator>());

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
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql("Host=localhost;Port=5432;Database=documentsearch;Username=mamo;Password=T1P3m!hvQ9;")
                );
            // Configure in-memory database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
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
