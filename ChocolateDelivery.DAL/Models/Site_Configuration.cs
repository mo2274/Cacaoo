using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class Site_Configuration
    {
        [Key]
        public int Config_Id { get; set; }
        public string Config_Name { get; set; } = string.Empty;
        public string Config_Value { get; set; } = string.Empty;       
        public short Config_Type { get; set; }
        public string? Subject { get; set; }    
        public string? From_Email { get; set; }
        public string? Password { get; set; }
        public string? CC_Email { get; set; }
        public string? BCC_Email { get; set; }
    }
}
