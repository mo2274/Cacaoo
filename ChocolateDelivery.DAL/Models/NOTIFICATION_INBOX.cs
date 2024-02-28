using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class NOTIFICATION_INBOX
    {
        [Key]
        public long Notification_Id { get; set; }
        public long Campaign_Id { get; set; }
        public long App_User_Id { get; set; }
        public bool Is_Read { get; set; }
        public bool Is_Deleted { get; set; }
    }
}
