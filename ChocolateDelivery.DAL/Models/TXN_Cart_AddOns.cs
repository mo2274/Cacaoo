using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class TXN_Cart_AddOns
    {
        [Key]
        public long Cart_AddOnId { get; set; }
        public long Product_AddOnId { get; set; }
        public long Cart_Id { get; set; }
    }
}
