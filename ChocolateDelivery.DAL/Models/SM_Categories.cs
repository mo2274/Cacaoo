using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ChocolateDelivery.DAL;

public class SM_Categories
{
    [Key]
    public long Category_Id { get; set; }
    public string Category_Name_E { get; set; } = string.Empty;
    public string? Category_Name_A { get; set; } = string.Empty;
    public string? Category_Desc_E { get; set; } = string.Empty;
    public string? Category_Desc_A { get; set; } = string.Empty;
    public string? Image_URL { get; set; } = string.Empty;
    public bool Show { get; set; }
    public int Sequence { get; set; } = 1;
    public string? Background_Color { get; set; } = string.Empty;
    public bool Is_Gift { get; set; }
    public int? Created_By { get; set; }
    public DateTime? Created_Datetime { get; set; }
    public int? Updated_By { get; set; }
    public DateTime? Updated_Datetime { get; set; }
    
    [NotMapped]
    public IFormFile? Image_File { get; set; }
}