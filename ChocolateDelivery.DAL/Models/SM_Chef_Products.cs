using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_Chef_Products
    {
        [Key]
        public long Id { get; set; }
        public long Chef_Id { get; set; }
        public long Product_Id { get; set; }
        public int Sequence { get; set; }
    }
}
