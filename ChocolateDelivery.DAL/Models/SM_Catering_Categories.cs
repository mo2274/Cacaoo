using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class SM_Catering_Categories
    {
        [Key]
        public long Category_Id { get; set; }
        public string Category_Name_E { get; set; } = string.Empty;
        public string Category_Name_A { get; set; } = string.Empty;
        public int Qty { get; set; }
        public int Restaurant_Id { get; set; }
        public bool Show { get; set; }
        public int Sequence { get; set; } = 1;      
        public int? Created_By { get; set; }
        public DateTime? Created_Datetime { get; set; }
        public int? Updated_By { get; set; }
        public DateTime? Updated_Datetime { get; set; }
    }
}
