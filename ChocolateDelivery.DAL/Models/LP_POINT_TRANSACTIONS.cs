using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class LP_POINTS_TRANSACTION
    {
        [Key]
        public long TXN_Id { get; set; }
        public int Type_Id { get; set; }
        public DateTime TXN_Date { get; set; }
        public long App_User_Id { get; set; }
        public long Order_Id { get; set; }
        public DateTime Created_Datetime { get; set; }
        public decimal Points { get; set; }
    }
}
