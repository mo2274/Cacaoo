using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class SM_Order_Status
    {
        [Key]
        public short Status_Id { get; set; }
        public string Status_Name_E { get; set; } = string.Empty;
        public string Status_Name_A { get; set; } = string.Empty;
      
    }
}
