using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_Product_Branches
    {
        [Key]
        public long Product_Branch_Id { get; set; }
        public long Product_Id { get; set; }
        public long Branch_Id { get; set; }     
        public bool Is_Available { get; set; }
    }
}
