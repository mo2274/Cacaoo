using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class SM_Home_Group_Details
{


    [Key]
    public long Group_Detail_Id { get; set; }
    public long Group_Id { get; set; }
    public short Group_Type_Id { get; set; }
    public long Id { get; set; }
    public bool Show { get; set; }      
    public int Sequence { get; set; }
    public string? Image_Url { get; set; } = string.Empty;
    public int Line_No { get; set; }
    
    [NotMapped]
    public string Item_Name { get; set; } = "";
    [NotMapped]
    public string Group_Type_Name { get; set; } = "";
}