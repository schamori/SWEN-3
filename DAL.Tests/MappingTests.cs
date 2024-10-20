using AutoMapper;
using NUnit.Framework;
using DAL.DTO;
using DAL.Entities;
using DAL.Mappers;

namespace DAL.Tests
{
    [TestFixture]
    public class MappingTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DocumentProfile>(); // Verwendet deine AutoMapper-Konfiguration
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void Should_Map_Document_To_DocumentDTO()
        {
            // Arrange
            var document = new Document
            {
                Id = 1,
                Title = "Test Title",
                Content = "Test Content"
            };

            // Act
            var documentDto = _mapper.Map<DocumentDTO>(document);

            // Assert
            Assert.AreEqual(document.Id, documentDto.Id);
            Assert.AreEqual(document.Title, documentDto.Title);
            Assert.AreEqual(document.Content, documentDto.Content);
        }

        [Test]
        public void Should_Map_DocumentDTO_To_Document()
        {
            // Arrange
            var documentDto = new DocumentDTO
            {
                Id = 1,
                Title = "DTO Title",
                Content = "DTO Content"
            };

            // Act
            var document = _mapper.Map<Document>(documentDto);

            // Assert
            Assert.AreEqual(documentDto.Id, document.Id);
            Assert.AreEqual(documentDto.Title, document.Title);
            Assert.AreEqual(documentDto.Content, document.Content);
        }
    }
}
