using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class SM_Chef_Products
{
    [Key]
    public long Id { get; set; }
    public long Chef_Id { get; set; }
    public long Product_Id { get; set; }
    public int Sequence { get; set; }
    
    [NotMapped]
    public string Product_Name { get; set; } = "";
}