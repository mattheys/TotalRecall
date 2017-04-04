using System;
using System.ComponentModel.DataAnnotations;

namespace TotalRecall.Models
{
    public class TRApplication
    {
        public int TRApplicationId { get; set; }

        [Required][Display(Name = "Application Name")]
        public string Name { get; set; }

        [Display(Name = "Application Description", Description ="Description of the Application")]
        public string Description { get; set; }

        [Display(Name = "Public Key", Description = "Used to view data, anyone with this key can read the database")]
        public Guid PublicKey { get; set; }

        public Guid UpdateKey { get; set; }

        public Guid AdminKey { get; set; }

        public bool Public { get; set; }

        [EmailAddress]
        public String Email { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
