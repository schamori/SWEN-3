﻿using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for each entity that should be mapped to the database
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: If you have any specific configurations for your entities
            // e.g., modelBuilder.Entity<Document>().Property(d => d.Name).HasMaxLength(100);
        }
    }
}
