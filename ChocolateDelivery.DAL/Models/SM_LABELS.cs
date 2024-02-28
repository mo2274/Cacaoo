using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class SM_LABELS
    {
        [Key]
        public int Label_Id { get; set; }
        [Required(ErrorMessage = "Enter Label Name")]
        public string L_Label_Name { get; set; } = "";
        public string? A_Label_Name { get; set; } = "";
        public bool Show { get; set; }
        public string? Label_Code { get; set; } = "";
        public bool Is_App { get; set; }
    }
}
