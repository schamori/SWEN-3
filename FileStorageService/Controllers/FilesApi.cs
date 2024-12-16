using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using FileStorageService;


namespace FileStorageService.Controllers
{

    public class FilesApi : IFilesApi
    {
        private readonly IMinioClient _minioClient;
        private readonly FileStorageServiceOptions _options;
        private readonly ILogger<FilesApi> _logger;

        public FilesApi(IMinioClient minioClient, IOptions<FileStorageServiceOptions> options, ILogger<FilesApi> logger)
        {
            _minioClient = minioClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Prüfen, ob der Bucket existiert
                var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_options.BucketName));

            if (!bucketExists)
            {
                // Bucket erstellen, falls er nicht existiert
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_options.BucketName));
                    _logger.LogInformation($"Bucket {_options.BucketName} erstellt.");
                }

            // Datei hochladen
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType));
                _logger.LogInformation($"Datei {fileName} erfolgreich in Bucket {_options.BucketName} hochgeladen.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Hochladen der Datei {fileName} in den Bucket {_options.BucketName}.");
                throw;
            }
        }

        public async Task<byte[]> DownloadFromMinioAsync(string bucketName, string fileName)
        {
            try
            {
                var downloadStream = new MemoryStream();

                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithCallbackStream(x =>
                    {
                        x.CopyTo(downloadStream);
                    }));

                return downloadStream.ToArray(); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Herunterladen der Datei {fileName} aus dem Bucket {bucketName}.");
                throw;
            }
        }

    }

}
