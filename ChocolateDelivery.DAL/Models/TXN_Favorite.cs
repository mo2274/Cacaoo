using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class TXN_Favorite
{
    [Key]
    public long Favorite_Id { get; set; }
    public long Product_Id { get; set; }
    public long App_User_Id { get; set; }
    public DateTime Created_Datetime { get; set; }
}