using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class PAYMENTS
    {
        [Key]
        public long Id { get; set; }
        public long? Order_Id { get; set; }
        public decimal Amount { get; set; }
        public string? Track_Id { get; set; } = "";
        public string? Payment_Id { get; set; } = "";
        public string? Trans_Id { get; set; } = "";
        public string? Auth { get; set; } = "";
        public string? Reference_No { get; set; } = "";
        public string? Result { get; set; } = "";
        public string? Payment_Mode { get; set; } = "";
        public DateTime? Created_Datetime { get; set; }
        public DateTime? Updated_Datetime { get; set; }
        public string? Comments { get; set; } = "";
     
    }
}
