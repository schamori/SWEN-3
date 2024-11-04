using SharedResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentTests
{
    [TestFixture]
    public class DocumentDALTests
    {
        [Test]
        public void DocumentDAL_ShouldInitializeCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var title = "Test Title";
            var filepath = "/path/to/file";
            var createdAt = DateTime.Now;
            var updatedAt = DateTime.Now.AddMinutes(10);

            // Act
            var documentDAL = new DocumentDAL
            {
                Id = id,
                Title = title,
                Filepath = filepath,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            Assert.AreEqual(id, documentDAL.Id);
            Assert.AreEqual(title, documentDAL.Title);
            Assert.AreEqual(filepath, documentDAL.Filepath);
            Assert.AreEqual(createdAt, documentDAL.CreatedAt);
            Assert.AreEqual(updatedAt, documentDAL.UpdatedAt);
        }
    }

}
