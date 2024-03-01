using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class SM_Product_AddOns
{
    [Key]
    public long Product_AddOnId { get; set; }
    public long AddOn_Id { get; set; }
    public long Product_Id { get; set; }      
    public string Line_AddOn_Name_E { get; set; } = string.Empty;
    public string? Line_AddOn_Name_A { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Publish { get; set; }
    public bool Is_Deleted { get; set; }
    public int? Deleted_By { get; set; }
    public DateTime? Deleted_Datetime { get; set; }
    [NotMapped]
    public string AddOn_Name { get; set; } = "";
    [NotMapped]
    public string AddOn_Type { get; set; } = "";
}