using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class SM_Product_Occasions
    {
        [Key]
        public long Product_Occasion_Id { get; set; }
        public long Occasion_Id { get; set; }
        public long Product_Id { get; set; }
    }
}
