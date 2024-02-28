using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class SM_Areas
    {
        [Key]
        public int Area_Id { get; set; }
        public string Area_Name_E { get; set; } = "";
        public string? Area_Name_A { get; set; } = "";
        public decimal Delivery_Charge { get; set; }
        public bool Show { get; set; }
    }
}
