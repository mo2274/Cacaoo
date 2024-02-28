using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class TXN_Order_Detail_Catering_Products
    {
        [Key]
        public long Detail_Categoring_Product_Id { get; set; }
        public long Detail_Id { get; set; }
        public long Category_Product_Id { get; set; }
        public int Qty { get; set; }
    }
}
