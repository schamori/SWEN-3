using NUnit.Framework;
using Moq;
using DAL.Persistence;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplicationSWEN3.Controllers;
using SharedResources.Entities;
using SharedResources.DTO;
using RabbitMq.QueueLibrary;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using BL.Services;
using Microsoft.AspNetCore.Http;
using System.Text;
using BL.Validators;

namespace DocumentTests
{
    [TestFixture]
    public class DocumentControllerTests : IDisposable
    {
        private Mock<IDocumentServices> _mockService;  // Changed to IDocumentService
        private Mock<IMapper> _mockMapper;
        private Mock<IQueueProducer> _mockQueueProducer;
        private Mock<ILogger<DocumentController>> _loggerMock;

        private DocumentController _controller;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IDocumentServices>();
            _mockMapper = new Mock<IMapper>();
            _mockQueueProducer = new Mock<IQueueProducer>();
            _loggerMock = new Mock<ILogger<DocumentController>>();
            _controller = new DocumentController(_mockMapper.Object, _mockQueueProducer.Object, _loggerMock.Object, _mockService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller != null)
            {
                (_controller as IDisposable)?.Dispose();
            }
        }

        public void Dispose()
        {
            TearDown();
        }

        #region Existing Tests

        [Test]
        public void GetDocument_ShouldReturnDocument_WhenDocumentExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new DocumentBl { Id = documentId, Title = "Doc1", Filepath = "/path1" };
            _mockService.Setup(service => service.GetDocumentById(documentId)).Returns(document);

