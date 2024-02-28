using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class APP_PUSH_CAMPAIGN
    {
        [Key]
        public long Campaign_Id { get; set; }
        public string Title_E { get; set; } = string.Empty;
        public string Title_A { get; set; } = string.Empty;
        public string? Desc_E { get; set; } = string.Empty;
        public string? Desc_A { get; set; } = string.Empty;
        public DateTime Created_Datetime { get; set; }
    }
}
