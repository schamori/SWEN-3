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

        public FilesApi(IMinioClient minioClient, IOptions<FileStorageServiceOptions> options)
        {
            _minioClient = minioClient;
            _options = options.Value;
        }

        public async Task UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            // Prüfen, ob der Bucket existiert
            var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_options.BucketName));

            if (!bucketExists)
            {
                // Bucket erstellen, falls er nicht existiert
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_options.BucketName));
            }

            // Datei hochladen
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType));
        }

        public async Task<Stream> DownloadFromMinioAsync(string bucketName, string fileName)
        {
            var stream = new MemoryStream();

            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithCallbackStream(stream.CopyTo));

            stream.Position = 0; // Zurücksetzen des Streams
            return stream;
        }

    }

}
