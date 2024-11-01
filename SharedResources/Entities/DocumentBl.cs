namespace SharedResources.Entities
{
    public class DocumentBl
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Filepath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
    }
}
