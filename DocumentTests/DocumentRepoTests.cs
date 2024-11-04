using AutoMapper;
using DAL.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using SharedResources.Entities;
using SharedResources.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentTests
{
    [TestFixture]
    public class DocumentRepoTests
    {
        private ApplicationDbContext _context;
        private DocumentRepo _documentRepo;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<DocumentProfile>());
            _context = new ApplicationDbContext(options);
            _mapper = configuration.CreateMapper();
            _documentRepo = new DocumentRepo(_context, _mapper); // Falls du kein Mapper verwendest, kannst du null übergeben
        }

        [TearDown]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void Get_ShouldReturnAllDocuments()
        {
            // Arrange
            _context.Documents.Add(new DocumentDAL { Id = Guid.NewGuid(), Title = "Doc 1", Filepath = "path1" });
            _context.Documents.Add(new DocumentDAL { Id = Guid.NewGuid(), Title = "Doc 2", Filepath = "path2" });
            _context.SaveChanges();

            // Act
            var result = _documentRepo.Get();

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void Create_ShouldAddNewDocument()
        {
            // Arrange
            var document = new DocumentDAL { Id = Guid.NewGuid(), Title = "Doc 1", Filepath = "path1" };

            // Act
            var result = _documentRepo.Create(document);

            // Assert
            Assert.AreEqual(document.Title, result.Title);
            Assert.AreEqual(1, _context.Documents.Count());
        }

        [Test]
        public void Read_ShouldReturnNull_WhenDocumentDoesNotExist()
        {
            // Act
            var result = _documentRepo.Read(Guid.NewGuid());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Update_ShouldModifyExistingDocument()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var existingDocument = new DocumentDAL { Id = documentId, Title = "Old Title", Filepath = "oldpath" };
            _context.Documents.Add(existingDocument);
            _context.SaveChanges();

            var updatedDocument = new DocumentDAL { Id = documentId, Title = "New Title", Filepath = "newpath" };

            // Act
            _documentRepo.Update(updatedDocument);

            // Assert
            var result = _context.Documents.Find(documentId);
            Assert.AreEqual("New Title", result.Title);
            Assert.AreEqual("newpath", result.Filepath);
        }

        [Test]
        public void Delete_ShouldRemoveDocument_WhenDocumentExists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new DocumentDAL { Id = documentId, Title = "Doc to Delete", Filepath = "deletepath" };
            _context.Documents.Add(document);
            _context.SaveChanges();

            // Act
            _documentRepo.Delete(documentId);

            // Assert
            var result = _context.Documents.Find(documentId);
            Assert.IsNull(result);
            Assert.AreEqual(0, _context.Documents.Count());
        }

        [Test]
        public void Delete_ShouldDoNothing_WhenDocumentDoesNotExist()
        {
            // Arrange
            var nonExistentDocumentId = Guid.NewGuid();

            // Act
            _documentRepo.Delete(nonExistentDocumentId);

            // Assert
            Assert.AreEqual(0, _context.Documents.Count());
        }

        [Test]
        public void Update_ShouldNotModifyDocument_WhenDocumentDoesNotExist()
        {
            // Arrange
            var nonExistentDocument = new DocumentDAL { Id = Guid.NewGuid(), Title = "New Title", Filepath = "newpath" };

            // Act
            _documentRepo.Update(nonExistentDocument);

            // Assert
            Assert.AreEqual(0, _context.Documents.Count());
        }

        [Test]
        public void Get_ShouldReturnEmptyList_WhenNoDocumentsExist()
        {
            // Act
            var result = _documentRepo.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Update_ShouldNotModify_WhenNoChangesMade()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var document = new DocumentDAL { Id = documentId, Title = "Test Title", Filepath = "Test Path" };
            _context.Documents.Add(document);
            _context.SaveChanges();

            var unchangedDocument = new DocumentDAL { Id = documentId, Title = "Test Title", Filepath = "Test Path" };

            // Act
            var result = _documentRepo.Update(unchangedDocument);

            // Assert
            Assert.AreEqual("Test Title", result.Title);
            Assert.AreEqual("Test Path", result.Filepath);
        }


    }
}
