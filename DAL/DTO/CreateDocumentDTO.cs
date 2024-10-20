using System.Runtime.Serialization;

namespace DAL.DTO
{
    public class CreateDocumentDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
