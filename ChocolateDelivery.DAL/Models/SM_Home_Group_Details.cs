using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_Home_Group_Details
    {
        [Key]
        public long Group_Detail_Id { get; set; }
        public long Group_Id { get; set; }
        public short Group_Type_Id { get; set; }
        public long Id { get; set; }
        public bool Show { get; set; }      
        public int Sequence { get; set; }
        public string? Image_Url { get; set; } = string.Empty;
        public int Line_No { get; set; }

    }
}
