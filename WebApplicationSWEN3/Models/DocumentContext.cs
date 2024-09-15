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

       
    }
}
