using Microsoft.EntityFrameworkCore;

namespace TotalRecall.Models
{
    public class TRContext : DbContext
    {
        public DbSet<TRApplication> Applications { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite("Data Source=totalrecall.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TRApplication>().
                HasIndex(i => new { i.LastUpdated }).
                HasName("idx_App_LastUpdated");
        }
    }
}
