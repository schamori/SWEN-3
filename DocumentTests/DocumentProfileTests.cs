using AutoMapper;
using SharedResources.DTO;
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
    public class DocumentProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<DocumentProfile>());
            _mapper = configuration.CreateMapper();
        }

        [Test]
        public void Should_Map_DocumentDTO_To_DocumentBl()
        {
            // Arrange
            var documentDTO = new DocumentDTO
            {
                Id = Guid.NewGuid(),
                Title = "Test Title",
                Filepath = "/path/to/file",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now.AddMinutes(10)
            };

            // Act
            var documentBl = _mapper.Map<DocumentBl>(documentDTO);

            // Assert
            Assert.AreEqual(documentDTO.Id, documentBl.Id);
            Assert.AreEqual(documentDTO.Title, documentBl.Title);
            Assert.AreEqual(documentDTO.Filepath, documentBl.Filepath);
        }

        [Test]
        public void Should_Map_DocumentBl_To_DocumentDTO()
        {
            // Arrange
            var documentBl = new DocumentBl
            {
                Id = Guid.NewGuid(),
                Title = "Test Title",
                Filepath = "/path/to/file",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now.AddMinutes(10)
            };

            // Act
            var documentDTO = _mapper.Map<DocumentDTO>(documentBl);

            // Assert
            Assert.AreEqual(documentBl.Id, documentDTO.Id);
            Assert.AreEqual(documentBl.Title, documentDTO.Title);
            Assert.AreEqual(documentBl.Filepath, documentDTO.Filepath);
        }

        [Test]
        public void DocumentProfile_ShouldMapDocumentBlToDocumentDAL()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DocumentProfile>());
            var mapper = config.CreateMapper();
            var documentBl = new DocumentBl { Id = Guid.NewGuid(), Title = "Valid Title", Filepath = "validpath" };

            // Act
            var documentDal = mapper.Map<DocumentDAL>(documentBl);

            // Assert
            Assert.AreEqual(documentBl.Id, documentDal.Id);
            Assert.AreEqual(documentBl.Title, documentDal.Title);
            Assert.AreEqual(documentBl.Filepath, documentDal.Filepath);
        }

        [Test]
        public void DocumentProfile_ShouldMapDocumentDALToDocumentBl()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DocumentProfile>());
            var mapper = config.CreateMapper();
            var documentDal = new DocumentDAL { Id = Guid.NewGuid(), Title = "Valid Title", Filepath = "validpath" };

            // Act
            var documentBl = mapper.Map<DocumentBl>(documentDal);

            // Assert
            Assert.AreEqual(documentDal.Id, documentBl.Id);
            Assert.AreEqual(documentDal.Title, documentBl.Title);
            Assert.AreEqual(documentDal.Filepath, documentBl.Filepath);
        }


    }

}
