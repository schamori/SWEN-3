using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileStorageService;
using FileStorageService.Controllers;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel; // Für ObjectStat
using Moq;
using NUnit.Framework;
using Minio.DataModel.Response;
using System.Net;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using BL.Services;

namespace DocumentTests
{
    [TestFixture]
    public class FilesApiTests
    {
        private Mock<IMinioClient> _minioClientMock;
        private IOptions<FileStorageServiceOptions> _options;
        private FilesApi _filesApi;
        private Mock<ILogger<FilesApi>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _minioClientMock = new Mock<IMinioClient>();
            _options = Options.Create(new FileStorageServiceOptions
            {
                BucketName = "test-bucket"
            });
            _loggerMock = new Mock<ILogger<FilesApi>>();

            _filesApi = new FilesApi(_minioClientMock.Object, _options, _loggerMock.Object);
        }

        #region UploadAsync Tests
        
        [Test]
        public async Task UploadAsync_BucketExists_ShouldPutObject()
        {
            // Arrange
            var fileName = "test-file.txt";
            var contentType = "text/plain";
            var fileContent = "Hello world";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            _minioClientMock
                .Setup(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var putObjectResponse = new PutObjectResponse(
                HttpStatusCode.OK,               // statusCode
                "test-etag",                     // etag
                new Dictionary<string, string>(),// metadata
                memoryStream.Length,             // size
                "test-version-id"                // versionId
            );

            _minioClientMock
                .Setup(m => m.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(putObjectResponse);

            // Act
            await _filesApi.UploadAsync(memoryStream, fileName, contentType);

            // Assert
            _minioClientMock.Verify(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()), Times.Once);
            _minioClientMock.Verify(m => m.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()), Times.Once);
            _minioClientMock.Verify(m => m.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task UploadAsync_BucketNotExists_ShouldCreateBucketAndPutObject()
        {
            // Arrange
            var fileName = "test-file.txt";
            var contentType = "text/plain";
            var fileContent = "Hello world";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            _minioClientMock
                .Setup(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _minioClientMock
                .Setup(m => m.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var putObjectResponse = new PutObjectResponse(
                HttpStatusCode.OK,               // statusCode
                "test-etag",                     // etag
                new Dictionary<string, string>(),// metadata
                memoryStream.Length,             // size
                "test-version-id"                // versionId
            );

            _minioClientMock
                .Setup(m => m.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(putObjectResponse);

            // Act
            await _filesApi.UploadAsync(memoryStream, fileName, contentType);

            // Assert
            _minioClientMock.Verify(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()), Times.Once);
            _minioClientMock.Verify(m => m.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()), Times.Once);
            _minioClientMock.Verify(m => m.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        [Test]
        public void DownloadFromMinioAsync_FileDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var bucketName = "test-bucket";
            var fileName = "non-existent-file.txt";

            _minioClientMock
                .Setup(m => m.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("File not found"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _filesApi.DownloadFromMinioAsync(bucketName, fileName));

            Assert.AreEqual("File not found", ex.Message);
            _minioClientMock.Verify(m => m.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void DownloadFromMinioAsync_ErrorDuringDownload_ShouldThrowException()
        {
            // Arrange
            var bucketName = "test-bucket";
            var fileName = "test-file.txt";

            _minioClientMock
                .Setup(m => m.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _filesApi.DownloadFromMinioAsync(bucketName, fileName));

            Assert.AreEqual("Network error", ex.Message);
            _minioClientMock.Verify(m => m.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
