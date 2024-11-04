using SharedResources.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentTests
{
    [TestFixture]
    public class DocumentDTOTests
    {
        [Test]
        public void DocumentDTO_ShouldInitializeCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var title = "Test Title";
            var filepath = "/path/to/file";
            var createdAt = DateTime.Now;
            var updatedAt = DateTime.Now.AddMinutes(10);

            // Act
            var documentDTO = new DocumentDTO
            {
                Id = id,
                Title = title,
                Filepath = filepath,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            Assert.AreEqual(id, documentDTO.Id);
            Assert.AreEqual(title, documentDTO.Title);
            Assert.AreEqual(filepath, documentDTO.Filepath);
            Assert.AreEqual(createdAt, documentDTO.CreatedAt);
            Assert.AreEqual(updatedAt, documentDTO.UpdatedAt);
        }
    }
}
