using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class TXN_Order_Detail_AddOns
{
    [Key]
    public long Detail_AddOnId { get; set; }
    public long Order_Detail_Id { get; set; }
    public long Product_AddOnId { get; set; }
    public decimal Price { get; set; }
    
    [NotMapped]
    public string AddOn_Name { get; set; } = "";
}