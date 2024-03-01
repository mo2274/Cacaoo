using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class SM_USERS
{
    [Key]
    public short User_Cd { get; set; }
    public string User_Id { get; set; } = String.Empty;
    public short Group_Cd { get; set; }
    public string Password { get; set; } = String.Empty;
    public string User_Name { get; set; } = String.Empty;
    public string? Phone { get; set; } = String.Empty;
    public string Preferred_Language { get; set; } = String.Empty;
    public string? Comments { get; set; } = String.Empty;
    public long Entity_Id { get; set; }
    public string? Email { get; set; } = String.Empty;
    public string? Default_Page { get; set; } = String.Empty;
    
    [NotMapped]
    public string Group_Name_En { get; set; } = "";
    [NotMapped]
    public string Group_Name_Ar { get; set; } = "";
}