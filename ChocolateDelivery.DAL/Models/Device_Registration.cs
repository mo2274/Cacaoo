using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public class Device_Registration
    {
        [Key]
        public string Device_Id { get; set; } = "";
        public string Notification_Token { get; set; } = "";
        public Nullable<int> Device_Type { get; set; }
        public Nullable<long> App_User_Id { get; set; }
        public Nullable<System.DateTime> Logged_In { get; set; }
        public Nullable<bool> Notification_Enabled { get; set; }
        public Nullable<bool> NotificationSound_Enabled { get; set; }
        public System.DateTime Created_Datetime { get; set; }
        public string? Client_Key { get; set; }
        public string? Mobile { get; set; }
        public string? Code { get; set; }
        public string? Preferred_Language { get; set; }
        public short App_Type { get; set; }
    }
}
