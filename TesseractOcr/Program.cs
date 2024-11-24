// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.QueueLibrary;
using TesseractOcr;
using FileStorageService.Controllers;
using Minio;

string basePath = AppDomain.CurrentDomain.BaseDirectory;
string folderPath = Path.Combine(basePath, "./", "appsettings.json");
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile(folderPath, false, true) // add as content / copy-always
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddScoped<OcrOptions>();

var ocrQueueOptions = config.GetSection("QueueOptionsOcr").Get<QueueOptions>();
var resultQueueOptions = config.GetSection("QueueOptionsResult").Get<QueueOptions>();


// Register QueueProducer with OCR options and logging
builder.Services.AddScoped<IQueueProducer>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<QueueProducer>>();
    return new QueueProducer(resultQueueOptions, logger);
});

// Register QueueConsumer with Result options and logging
builder.Services.AddScoped<IQueueConsumer>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<QueueConsumer>>();
    return new QueueConsumer(
        ocrQueueOptions,
        logger
    );
});

builder.Services.AddSingleton<IMinioClient>(_ =>
            new MinioClient()
                .WithEndpoint(config["FileStorage:Endpoint"].Replace("http://", "").Replace("https://", ""))
                .WithCredentials(config["FileStorage:AccessKey"], config["FileStorage:SecretKey"])
                .Build());

builder.Services.AddScoped<IOcrClient, OcrClient>();
builder.Services.AddScoped<IFilesApi, FilesApi>();
builder.Services.AddScoped<IQueueService, QueueService>();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();

    queueService.Start();
}

