using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class TXN_Order_Details
    {
        [Key]
        public long Order_Detail_Id { get; set; }
        public long Order_Id { get; set; }
        public long Product_Id { get; set; }
        public string Product_Name { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Gross_Amount { get; set; }
        public decimal Discount_Amount { get; set; }
        public decimal Net_Amount { get; set; }
        public string? Promo_Code { get; set; } = string.Empty;
        public string? Comments { get; set; } = string.Empty;
        public int? Rating { get; set; }
    }
}
