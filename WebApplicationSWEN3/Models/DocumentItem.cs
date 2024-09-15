using Microsoft.EntityFrameworkCore;

public class DocumentItem
{
    // Properties
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; }

    public static implicit operator DbSet<object>(DocumentItem v)
    {
        throw new NotImplementedException();
    }
}
