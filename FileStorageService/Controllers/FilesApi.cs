using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using FileStorageService;


namespace FileStorageService.Controllers
{
    [ApiController]
    [Route("test/files")]
    public class FilesApi : Controller
    {
        private readonly IMinioClient _minioClient;
        private readonly FileStorageServiceOptions _options;

        public FilesApi(IOptions<FileStorageServiceOptions> options)
        {
            _options = options.Value;
            _minioClient = new MinioClient()
                .WithEndpoint(_options.Endpoint)
                .WithCredentials(_options.AccessKey, _options.SecretKey)
                .Build();

            // Erstelle den Bucket, falls er nicht existiert
            _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_options.BucketName)).Wait();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                var objectName = file.FileName;

                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType));

                return Ok($"File {objectName} uploaded to MinIO.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                var stream = new MemoryStream();
                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(fileName)
                    .WithCallbackStream(s => s.CopyTo(stream)));

                stream.Position = 0; // Stream zurücksetzen
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return NotFound($"File not found: {fileName}. Error: {ex.Message}");
            }
        }


        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> Delete(string fileName)
        {
            try
            {
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(fileName));

                return Ok($"File {fileName} deleted from MinIO.");
            }
            catch (Exception ex)
            {
                return NotFound($"File not found: {fileName}. Error: {ex.Message}");
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        [HttpGet]
        public IActionResult PingU()
        {
            return Ok("FileStorageService is okay.");
        }



    }
}
