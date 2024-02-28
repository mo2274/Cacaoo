using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class APP_SETTINGS
    {
        [Key]
        public int SETTING_ID { get; set; }
        public int SETTING_TYPE { get; set; }
        public string SETTING_NAME { get; set; } = "";
        public string SETTING_VALUE { get; set; } = "";
        public string COMMENTS { get; set; } = "";
        public bool ALLOW_ADMIN { get; set; }
    }
}
