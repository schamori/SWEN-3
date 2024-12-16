using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DAL.Persistence;
using FluentValidation.AspNetCore;
using BL.Validators;
using SharedResources.Mappers;
using RabbitMq.QueueLibrary;
using BL.Services;
using FileStorageService.Controllers;
using Minio;
using ElasticSearch;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;
using WebApplicationSWEN3;


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

            builder.Services.AddControllers()
                .AddFluentValidation(fv =>
                fv.RegisterValidatorsFromAssemblyContaining<DocumentValidator>());


            builder.Services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole(); 
                logging.AddDebug();   
            });
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();


            var ocrQueueOptions = config.GetSection("QueueOptionsOcr").Get<QueueOptions>();
            var resultQueueOptions = config.GetSection("QueueOptionsResult").Get<QueueOptions>();

            builder.Services.AddSingleton<IMinioClient>(_ =>
            new MinioClient()
                .WithEndpoint(config["FileStorage:Endpoint"].Replace("http://", "").Replace("https://", ""))
                .WithCredentials(config["FileStorage:AccessKey"], config["FileStorage:SecretKey"])
                .Build());

            // Register QueueProducer with OCR options and logging
            builder.Services.AddScoped<IQueueProducer>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<QueueProducer>>();
                return new QueueProducer(ocrQueueOptions, logger);
            });

            builder.Services.AddScoped<ISearchIndex, ElasticSearchIndex>();

            // Register QueueConsumer with Result options and logging
            builder.Services.AddScoped<IQueueConsumer>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<QueueConsumer>>();
                return new QueueConsumer(resultQueueOptions, logger);
            });

            builder.Services.AddScoped<IDocumentServices, DocumentService>();

            builder.Services.AddScoped<IDocumentRepo, DocumentRepo>();

            builder.Services.AddScoped<IFilesApi, FilesApi>();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WebApplicationSWEN3 API",
                    Version = "v1",
                    Description = "API-Dokumentation für WebApplicationSWEN3"
                });
                // Fügen Sie den OperationFilter hinzu
                c.OperationFilter<FileUploadOperationFilter>();

            });


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



            var tmp = config.GetSection("QueueOptions");


            builder.Services.AddAutoMapper(typeof(DocumentProfile));


            builder.Services.AddScoped<DocumentRepo>();

            var app = builder.Build();

            // Enable CORS
            app.UseCors();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate(); 
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplicationSWEN3 API v1");
                c.RoutePrefix = "swagger"; // Swagger UI unter /swagger verfügbar
            });


            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Map only API controllers, no static files served by this server
            app.MapControllers();

            app.Run();
        }
    }
}
