using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_Products
    {
        [Key]
        public long Product_Id { get; set; }
        public int Main_Category_Id { get; set; }
        public int Product_Type_Id { get; set; }
        public int Sub_Category_Id { get; set; }
        public long Restaurant_Id { get; set; }
        public string Product_Name_E { get; set; } = string.Empty;
        public string? Product_Name_A { get; set; } = string.Empty;
        public string? Product_Desc_E { get; set; } = string.Empty;
        public string? Product_Desc_A { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Image_URL { get; set; } = string.Empty;
        public bool Show { get; set; }
        public bool Publish { get; set; }
        public int Sequence { get; set; } = 1;
        public decimal Stock_In_Hand { get; set; }
        public int? Created_By { get; set; }
        public DateTime? Created_Datetime { get; set; }
        public int? Updated_By { get; set; }
        public DateTime? Updated_Datetime { get; set; }
        public Guid Row_Id { get; set; }
        public string? Comments { get; set; } = string.Empty;
        public string? Admin_Comments { get; set; } = string.Empty;
        public int Status_Id { get; set; } = 1;
        public long? Brand_Id { get; set; }
        public string? Short_Desc_E { get; set; } = string.Empty;
        public string? Short_Desc_A { get; set; } = string.Empty;
        public string? Nutritional_Facts_E { get; set; } = string.Empty;
        public string? Nutritional_Facts_A { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int Total_Ratings { get; set; }
        public bool Is_Exclusive { get; set; }
        public bool Is_Catering { get; set; }
        public bool Is_Catering_Menu_Product { get; set; }
    }
}
