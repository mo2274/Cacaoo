using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class TXN_Cart_AddOns
{
    [Key]
    public long Cart_AddOnId { get; set; }
    public long Product_AddOnId { get; set; }
    public long Cart_Id { get; set; }
}