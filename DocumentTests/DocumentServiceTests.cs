using NUnit.Framework;
using Moq;
using AutoMapper;
using DAL.Persistence;
using SharedResources.DTO;
using SharedResources.Entities;
using RabbitMq.QueueLibrary;
using System;
using System.Collections.Generic;
using BL.Services;
using FileStorageService.Controllers;
using System.Text;

namespace DocumentTests
{
    [TestFixture]
    public class DocumentServiceTests
    {
        private Mock<IDocumentRepo> _mockRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<IQueueProducer> _mockQueueProducer;
        private Mock<IQueueConsumer> _mockQueueConsumer;
        private Mock<IFilesApi> _filesApi;
        private DocumentService _service;

        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IDocumentRepo>();
            _mockMapper = new Mock<IMapper>();
            _mockQueueProducer = new Mock<IQueueProducer>();
            _mockQueueConsumer = new Mock<IQueueConsumer>();
            _filesApi = new Mock<IFilesApi>();
            _service = new DocumentService(_mockRepo.Object, _mockMapper.Object, _mockQueueProducer.Object, _mockQueueConsumer.Object, _filesApi.Object);
        }


        [Test]
        public void GetDocumentById_ShouldReturnNull_WhenDocumentDoesNotExist()
        {
            var documentId = Guid.NewGuid();
            _mockRepo.Setup(r => r.Read(documentId)).Returns((DocumentDAL)null);

            var result = _service.GetDocumentById(documentId);
            Assert.IsNull(result);
        }

        [Test]
        public void DeleteDocument_ShouldReturnTrue_WhenDocumentIsDeleted()
        {
            var documentId = Guid.NewGuid();
            var documentDal = new DocumentDAL { Id = documentId, Title="validPath", Filepath = "validPathh" };

            _mockRepo.Setup(r => r.Read(documentId)).Returns(documentDal);

            var result = _service.DeleteDocument(documentId);

            _mockRepo.Verify(r => r.Delete(documentId), Times.Once);
            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteDocument_ShouldReturnFalse_WhenDocumentDoesNotExist()
        {
            var documentId = Guid.NewGuid();
            _mockRepo.Setup(r => r.Read(documentId)).Returns((DocumentDAL)null);

            var result = _service.DeleteDocument(documentId);

            Assert.IsFalse(result);
        }

        [Test]
        public void SendToQueue_ShouldCallQueueProducerSendMethod()
        {
            var documentId = Guid.NewGuid();
            var filePath = "/path/to/document";

            _service.SendToQueue(filePath, documentId);

            _mockQueueProducer.Verify(q => q.Send(filePath, documentId), Times.Once);
        }

        #region CreateDocument Tests


        [Test]
        public async Task CreateDocument_FilesApiUploadFails_ThrowsException()
        {
            // Arrange
            var documentDto = new DocumentBl
            {
                Id = Guid.NewGuid(),
                Title = "validPath",
                Filepath = "path/to/file.txt",
                // Weitere erforderliche Eigenschaften setzen
            };

            var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("Dummy file content"));
            string contentType = "text/plain";

            // Setup des Mappers
            var documentDal = new DocumentDAL
            {
                Id = documentDto.Id,
                Title = documentDto.Title,
                Filepath = documentDto.Filepath,
                // Weitere erforderliche Eigenschaften setzen
            };
            _mockMapper.Setup(m => m.Map<DocumentDAL>(It.IsAny<DocumentBl>()))
                       .Returns(documentDal);

            // Setup der Dokumenten-Repository
            _mockRepo.Setup(r => r.Create(It.IsAny<DocumentDAL>())).Verifiable();

            // Setup des FilesApi UploadAsync mit Fehler
            _filesApi.Setup(f => f.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ThrowsAsync(new Exception("Upload failed"))
                .Verifiable();

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _service.CreateDocument(documentDto, fileStream, contentType)
            );

            Assert.IsNotNull(ex);
            Assert.AreEqual("Upload failed", ex.Message);

            _filesApi.Verify(f => f.UploadAsync(fileStream, documentDto.Filepath, contentType), Times.Once);
            _mockRepo.Verify(r => r.Create(It.IsAny<DocumentDAL>()), Times.Never);
            _mockQueueProducer.Verify(q => q.Send(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
            _mockQueueConsumer.Verify(qc => qc.StartReceive(), Times.Never);
        }

        #endregion

        #region GetDocuments Tests

        [Test]
        public void GetDocuments_WithExistingDocuments_ReturnsMappedDocuments()
        {
            // Arrange
            var documentsDal = new List<DocumentDAL>
            {
                new DocumentDAL { Id = Guid.NewGuid(), Title="validPath1", Filepath = "path/to/file1.txt" },
                new DocumentDAL { Id = Guid.NewGuid(), Title="validPath2", Filepath = "path/to/file2.txt" }
            };

            var documentsBl = new List<DocumentBl>
            {
                new DocumentBl { Id = documentsDal[0].Id, Title=documentsDal[0].Title ,Filepath = documentsDal[0].Filepath },
                new DocumentBl { Id = documentsDal[1].Id, Title = documentsDal[1].Title, Filepath = documentsDal[1].Filepath }
            };

            _mockRepo.Setup(r => r.Get()).Returns(documentsDal);
            _mockMapper.Setup(m => m.Map<List<DocumentBl>>(documentsDal)).Returns(documentsBl);

            // Act
            var result = _service.GetDocuments();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(documentsBl[0].Id, result[0].Id);
            Assert.AreEqual(documentsBl[1].Filepath, result[1].Filepath);

            _mockRepo.Verify(r => r.Get(), Times.Once);
            _mockMapper.Verify(m => m.Map<List<DocumentBl>>(documentsDal), Times.Once);
        }

        [Test]
        public void GetDocuments_NoDocuments_ReturnsEmptyList()
        {
            // Arrange
            var documentsDal = new List<DocumentDAL>();
            var documentsBl = new List<DocumentBl>();

            _mockRepo.Setup(r => r.Get()).Returns(documentsDal);
            _mockMapper.Setup(m => m.Map<List<DocumentBl>>(documentsDal)).Returns(documentsBl);

            // Act
            var result = _service.GetDocuments();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            _mockRepo.Verify(r => r.Get(), Times.Once);
            _mockMapper.Verify(m => m.Map<List<DocumentBl>>(documentsDal), Times.Once);
        }

        #endregion
    }
}
