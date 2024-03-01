using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class App_User_Address
{
    [Key]
    public long Address_Id { get; set; }
    public long App_User_Id { get; set; }
    public string Address_Name { get; set; } = "";
    public string Block { get; set; } = "";
    public string Street { get; set; } = "";
    public string Building { get; set; } = "";
    public string? Avenue { get; set; } = "";
    public string? Floor { get; set; } = "";
    public string? Apartment { get; set; } = "";
    public int Area_Id { get; set; }
    public string? Mobile { get; set; } = "";
    public string? Extra_Direction { get; set; } = "";
    public string? House_No { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Paci_Number { get; set; } = "";
    public DateTime Created_Datetime { get; set; }
    public DateTime? Updated_Datetime { get; set; }
    public bool Is_Deleted { get; set; }
    public DateTime? Deleted_Datetime { get; set; }
    
    [NotMapped]
    public string Area_Name { get; set; } = "";
}