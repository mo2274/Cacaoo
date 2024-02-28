using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_USER_GROUPS
    {

        [Key]
        public short Group_Cd { get; set; }
        public string Group_Name_En { get; set; } = String.Empty;
        public string? Group_Name_Ar { get; set; } = String.Empty;
        public bool Admin { get; set; }
        public bool? Show { get; set; }
        public System.DateTime User_Date_Time { get; set; }

    }
}
