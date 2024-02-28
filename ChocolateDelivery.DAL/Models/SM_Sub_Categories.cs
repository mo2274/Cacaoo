using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_Sub_Categories
    {
        [Key]
        public long Sub_Category_Id { get; set; }
        public long Category_Id { get; set; }
        public string Sub_Category_Name_E { get; set; } = string.Empty;
        public string? Sub_Category_Name_A { get; set; } = string.Empty;
        public string? Sub_Category_Desc_E { get; set; } = string.Empty;
        public string? Sub_Category_Desc_A { get; set; } = string.Empty;
        public string? Image_URL { get; set; } = string.Empty;
        public bool Show { get; set; }
        public int Sequence { get; set; } = 1;
        public string? Background_Color { get; set; } = string.Empty;
        public int? Created_By { get; set; }
        public DateTime? Created_Datetime { get; set; }
        public int? Updated_By { get; set; }
        public DateTime? Updated_Datetime { get; set; }
    }
}
