
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using RabbitMq.QueueLibrary;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string basePath = AppDomain.CurrentDomain.BaseDirectory;
string folderPath = Path.Combine(basePath, "./", "appsettings.json");
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile(folderPath, false, true) // add as content / copy-always
    .Build();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddScoped<IQueueProducer, QueueProducer>();
builder.Services.Configure<QueueOptions>(config.GetSection("QueueOptions"));



app.Run();


