using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class Device_Registration
{
    [Key]
    public string Device_Id { get; set; } = "";
    public string Notification_Token { get; set; } = "";
    public int? Device_Type { get; set; }
    public long? App_User_Id { get; set; }
    public System.DateTime? Logged_In { get; set; }
    public bool? Notification_Enabled { get; set; }
    public bool? NotificationSound_Enabled { get; set; }
    public System.DateTime Created_Datetime { get; set; }
    public string? Client_Key { get; set; }
    public string? Mobile { get; set; }
    public string? Code { get; set; }
    public string? Preferred_Language { get; set; }
    public short App_Type { get; set; }
}