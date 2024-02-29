using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class SM_Payment_Types
    {
        [Key]
        public short Payment_Type_Id { get; set; }
        public string Payment_Type_Name_E { get; set; } = string.Empty;
        public string Payment_Type_Name_A { get; set; } = string.Empty;
        public bool Show { get; set; }
        public string tap_payment_id { get; set; }
        public string icon { get; set; }
    }
}
