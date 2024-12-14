using Microsoft.EntityFrameworkCore;
using SharedResources.Entities;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace DAL.Persistence
{
    [ExcludeFromCodeCoverage]
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        

        public DbSet<DocumentDAL> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DocumentDAL>()
                .ToTable("Documents")
                .HasKey(t => t.Id);

            modelBuilder.Entity<DocumentDAL>()
                .Property(t => t.Id)
                .HasColumnType("uuid");

        }
    }
}
