using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class TXN_Cart
    {
        [Key]
        public long Cart_Id { get; set; }
        public long Product_Id { get; set; } 
        public long App_User_Id { get; set; }
        public int Qty { get; set; }
        public string? Comments { get; set; }
        public DateTime? Created_Datetime { get; set; }
        public DateTime? Updated_Datetime { get; set; }
    }
}
