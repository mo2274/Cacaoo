using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class TXN_Cart_Catering_Products
{
    [Key]
    public long Cart_Catering_Product_Id { get; set; }
    public long Cart_Id { get; set; }
    public long Catering_Product_Id { get; set; }     
    public int Qty { get; set; }
}