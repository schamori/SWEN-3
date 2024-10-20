using NUnit.Framework;
using DAL.Entities;
using DAL.DTO;
using DAL.Persistence;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DAL.Tests
{
    [TestFixture]
    public class DocumentRepoTests
    {
        private ApplicationDbContext _context;
        private Mock<IMapper> _mockMapper;
        private DocumentRepo _documentRepo;

        [SetUp]
        public void Setup()
        {
            // Erstelle eine In-Memory-Datenbank und konfiguriere sie für den ApplicationDbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _mockMapper = new Mock<IMapper>();

            _documentRepo = new DocumentRepo(_context, _mockMapper.Object);
        }

        [TearDown]
        public void Cleanup()
        {
            // Lösche die Datenbank nach jedem Test, um sicherzustellen, dass keine Daten erhalten bleiben
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void Create_ShouldAddNewDocument()
        {
            // Arrange
            var documentDto = new DocumentDTO { Id = 1, Title = "Test Title", Content = "Test Content" };
            var documentEntity = new Document { Id = 1, Title = "Test Title", Content = "Test Content" };

            _mockMapper.Setup(m => m.Map<Document>(documentDto)).Returns(documentEntity);

            // Act
            _documentRepo.Create(documentDto);

            // Assert
            var addedDocument = _context.Documents.FirstOrDefault(d => d.Id == 1);
            Assert.IsNotNull(addedDocument);
            Assert.AreEqual("Test Title", addedDocument.Title);
        }

        [Test]
        public void Get_ShouldReturnAllDocuments()
        {
            // Arrange
            _context.Documents.AddRange(
                new Document { Id = 1, Title = "Doc 1", Content = "Content 1" },
                new Document { Id = 2, Title = "Doc 2", Content = "Content 2" }
            );
            _context.SaveChanges();

            // Act
            var result = _documentRepo.Get();

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void Read_ShouldReturnDocumentById()
        {
            // Arrange
            var documentEntity = new Document { Id = 1, Title = "Test Title", Content = "Test Content" };
            _context.Documents.Add(documentEntity);
            _context.SaveChanges();

            _mockMapper.Setup(m => m.Map<DocumentDTO>(documentEntity)).Returns(new DocumentDTO { Id = 1, Title = "Test Title", Content = "Test Content" });

            // Act
            var result = _documentRepo.Read(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test Title", result.Title);
        }

        [Test]
        public void Update_ShouldModifyExistingDocument()
        {
            // Arrange
            var existingDocument = new Document { Id = 1, Title = "Old Title", Content = "Old Content" };
            _context.Documents.Add(existingDocument);
            _context.SaveChanges();

            var updatedDto = new DocumentDTO { Id = 1, Title = "New Title", Content = "New Content" };

            _mockMapper.Setup(m => m.Map(updatedDto, existingDocument)).Callback<DocumentDTO, Document>((src, dest) =>
            {
                dest.Title = src.Title;
                dest.Content = src.Content;
            });

            // Act
            _documentRepo.Update(updatedDto);

            // Assert
            var updatedDocument = _context.Documents.FirstOrDefault(d => d.Id == 1);
            Assert.IsNotNull(updatedDocument);
            Assert.AreEqual("New Title", updatedDocument.Title);
            Assert.AreEqual("New Content", updatedDocument.Content);
        }


        [Test]
        public void Delete_ShouldRemoveDocumentById()
        {
            // Arrange
            var documentEntity = new Document { Id = 1, Title = "Test Title", Content = "Test Content" };
            _context.Documents.Add(documentEntity);
            _context.SaveChanges();

            // Act
            _documentRepo.Delete(1);

            // Assert
            var deletedDocument = _context.Documents.FirstOrDefault(d => d.Id == 1);
            Assert.IsNull(deletedDocument);
        }
    }
}
