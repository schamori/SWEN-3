using NUnit.Framework;
using DAL.DTO;
using DAL.Validators;
using FluentValidation.TestHelper;

namespace DAL.Tests
{
    [TestFixture]
    public class DocumentValidatorTests
    {
        private DocumentValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new DocumentValidator();
        }

        [Test]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            var dto = new CreateDocumentDTO { Title = "", Content = "Valid content" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(doc => doc.Title).WithErrorMessage("Title is required.");
        }

        [Test]
        public void Should_Not_Have_Error_When_DTO_Is_Valid()
        {
            var dto = new CreateDocumentDTO { Title = "Valid Title", Content = "Valid content that meets all requirements." };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(doc => doc.Title);
            result.ShouldNotHaveValidationErrorFor(doc => doc.Content);
        }
    }
}