using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TotalRecall.Models
{
    public class TRModelContext : DbContext
    {
        public DbSet<Application> Applications { get; set; }
        public DbSet<Data> Data { get; set; }
        public DbSet<DataItem> DataItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite("Data Source=my.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DataItem>().
                HasIndex(i => new { i.DataId, i.PropertyName }).
                HasName("idxDataIdPropName");
        }
    }

    public class Application
    {
        public Application()
        {
            Data = new List<Data>();
        }
        public int ApplicationId { get; set; }
        [Required][Display(Name = "Application Name")]
        public string Name { get; set; }
        public Guid PublicKey { get; set; }
        public Guid PrivateKey { get; set; }
        public bool HideFromSearch { get; set; }
        public DateTime InsertDate { get; set; }
        public List<Data> Data { get; set; }
    }

    public class Data
    {
        public Data()
        {
            DataItems = new List<DataItem>();
        }
        public int DataId { get; set; }
        public int ApplicationId { get; set; }
        public DateTime InsertDate { get; set; }
        public List<DataItem> DataItems { get; set; }
    }

    public class DataItem
    {
        public int DataItemId { get; set; }
        public int DataId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

}
