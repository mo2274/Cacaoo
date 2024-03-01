using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class SM_Carousels
{
    [Key]
    public long Carousel_Id { get; set; }
    public string Title_E { get; set; } = string.Empty;
    public string? Title_A { get; set; }
    public string? Sub_Title_E { get; set; }
    public string? Sub_Title_A { get; set; }
    public short Media_Type { get; set; }
    public short? Media_From_Type { get; set; }
    public long? Redirect_Id { get; set; }
       
    public int Sequence { get; set; }
    public bool Show { get; set; }
    public bool Is_Deleted { get; set; }
    public short? Deleted_By { get; set; }
    public DateTime? Deleted_Datetime { get; set; }
    public short? Created_By { get; set; }
    public DateTime Created_Datetime { get; set; }
    public short? Updated_By { get; set; }
    public DateTime? Updated_Datetime { get; set; }
    public string? Media_URL { get; set; }
    public string? Thumbnail_URL { get; set; }
}