using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ChocolateDelivery.DAL;

public class SM_Brands
{

    [Key]
    public long Brand_Id { get; set; }
    public long Restaurant_Id { get; set; }
    public string Brand_Name_E { get; set; } = string.Empty;
    public string? Brand_Name_A { get; set; } = string.Empty;
    public string? Brand_Desc_E { get; set; } = string.Empty;
    public string? Brand_Desc_A { get; set; } = string.Empty;
    public string? Image_URL { get; set; } = string.Empty;
    public bool Show { get; set; }
    public int Sequence { get; set; } = 1;
    public string? Background_Color { get; set; } = string.Empty;
    public int? Created_By { get; set; }
    public DateTime? Created_Datetime { get; set; }
    public int? Updated_By { get; set; }
    public DateTime? Updated_Datetime { get; set; }
    
    [NotMapped]
    public IFormFile? Image_File { get; set; }
    [NotMapped]
    public string Delivery_Time { get; set; } = "";
    [NotMapped]
    public decimal Delivery_Charge { get; set; }
    [NotMapped]
    public string Categories { get; set; } = "";

}