using System.Runtime.Serialization;

namespace DAL.DTO
{
    public class DocumentDTO
    {
        [DataMember(Name = "id", EmitDefaultValue = true)]
        public required int Id { get; set; }
        [DataMember(Name = "title", EmitDefaultValue = true)]
        public required string Title { get; set; }
        [DataMember(Name = "content", EmitDefaultValue = true)]


        public required string Content { get; set; }

        [DataMember(Name = "createdat", EmitDefaultValue = true)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DataMember(Name = "updatedat", EmitDefaultValue = true)]
        public DateTime UpdatedAt { get; set; }
    }
}
