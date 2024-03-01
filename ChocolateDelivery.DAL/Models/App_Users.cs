using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class App_Users
{
    [Key]
    public int App_User_Id { get; set; }
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public int? Cust_Id { get; set; }
    public DateTime? Created_Datetime { get; set; }
    public string Email { get; set; } = "";
    public string Mobile { get; set; } = "";
    public short Login_Type { get; set; }
    public short App_User_Type { get; set; }
    public DateTime? Updated_Datetime { get; set; }
    public bool Show { get; set; }
    public string? Google_Id { get; set; }
    public string? Facebook_Id { get; set; }
    public string? Apple_Id { get; set; }
    public Guid Row_Id { get; set; }
    public short? Created_By { get; set; }
    public short? Updated_By { get; set; }
    public string? Plate_Num { get; set; }
    public decimal Current_Points { get; set; }
}