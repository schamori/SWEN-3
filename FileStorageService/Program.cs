using FileStorageService;
using Minio;

var builder = WebApplication.CreateBuilder(args);

// Optionen für FileStorage konfigurieren
var fileStorageOptions = new FileStorageServiceOptions();
builder.Configuration.Bind("FileStorage", fileStorageOptions);

// MinIO-Client registrieren
builder.Services.AddSingleton<IMinioClient>(_ =>
    new MinioClient()
        .WithEndpoint(fileStorageOptions.Endpoint.Replace("http://", "").Replace("https://", ""))
        .WithCredentials(fileStorageOptions.AccessKey, fileStorageOptions.SecretKey)
        .Build());

// Controller und API konfigurieren
builder.Services.AddControllers();

// Swagger (optional, für API-Dokumentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware einrichten
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run("http://0.0.0.0:8088");
