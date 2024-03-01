using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class SM_USER_GROUPS
{

    [Key]
    public short Group_Cd { get; set; }
    public string Group_Name_En { get; set; } = String.Empty;
    public string? Group_Name_Ar { get; set; } = String.Empty;
    public bool Admin { get; set; }
    public bool? Show { get; set; }
    public System.DateTime User_Date_Time { get; set; }
    
    [NotMapped]
    public List<SM_USER_GROUP_RIGHTS> SM_USER_GROUP_RIGHTS { get; set; } = new();
}