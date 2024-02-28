using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL.Models
{
    public class TXN_Wallet
    {
        [Key]
        public long TXN_Id { get; set; }
        public string TXN_Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public long App_User_Id { get; set; }
        public long? Order_Id { get; set; }
        public string? Comments { get; set; } = string.Empty;
        public string? Remarks { get; set; } = string.Empty;
        public int? Created_By { get; set; }
        public DateTime? Created_Datetime { get; set; }
    }
}
