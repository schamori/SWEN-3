// See https://aka.ms/new-console-template for more information
using RabbitMq.QueueLibrary;
using TesseractOcr;


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

builder.Services.AddScoped<OcrOptions>();

builder.Services.AddScoped<IQueueProducer, QueueProducer>();
builder.Services.AddScoped<IOcrClient, OcrClient>();
builder.Services.AddScoped<IQueueConsumer, QueueConsumer>();
builder.Services.AddScoped<IQueueService, QueueService>();

builder.Services.Configure<QueueOptions>(config.GetSection("QueueOptions"));



builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var queueService = scope.ServiceProvider.GetRequiredService<IQueueProducer>();

// queueService.Start(); 
}

