using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class SM_Restaurant_Branches
{
    [Key]
    public long Branch_Id { get; set; }
    public long Restaurant_Id { get; set; }
    public string Branch_Name_E { get; set; } = string.Empty;
    public string? Branch_Name_A { get; set; } = string.Empty;     
    public string? Address_E { get; set; } = string.Empty;
    public string? Address_A { get; set; } = string.Empty;     
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool Show { get; set; }
    public int Sequence { get; set; } = 1;
    public TimeSpan? Opening_Time { get; set; }
    public TimeSpan? Closing_Time { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;      
    public int? Created_By { get; set; }
    public DateTime? Created_Datetime { get; set; }
    public int? Updated_By { get; set; }
    public DateTime? Updated_Datetime { get; set; }
    
    [NotMapped]
    public string Restaurant_Name { get; set; } = "";

    [NotMapped]
    public string Opening_Time_String { get; set; } = "";
    [NotMapped]
    public string Closing_Time_String { get; set; } = "";
}