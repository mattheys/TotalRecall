using System;
using System.Collections.Generic;

namespace TotalRecall.Models
{
    public class Data
    {
        public Data()
        {
            DataItems = new List<DataItem>();
        }
        public int DataId { get; set; }
        //public int ApplicationId { get; set; }
        public DateTime InsertDate { get; set; }
        public List<DataItem> DataItems { get; set; }
    }

}
