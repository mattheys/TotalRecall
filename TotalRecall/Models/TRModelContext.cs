using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.TotalRecall
{
    public class TRModelContext : DbContext
    {
        public DbSet<Application> Applications { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite("Data Source=my.db");
        }
    }

    public class Application
    {
        public int ApplicationId { get; set; }
        [Required][Display(Name = "Application Name")]
        public string Name { get; set; }
        public Guid PublicKey { get; set; }
        public Guid PrivateKey { get; set; }
        public bool HideFromSearch { get; set; }
        public List<Data> Data { get; set; }
    }

    public class Data
    {
        public int DataId { get; set; }
        public List<DataItem> DataItems { get; set; }
    }

    public class DataItem
    {
        public int DataItemId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

}
