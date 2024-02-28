using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class TXN_Order_Detail_AddOns
    {
        [Key]
        public long Detail_AddOnId { get; set; }
        public long Order_Detail_Id { get; set; }
        public long Product_AddOnId { get; set; }
        public decimal Price { get; set; }

    }
}
