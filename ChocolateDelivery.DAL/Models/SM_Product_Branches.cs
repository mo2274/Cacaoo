using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class SM_Product_Branches
{


    [Key]
    public long Product_Branch_Id { get; set; }
    public long Product_Id { get; set; }
    public long Branch_Id { get; set; }     
    public bool Is_Available { get; set; }
    [NotMapped]
    public string Branch_Name { get; set; } = "";
}