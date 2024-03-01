using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class TXN_Order_Detail_Catering_Products
{
    [Key]
    public long Detail_Categoring_Product_Id { get; set; }
    public long Detail_Id { get; set; }
    public long Category_Product_Id { get; set; }
    public int Qty { get; set; }
    
    [NotMapped]
    public string Product_Name { get; set; } = "";

}