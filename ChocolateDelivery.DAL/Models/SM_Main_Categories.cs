using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class SM_Main_Categories
{
    [Key]
    public long Category_Id { get; set; }
    public string Category_Name_E { get; set; } = string.Empty;
    public string? Category_Name_A { get; set; } = string.Empty;
    public string? Category_Desc_E { get; set; } = string.Empty;
    public string? Category_Desc_A { get; set; } = string.Empty;     
    public bool Show { get; set; }
    public int Sequence { get; set; } = 1;
    public int? Created_By { get; set; }
    public DateTime? Created_Datetime { get; set; }
    public int? Updated_By { get; set; }
    public DateTime? Updated_Datetime { get; set; }
}