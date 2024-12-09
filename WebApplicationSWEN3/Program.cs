using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DAL.Persistence;
using FluentValidation.AspNetCore;
using BL.Validators;
using SharedResources.Mappers;
using RabbitMq.QueueLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BL.Services;
using FileStorageService.Controllers;
using Minio;
using ElasticSearch;
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


            builder.Services.AddSwaggerGen();
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



            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Map only API controllers, no static files served by this server
            app.MapControllers();

            app.Run();
        }
    }
}
