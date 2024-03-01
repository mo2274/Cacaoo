using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class SM_USER_GROUP_DASHBOARD
{
    [Key]
    public int Dashboard_Id { get; set; }
    public int Group_Cd { get; set; }
    public int List_Id { get; set; }
    public bool Show { get; set; }
    public int Sequence { get; set; }
}