using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ChocolateDelivery.DAL;

public class SM_Restaurant_AddOns
{
    [Key]
    public long AddOn_Id { get; set; }
    public long Restaurant_Id { get; set; }
    public short AddOn_Type_Id { get; set; }
    public string AddOn_Name_E { get; set; } = string.Empty;
    public string? AddOn_Name_A { get; set; } = string.Empty;
    public string? AddOn_Desc_E { get; set; } = string.Empty;
    public string? AddOn_Desc_A { get; set; } = string.Empty;      
    public string? Image_URL { get; set; } = string.Empty;       
    public bool Show { get; set; }
    public int Sequence { get; set; } = 1;       
    public int? Created_By { get; set; }
    public DateTime? Created_Datetime { get; set; }
    public int? Updated_By { get; set; }
    public DateTime? Updated_Datetime { get; set; }      
    public Guid Row_Id { get; set; }
    
    [NotMapped]
    public IFormFile? Image_File { get; set; }
}