            // Act
            var result = _controller.GetDocument(documentId) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(document, result.Value);
        }

        #endregion

        #region Get() Tests

        [Test]
        public void Get_ShouldReturnOk_WithListOfDocuments()
        {
            // Arrange
            var documents = new List<DocumentBl>
            {
                new DocumentBl { Id = Guid.NewGuid(), Title = "Doc1", Filepath = "/path1" },
                new DocumentBl { Id = Guid.NewGuid(), Title = "Doc2", Filepath = "/path2" }
            };
            _mockService.Setup(service => service.GetDocuments()).Returns(documents);

            // Act
            var result = _controller.Get() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(documents, result.Value);
            _mockService.Verify(service => service.GetDocuments(), Times.Once);
        }

        [Test]
        public void Get_ShouldReturnStatusCode500_WhenExceptionOccurs()
        {
            // Arrange
            _mockService.Setup(service => service.GetDocuments()).Throws(new Exception("Database failure"));

            // Act
            var result = _controller.Get() as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(500, result.StatusCode);
            Assert.AreEqual("Internal server error.", result.Value);
            _mockService.Verify(service => service.GetDocuments(), Times.Once);
        }

        #endregion

        #region PostDocument() Tests

        [Test]
        public async Task PostDocument_ValidFile_ReturnsCreatedAtAction()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Dummy file content";
            var fileName = "validFile.txt";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(_ => _.Length).Returns(memoryStream.Length);
            fileMock.Setup(_ => _.ContentType).Returns("text/plain");

            var uploadDto = new DocumentUploadDto
            {
                File = fileMock.Object
            };

            var documentDto = new DocumentDTO
            {
                Id = Guid.NewGuid(),
                Title = fileName,
                Filepath = fileName
            };

            var documentBl = new DocumentBl
            {
                Id = documentDto.Id,
                Title = documentDto.Title,
                Filepath = documentDto.Filepath
            };

            // Setup IMapper to map DocumentDTO to DocumentBl correctly
            _mockMapper.Setup(m => m.Map<DocumentBl>(It.IsAny<DocumentDTO>()))
                       .Returns<DocumentDTO>(dto => new DocumentBl
                       {
                           Id = dto.Id,
                           Title = dto.Title,
                           Filepath = dto.Filepath
                       });

            // Variable to capture the DocumentBl passed to CreateDocument
            DocumentBl capturedDocumentBl = null;

            // Setup the service to capture the DocumentBl and return an OCR result
            _mockService.Setup(s => s.CreateDocument(It.IsAny<DocumentBl>(), memoryStream, "text/plain"))
                        .Callback<DocumentBl, Stream, string>((docBl, stream, type) =>
                        {
                            capturedDocumentBl = docBl;
                        })
                        .ReturnsAsync("OCR processed text");

            // Act
            var actionResult = await _controller.PostDocument(uploadDto);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(actionResult.Result);
            var createdAtActionResult = actionResult.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);
            Assert.AreEqual(201, createdAtActionResult.StatusCode);
            Assert.AreEqual(nameof(_controller.GetDocument), createdAtActionResult.ActionName);
            Assert.AreEqual(capturedDocumentBl.Id, createdAtActionResult.RouteValues["id"]);

            var returnedDocumentDto = createdAtActionResult.Value as DocumentDTO;
            Assert.IsNotNull(returnedDocumentDto);
            Assert.AreEqual(capturedDocumentBl.Id, returnedDocumentDto.Id);
            Assert.AreEqual(capturedDocumentBl.Title, returnedDocumentDto.Title);
            Assert.AreEqual(capturedDocumentBl.Filepath, returnedDocumentDto.Filepath);

            _mockMapper.Verify(m => m.Map<DocumentBl>(It.IsAny<DocumentDTO>()), Times.Once);
            _mockService.Verify(s => s.CreateDocument(It.IsAny<DocumentBl>(), memoryStream, "text/plain"), Times.Once);
        }

        [Test]
        public async Task PostDocument_ServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Dummy file content";
            var fileName = "file.txt";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(_ => _.Length).Returns(memoryStream.Length);
            fileMock.Setup(_ => _.ContentType).Returns("text/plain");

            var uploadDto = new DocumentUploadDto
            {
                File = fileMock.Object
            };

            var documentDto = new DocumentDTO
            {
                Id = Guid.NewGuid(),
                Title = fileName,
                Filepath = fileName
            };

            var documentBl = new DocumentBl
            {
                Id = documentDto.Id,
                Title = documentDto.Title,
                Filepath = documentDto.Filepath
            };

            _mockMapper.Setup(m => m.Map<DocumentBl>(It.IsAny<DocumentDTO>())).Returns(documentBl);

            _mockService.Setup(s => s.CreateDocument(documentBl, memoryStream, "text/plain"))
                        .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.PostDocument(uploadDto);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Internal server error.", objectResult.Value);

            _mockMapper.Verify(m => m.Map<DocumentBl>(It.IsAny<DocumentDTO>()), Times.Once);
            _mockService.Verify(s => s.CreateDocument(documentBl, memoryStream, "text/plain"), Times.Once);
        }

        #endregion


        #region SearchDocuments Tests

        [Test]
        public void SearchDocuments_ShouldReturnOk_WithDocuments_WhenDocumentsExist()
        {
            // Arrange
            var query = "test";
            var documents = new List<DocumentBl>
    {
        new DocumentBl { Id = Guid.NewGuid(), Title = "Doc1", Filepath = "path1.pdf" },
        new DocumentBl { Id = Guid.NewGuid(), Title = "Doc2", Filepath = "path2.pdf" }
    };
            _mockService.Setup(s => s.SearchDocuments(query)).Returns(documents);

            // Act
            var result = _controller.SearchDocuments(query) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(documents, result.Value);
            _mockService.Verify(s => s.SearchDocuments(query), Times.Once);
        }

        [Test]
        public void SearchDocuments_ShouldReturnNotFound_WhenNoDocumentsFound()
        {
            // Arrange
            var query = "nonexistent";
            var documents = new List<DocumentBl>();
            _mockService.Setup(s => s.SearchDocuments(query)).Returns(documents);

            // Act
            var result = _controller.SearchDocuments(query) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("No documents found matching the search criteria.", result.Value);
            _mockService.Verify(s => s.SearchDocuments(query), Times.Once);
        }

        #endregion

        #region DownloadDocument Tests

        [Test]
        public async Task DownloadDocument_ShouldReturnFile_WhenDocumentExistsAndFileExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new DocumentBl { Id = documentId, Title = "Doc1", Filepath = "path1.pdf" };
            var fileBytes = Encoding.UTF8.GetBytes("Dummy file content");

            _mockService.Setup(s => s.GetDocumentById(documentId)).Returns(document);
            _mockService.Setup(s => s.GetDocumentFile(documentId)).ReturnsAsync(fileBytes);

            // Act
            var result = await _controller.DownloadDocument(documentId) as FileContentResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("application/pdf", result.ContentType);
            Assert.AreEqual(document.Filepath, result.FileDownloadName);
            Assert.AreEqual(fileBytes, result.FileContents);
            _mockService.Verify(s => s.GetDocumentById(documentId), Times.Once);
            _mockService.Verify(s => s.GetDocumentFile(documentId), Times.Once);
        }

        [Test]
        public async Task DownloadDocument_ShouldReturnNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            _mockService.Setup(s => s.GetDocumentById(documentId)).Returns((DocumentBl)null);

            // Act
            var result = await _controller.DownloadDocument(documentId) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual($"Document with ID {documentId} not found!", result.Value);
            _mockService.Verify(s => s.GetDocumentById(documentId), Times.Once);
            _mockService.Verify(s => s.GetDocumentFile(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

    }
}
