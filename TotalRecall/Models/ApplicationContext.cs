using Microsoft.EntityFrameworkCore;
using System;

namespace TotalRecall.Models
{
    public class ApplicationContext : DbContext
    {
        private string dbFolder = "dbs" + System.IO.Path.DirectorySeparatorChar;
        public Guid PublicKey { get; set; }
        public ApplicationContext(Guid publicKey)
        {
            PublicKey = publicKey;
        }

        public DbSet<Data> Data { get; set; }

        public DbSet<DataItem> DataItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite($"Data Source={dbFolder}{PublicKey.ToString()}.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DataItem>().
                HasIndex(i => new { i.DataId, i.PropertyName }).
                HasName("idx_DataItem_DataId_PropName");

            modelBuilder.Entity<Data>().
                HasIndex(i => new { i.InsertDate }).
                HasName("idx_Data_InsertDate");
        }
    }

}
