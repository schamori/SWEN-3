using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DAL.Persistence;
using FluentValidation.AspNetCore;
using SharedResources.Validators;
using SharedResources.Mappers;
using RabbitMq.QueueLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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


            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole(); 
                logging.AddDebug();   
            });
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddScoped<IQueueProducer, QueueProducer>();

            builder.Services.AddScoped<IQueueProducer, QueueProducer>();
            builder.Services.AddScoped<IDocumentRepo, DocumentRepo>();


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
                options.UseNpgsql("Host=paperless-postgres;Port=5432;Database=documentsearch;Username=mamo;Password=T1P3m!hvQ9")
                );

            builder.Services.Configure<QueueOptions>(config.GetSection("QueueOptions"));


            builder.Services.AddAutoMapper(typeof(DocumentProfile));


            builder.Services.AddScoped<DocumentRepo>();
            // Add services to the container.
            builder.Services.AddControllers();


            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            // Enable CORS
            app.UseCors();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate(); 
            }



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
