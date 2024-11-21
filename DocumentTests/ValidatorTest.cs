using System.Reflection.Metadata;
using BL.Validators;
using SharedResources.Entities;

namespace DocumentTests
{
    [TestFixture]
    public class DocumentBLValidatorAdditionalTests
    {
        private DocumentValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new DocumentValidator();
        }

        [Test]
        public void EmptyTitle_ShouldFail()
        {
            // Arrange
            var document = new DocumentBl { Id = Guid.NewGuid(), Title = "", Filepath = "/valid/path" };

            // Act
            var result = _validator.Validate(document);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Title is required.", result.Errors.First().ErrorMessage);
        }

        [Test]
        public void Validate_TooLongTitle_ShouldFail()
        {
            // Arrange
            var longTitle = new string('A', 101); // Title too long
            var document = new DocumentBl { Id = Guid.NewGuid(), Title = longTitle, Filepath = "/valid/path" };

            // Act
            var result = _validator.Validate(document);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Title cannot exceed 100 characters.", result.Errors.First().ErrorMessage);
        }

        [Test]
        public void EmptyFilepath_ShouldFail()
        {
            // Arrange
            var document = new DocumentBl { Id = Guid.NewGuid(), Title = "Valid Title", Filepath = "" };

            // Act
            var result = _validator.Validate(document);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Filepath is required.", result.Errors.First().ErrorMessage);
        }

        [Test]
        public void ShouldReturnError_WhenTitleIsTooShort()
        {
            // Arrange
            var validator = new DocumentValidator();
            var invalidDocument = new DocumentBl { Id = Guid.NewGuid(), Title = "v", Filepath = "validpath", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            // Act
            var result = validator.Validate(invalidDocument);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Title must be at least 5 characters long.", result.Errors[0].ErrorMessage);
        }
    }

}