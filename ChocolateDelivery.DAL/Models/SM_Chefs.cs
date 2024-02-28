using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_Chefs
    {
        [Key]
        public long Chef_Id { get; set; }
        public string Chef_Name_E { get; set; } = string.Empty;
        public string? Chef_Name_A { get; set; } = string.Empty;
        public string? Chef_Desc_E { get; set; } = string.Empty;
        public string? Chef_Desc_A { get; set; } = string.Empty;
        public short Type_Id { get; set; } = 1;
        public string? Image_URL { get; set; } = string.Empty;
      
        public bool Show { get; set; }
        public int Sequence { get; set; } = 1;
        public int? Created_By { get; set; }
        public DateTime? Created_Datetime { get; set; }
        public int? Updated_By { get; set; }
        public DateTime? Updated_Datetime { get; set; }
    }
}
