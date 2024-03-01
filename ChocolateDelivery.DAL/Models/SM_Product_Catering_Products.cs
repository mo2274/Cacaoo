using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class SM_Product_Catering_Products
{
    [Key]
    public long Catering_Product_Id { get; set; }
    public long Product_Id { get; set; }
    public long Category_Id { get; set; }
    public long Child_Product_Id { get; set; }
    public bool Publish { get; set; }
    public bool Is_Deleted { get; set; }
    public int? Deleted_By { get; set; }
    public DateTime? Deleted_Datetime { get; set; }
    
    [NotMapped]
    public string Category_Name { get; set; } = "";
    [NotMapped]
    public string Product_Name { get; set; } = "";
}