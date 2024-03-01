using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class SM_USER_GROUP_RIGHTS
{
    [Key]
    public long Right_Id { get; set; }
    public short Group_Cd { get; set; }
    public long? Menu_Id { get; set; }
    public bool AllowView { get; set; }
    public bool AllowAdd { get; set; }
    public bool AllowEdit { get; set; }
    public bool AllowDelete { get; set; }
    public System.DateTime User_Date_Time { get; set; }

    [NotMapped]
    public string MenuName_En { get; set; } = "";
}