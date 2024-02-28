using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_Home_Groups
    {
        [Key]
        public long Group_Id { get; set; }
        public string Group_Name_E { get; set; } = string.Empty;
        public string Group_Name_A { get; set; } = string.Empty;      
        public short Display_Type { get; set; } = 1;       
        public bool Show { get; set; }
        public int Sequence { get; set; } = 1;
        public int? Created_By { get; set; }
        public DateTime? Created_Datetime { get; set; }
        public int? Updated_By { get; set; }
        public DateTime? Updated_Datetime { get; set; }
    }
}
