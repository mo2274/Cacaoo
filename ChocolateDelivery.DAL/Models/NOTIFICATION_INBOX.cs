using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class NOTIFICATION_INBOX
{

    [Key]
    public long Notification_Id { get; set; }
    public long Campaign_Id { get; set; }
    public long App_User_Id { get; set; }
    public bool Is_Read { get; set; }
    public bool Is_Deleted { get; set; }
    
    [NotMapped]
    public string Title { get; set; } = "";
    [NotMapped]
    public string Desc { get; set; } = "";
    [NotMapped]
    public DateTime Created_Datetime { get; set; }
}