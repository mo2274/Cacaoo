namespace ChocolateDelivery.UI.Areas.Merchant.Models
{
    public class MerchantModel
    {
    }
    public class AddOnDeleteRequest
    {
        public long Detail_Id { get; set; }
        public int Deleted_By { get; set; }
    }
    public partial class AddOnDeleteResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";

    }
}
