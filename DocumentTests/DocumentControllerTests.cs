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

        [Test]
        public void Get_ShouldReturnListOfDocuments()
        {
            var documentList = new List<DocumentDTO>
            {
                new DocumentDTO { Id = Guid.NewGuid(), Title = "Doc1", Filepath = "/path1" },
                new DocumentDTO { Id = Guid.NewGuid(), Title = "Doc2", Filepath = "/path2" }
            };
            _mockService.Setup(service => service.GetDocuments()).Returns(documentList);

            var result = _controller.Get() as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void GetDocument_ShouldReturnDocument_WhenDocumentExists()
        {
            var documentId = Guid.NewGuid();
            var document = new DocumentDTO { Id = documentId, Title = "Doc1", Filepath = "/path1" };
            _mockService.Setup(service => service.GetDocumentById(documentId)).Returns(document);

            var result = _controller.GetDocument(documentId) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void GetDocument_ShouldReturnNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            _mockService.Setup(service => service.GetDocumentById(documentId)).Returns((DocumentDTO)null);  // Adjusted to use DocumentDTO

            // Act
            var result = _controller.GetDocument(documentId) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }
    }
}
