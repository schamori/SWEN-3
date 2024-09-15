using Microsoft.EntityFrameworkCore;

namespace sws.Models;

public class DocumentContext : DbContext
{
    public DocumentContext(DbContextOptions<DocumentContext> options)
        : base(options)
    {

    }

    public DbSet<DocumentItem> DocumentItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add hardcoded entry
        modelBuilder.Entity<DocumentItem>().HasData(new DocumentItem
        {
            Id = 1,
            Title = "Hardcoded Document",
            Content = "This is a hardcoded document entry"
        });

    }
}
