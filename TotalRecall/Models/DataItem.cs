using System.ComponentModel.DataAnnotations;

namespace TotalRecall.Models
{
    public class DataItem
    {

        public int DataItemId { get; set; }

        public int DataId { get; set; }

        [MaxLength(50, ErrorMessage = "Maximum Property Name is 50 characters long")]
        public string PropertyName { get; set; }

        [MaxLength(50, ErrorMessage = "Maximum Property Value is 50 characters long")]
        public string PropertyValue { get; set; }
    }

}
