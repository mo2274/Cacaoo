namespace ChocolateDelivery.UI.Areas.Admin.Models;

public class ItemDeleteRequest
{
    public long Detail_Id { get; set; }
    public int Deleted_By { get; set; }
}
public class ItemDeleteResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";

}