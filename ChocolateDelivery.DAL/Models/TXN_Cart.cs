using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class TXN_Cart
{
    [Key]
    public long Cart_Id { get; set; }
    public long Product_Id { get; set; } 
    public long App_User_Id { get; set; }
    public int Qty { get; set; }
    public string? Comments { get; set; }
    public DateTime? Created_Datetime { get; set; }
    public DateTime? Updated_Datetime { get; set; }
}