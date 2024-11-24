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
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType));
        }
    }

}
