using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class TXN_Order_Logs
    {
        [Key]
        public long Log_Id { get; set; }
        public long Order_Id { get; set; }
        public short Status_Id { get; set; }
        public DateTime Created_Datetime { get; set; }
        public string? Comments { get; set; } = string.Empty;
        public long? Driver_Id { get; set; }
    }
}
