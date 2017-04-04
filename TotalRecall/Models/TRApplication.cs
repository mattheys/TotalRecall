using System;
using System.ComponentModel.DataAnnotations;

namespace TotalRecall.Models
{
    public class TRApplication
    {
        public int TRApplicationId { get; set; }

        [Required][Display(Name = "Application Name")]
        [MaxLength(100, ErrorMessage = "Max Application Name is 100 characters")]
        public string Name { get; set; }

        [Display(Name = "Application Description", Description ="Description of the Application")]
        [MaxLength(1000, ErrorMessage = "Max Descriptions length is 1000 characters")]
        public string Description { get; set; }

        [Display(Name = "Public Key", Description = "Used to view data, anyone with this key can read your database")]
        public Guid PublicKey { get; set; }

        [Display(Name = "Update Key", Description = "Used along with the Public Key to add data to your database")]
        public Guid UpdateKey { get; set; }

        [Display(Name = "Admin Key", Description = "This is your master key and allows you to do delete or download your database")]
        public Guid AdminKey { get; set; }

        public bool Public { get; set; }

        [EmailAddress]
        public String Email { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
