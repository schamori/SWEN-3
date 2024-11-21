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

namespace DocumentTests
{
    [TestFixture]
    public class DocumentServiceTests
    {
        private Mock<IDocumentRepo> _mockRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<IQueueProducer> _mockQueueProducer;
        private DocumentService _service;

        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IDocumentRepo>();
            _mockMapper = new Mock<IMapper>();
            _mockQueueProducer = new Mock<IQueueProducer>();
            _service = new DocumentService(_mockRepo.Object, _mockMapper.Object, _mockQueueProducer.Object);
        }

        [Test]
        public void GetDocumentById_ShouldReturnDocument_WhenDocumentExists()
        {
            var documentId = Guid.NewGuid();
            var documentDal = new DocumentDAL { Id = documentId, Title = "Test Doc", Filepath = "validPathh" };
            var documentDto = new DocumentDTO { Id = documentId, Title = "Test Doc" , Filepath = "validPathh" };

            _mockRepo.Setup(r => r.Read(documentId)).Returns(documentDal);
            _mockMapper.Setup(m => m.Map<DocumentDTO>(documentDal)).Returns(documentDto);

            var result = _service.GetDocumentById(documentId);

            Assert.IsNotNull(result);
            Assert.AreEqual(documentId, result.Id);
            Assert.AreEqual("Test Doc", result.Title);
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
        public void GetDocuments_ShouldReturnListOfDocuments()
        {
            var documentList = new List<DocumentDAL>
            {
                new DocumentDAL { Id = Guid.NewGuid(), Title = "Doc1", Filepath="validPathh" },
                new DocumentDAL { Id = Guid.NewGuid(), Title = "Doc2" , Filepath="validPathh"}
            };

            var documentDtoList = new List<DocumentDTO>
            {
                new DocumentDTO { Id = Guid.NewGuid(), Title = "Doc1", Filepath="validPathh"},
                new DocumentDTO { Id = Guid.NewGuid(), Title = "Doc2", Filepath="validPathh" }
            };

            _mockRepo.Setup(r => r.Get()).Returns(documentList);
            _mockMapper.Setup(m => m.Map<List<DocumentDTO>>(documentList)).Returns(documentDtoList);

            var result = _service.GetDocuments();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Doc1", result[0].Title);
            Assert.AreEqual("Doc2", result[1].Title);
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
    }
}
