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
using ElasticSearch;
using Microsoft.OpenApi.Services;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;
using Microsoft.Extensions.Logging;
using BL.Exceptions;

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
        private Mock<ISearchIndex> _searchIndex;
        private Mock<ILogger<DocumentService>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IDocumentRepo>();
            _mockMapper = new Mock<IMapper>();
            _mockQueueProducer = new Mock<IQueueProducer>();
            _mockQueueConsumer = new Mock<IQueueConsumer>();
            _filesApi = new Mock<IFilesApi>();
            _searchIndex = new Mock<ISearchIndex>();
            _loggerMock = new Mock<ILogger<DocumentService>>();
            _service = new DocumentService(_mockRepo.Object, _mockMapper.Object, _mockQueueProducer.Object, _mockQueueConsumer.Object, _filesApi.Object, _searchIndex.Object, _loggerMock.Object);
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
                .ThrowsAsync(new BusinessLogicException("Upload failed"))
                .Verifiable();

            // Act & Assert
            var ex = Assert.ThrowsAsync<BusinessLogicException>(async () =>
                await _service.CreateDocument(documentDto, fileStream, contentType)
            );

            Assert.IsNotNull(ex);
            Assert.AreEqual($"Unerwarteter Fehler beim Erstellen des Dokuments mit ID: {documentDto.Id}.", ex.Message);

            // Verifikation der erwarteten Aufrufe
            _filesApi.Verify(f => f.UploadAsync(fileStream, documentDto.Id.ToString(), contentType), Times.Once);
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

        #region GetDocumentById Tests

        [Test]
        public void GetDocumentById_ShouldReturnDocument_WhenDocumentExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var documentDal = new DocumentDAL { Id = documentId, Title = "Doc1", Filepath = "path1.pdf" };
            var documentBl = new DocumentBl { Id = documentDal.Id, Title = documentDal.Title, Filepath = documentDal.Filepath };

            _mockRepo.Setup(r => r.Read(documentId)).Returns(documentDal);
            _mockMapper.Setup(m => m.Map<DocumentBl>(documentDal)).Returns(documentBl);

            // Act
            var result = _service.GetDocumentById(documentId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(documentBl.Id, result.Id);
            Assert.AreEqual(documentBl.Title, result.Title);
            Assert.AreEqual(documentBl.Filepath, result.Filepath);

            _mockRepo.Verify(r => r.Read(documentId), Times.Once);
            _mockMapper.Verify(m => m.Map<DocumentBl>(documentDal), Times.Once);
        }

        #endregion

        #region CreateDocument Tests

        [Test]
        public async Task CreateDocument_ValidInput_ReturnsOcrResult()
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

            var documentDal = new DocumentDAL
            {
                Id = documentDto.Id,
                Title = documentDto.Title,
                Filepath = documentDto.Filepath,
                // Weitere erforderliche Eigenschaften setzen
            };

            var ocrResult = "OCR processed text";

            _mockMapper.Setup(m => m.Map<DocumentDAL>(documentDto)).Returns(documentDal);

            _filesApi.Setup(f => f.UploadAsync(fileStream, documentDto.Id.ToString(), contentType))
                     .Returns(Task.CompletedTask)
                     .Verifiable();

            _mockRepo.Setup(r => r.Create(documentDal)).Verifiable();

            _mockQueueProducer.Setup(q => q.Send(documentDto.Filepath, documentDto.Id)).Verifiable();

            _mockQueueConsumer.Setup(qc => qc.StartReceive()).Verifiable();

            // Korrigierte Setup für das OnReceived-Event mit korrektem EventArgs-Typ und notwendigen Parametern
            _mockQueueConsumer.SetupAdd(qc => qc.OnReceived += It.IsAny<EventHandler<RabbitMq.QueueLibrary.QueueReceivedEventArgs>>())
                               .Callback<EventHandler<RabbitMq.QueueLibrary.QueueReceivedEventArgs>>(handler =>
                               {
                                   // Simuliere den Empfang einer Nachricht
                                   var args = new RabbitMq.QueueLibrary.QueueReceivedEventArgs("OCR processed text", documentDto.Id);
                                   handler.Invoke(this, args);
                               });

            // Act
            var result = await _service.CreateDocument(documentDto, fileStream, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ocrResult, result);

            _filesApi.Verify(f => f.UploadAsync(fileStream, documentDto.Id.ToString(), contentType), Times.Once);
            _mockRepo.Verify(r => r.Create(documentDal), Times.Once);
            _mockQueueProducer.Verify(q => q.Send(documentDto.Filepath, documentDto.Id), Times.Once);
            _mockQueueConsumer.Verify(qc => qc.StartReceive(), Times.Once);
        }

        #endregion

        #region SearchDocuments Tests

        [Test]
        public void SearchDocuments_ShouldReturnDocuments_WhenMatchesFound()
        {
            // Arrange
            var query = "test";

            // Angenommen, SearchDocumentAsync gibt eine Liste von DocumentOcr zurück, die eine Id-Eigenschaft hat
            var searchResults = new List<DocumentOcr>
            {
                new DocumentOcr { Id = Guid.NewGuid(), Title="Title", Content="Content" },
                new DocumentOcr { Id = Guid.NewGuid(), Title="Title", Content="Content" }
            };

            _searchIndex.Setup(s => s.SearchDocumentAsync(query))
                        .Returns(searchResults);

            // Setup des Repositories zum Lesen der Dokumente
            var documentDal1 = new DocumentDAL { Id = searchResults[0].Id, Title = "Doc1", Filepath = "path1.pdf" };
            var documentDal2 = new DocumentDAL { Id = searchResults[1].Id, Title = "Doc2", Filepath = "path2.pdf" };
            _mockRepo.Setup(r => r.Read(searchResults[0].Id)).Returns(documentDal1);
            _mockRepo.Setup(r => r.Read(searchResults[1].Id)).Returns(documentDal2);

            var expectedDocuments = new List<DocumentBl>
            {
                new DocumentBl { Id = documentDal1.Id, Title = documentDal1.Title, Filepath = documentDal1.Filepath },
                new DocumentBl { Id = documentDal2.Id, Title = documentDal2.Title, Filepath = documentDal2.Filepath }
            };

            _mockMapper.Setup(m => m.Map<List<DocumentBl>>(It.IsAny<List<DocumentDAL>>()))
                       .Returns(expectedDocuments);

            // Act
            var result = _service.SearchDocuments(query);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(expectedDocuments[0].Id, result[0].Id);
            Assert.AreEqual(expectedDocuments[1].Title, result[1].Title);

            _searchIndex.Verify(s => s.SearchDocumentAsync(query), Times.Once);
            _mockRepo.Verify(r => r.Read(searchResults[0].Id), Times.Once);
            _mockRepo.Verify(r => r.Read(searchResults[1].Id), Times.Once);
            _mockMapper.Verify(m => m.Map<List<DocumentBl>>(It.IsAny<List<DocumentDAL>>()), Times.Once);
        }

        [Test]
        public void SearchDocuments_ShouldReturnEmptyList_WhenNoMatchesFound()
        {
            // Arrange
            var query = "nonexistent";
            var searchResults = new List<DocumentOcr>();

            _searchIndex.Setup(s => s.SearchDocumentAsync(query))
                        .Returns(searchResults);

            _mockMapper.Setup(m => m.Map<List<DocumentBl>>(It.IsAny<List<DocumentDAL>>()))
                       .Returns(new List<DocumentBl>());

            // Act
            var result = _service.SearchDocuments(query);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            _searchIndex.Verify(s => s.SearchDocumentAsync(query), Times.Once);
            _mockRepo.Verify(r => r.Read(It.IsAny<Guid>()), Times.Never);
            _mockMapper.Verify(m => m.Map<List<DocumentBl>>(It.IsAny<List<DocumentDAL>>()), Times.Once);
        }

        #endregion

        #region GetDocumentFile Tests

        [Test]
        public async Task GetDocumentFile_ShouldReturnFileBytes_WhenFileExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var fileBytes = Encoding.UTF8.GetBytes("Dummy file content");

            _filesApi.Setup(f => f.DownloadFromMinioAsync("documents", documentId.ToString()))
                     .ReturnsAsync(fileBytes);

            // Act
            var result = await _service.GetDocumentFile(documentId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(fileBytes, result);

            _filesApi.Verify(f => f.DownloadFromMinioAsync("documents", documentId.ToString()), Times.Once);
        }

        [Test]
        public void GetDocumentFile_ShouldThrowException_WhenDownloadFails()
        {
            // Arrange
            var documentId = Guid.NewGuid();

            _filesApi.Setup(f => f.DownloadFromMinioAsync("documents", documentId.ToString()))
                     .ThrowsAsync(new BusinessLogicException("Download failed"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<BusinessLogicException>(async () =>
                await _service.GetDocumentFile(documentId)
            );

            Assert.IsNotNull(ex);
            Assert.AreEqual($"Fehler beim Herunterladen der Datei für Dokument ID: {documentId}.", ex.Message);

            _filesApi.Verify(f => f.DownloadFromMinioAsync("documents", documentId.ToString()), Times.Once);
        }

        #endregion



    }
}
