using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class SM_MENU
{
    [Key]
    public long MenuId { get; set; }
    public long ParentMenuId { get; set; }
    public string MenuName_En { get; set; } = "";
    public string MenuName_Ar { get; set; } = "";
    public string? PageUrl { get; set; } = "";
    public string? Icon { get; set; } = "";
    public int OrderBy { get; set; }
    public bool Show { get; set; }
}