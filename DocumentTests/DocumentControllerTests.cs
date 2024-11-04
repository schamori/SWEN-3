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

namespace DocumentTests
{
    [TestFixture]
    public class DocumentControllerTests : IDisposable
    {
        private Mock<IDocumentRepo> _mockRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<IQueueProducer> _mockQueueProducer;
        private DocumentController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IDocumentRepo>();
            _mockMapper = new Mock<IMapper>();
            _mockQueueProducer = new Mock<IQueueProducer>();
            _controller = new DocumentController(_mockRepo.Object, _mockMapper.Object, _mockQueueProducer.Object);
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the controller if it implements IDisposable
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
            // Arrange
            var documentList = new List<DocumentDAL>
        {
            new DocumentDAL { Id = Guid.NewGuid(), Title = "Doc1", Filepath = "/path1" },
            new DocumentDAL { Id = Guid.NewGuid(), Title = "Doc2", Filepath = "/path2" }
        };

            var documentDTOList = new List<DocumentDTO>
        {
            new DocumentDTO { Id = Guid.NewGuid(), Title = "Doc1", Filepath = "/path1" },
            new DocumentDTO { Id = Guid.NewGuid(), Title = "Doc2", Filepath = "/path2" }
        };

            _mockRepo.Setup(repo => repo.Get()).Returns(documentList);
            _mockMapper.Setup(mapper => mapper.Map<List<DocumentDTO>>(It.IsAny<List<DocumentBl>>())).Returns(documentDTOList);

            // Act
            var result = _controller.Get() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void GetDocument_ShouldReturnDocument_WhenDocumentExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new DocumentDAL { Id = documentId, Title = "Doc1", Filepath = "/path1" };

            _mockRepo.Setup(repo => repo.Read(documentId)).Returns(document);

            // Act
            var result = _controller.GetDocument(documentId) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void GetDocument_ShouldReturnNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            _mockRepo.Setup(repo => repo.Read(documentId)).Returns((DocumentDAL)null);

            // Act
            var result = _controller.GetDocument(documentId) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }
    }
}
