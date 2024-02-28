using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    internal class CustomDTO
    {
    }
	public partial class DriverOrderDTO
	{
		public long Order_Id { get; set; }
		public string Order_No { get; set; } = string.Empty;
		public string Pickup_Address { get; set; } = string.Empty;
		public string Delivery_Address { get; set; } = string.Empty;
		public decimal Order_Amount { get; set; }
		public string Order_Status { get; set; } = string.Empty;
		public string Order_Date { get; set; } = string.Empty;
		public string Payment_Type { get; set; } = string.Empty;
		public string Order_Time { get; set; } = string.Empty;
		public string Restaurant_Name { get; set; } = string.Empty;
		public string Restaurant_Branch { get; set; } = string.Empty;
		public string Driver_Name { get; set; } = string.Empty;
		public string Cancelled_Reason { get; set; } = string.Empty;
		public string Rejected_Reason { get; set; } = string.Empty;
		public string Customer_Mobile { get; set; } = string.Empty;
		public string Customer_Note { get; set; } = string.Empty;
		public string Staff_Note { get; set; } = string.Empty;
	}
	public class GroupsRightsDTO
    {
        public long Right_Id { get; set; }
        public long Menu_Id { get; set; }
        public string MenuName_En { get; set; } = string.Empty;
        public short Group_Cd { get; set; }
        public string Group_Name_En { get; set; } = string.Empty;
        public bool AllowView { get; set; }
        public bool AllowAdd { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }
    }
    public partial class CalenderDTO
    {
        public long Appointment_Detail_Id { get; set; }
        public long Appointment_Id { get; set; }
        public int Staff_Id { get; set; }
        public string Services_Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Customer_Name { get; set; } = string.Empty;
        public DateTime Start_Time { get; set; }
        public DateTime End_Time { get; set; }
        public string? Color { get; set; }
        public string? URL { get; set; }
        public string Appointment_No { get; set; } = string.Empty;
    }
    public class Select2DTO
    {
        public string id { get; set; } = "";
        public string text { get; set; } = "";

    }
    public partial class ProductRequest
    {
        public short Sub_Category_Id { get; set; }
        public string Like { get; set; } = "";
        public int Page_No { get; set; }
        public long Occasion_Id { get; set; }

    }
    public class AppProducts
    {
        public long Total_Items { get; set; }
        public List<SM_Products> Items { get; set; }
        public AppProducts()
        {
            Items = new List<SM_Products>();

        }
    }
    public partial class ProductDetailResponse
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
        public string Product_Name { get; set; } = "";
        public string Product_Desc { get; set; } = "";
        public string Short_Desc { get; set; } = "";
        public string Nutritional_Facts { get; set; } = "";
        public decimal Price { get; set; }
        public string Image_Url { get; set; } = "";
        public decimal Avg_Rating { get; set; }
        public int Total_Ratings { get; set; }
        public bool Is_Favorite { get; set; }
        public bool Is_Exclusive { get; set; }
        public bool Is_Catering { get; set; }
        public List<AddOnDTO> AddOns { get; set; }
        public List<CateringCategoryDTO> Catering_Categories { get; set; }

        public ProductDetailResponse()
        {
            AddOns = new List<AddOnDTO>();
            Catering_Categories = new List<CateringCategoryDTO>();
        }
    }
    public class AddOnDTO
    {
        public long AddOn_Id { get; set; }
        public string AddOn_Name { get; set; } = "";
        public string AddOn_Desc { get; set; } = "";
        public int AddOn_Type { get; set; }
        public List<AddOnOptionDTO> Options { get; set; }
        public AddOnDTO()
        {
            Options = new List<AddOnOptionDTO>();
        }
    }
    public class AddOnOptionDTO
    {
        public long Option_Id { get; set; }
        public string Option_Name { get; set; } = "";
        public decimal Price { get; set; }

    }
    public class CateringCategoryDTO
    {
        public long Category_Id { get; set; }
        public string Category_Name { get; set; } = "";
        public int Qty_To_Choose { get; set; }
        public List<CateringOptionDTO> Options { get; set; }
        public CateringCategoryDTO()
        {
            Options = new List<CateringOptionDTO>();
        }
    }
    public class CateringOptionDTO
    {
        public long Cateroing_Product_Id { get; set; }
        public string Option_Name { get; set; } = "";      

    }
    public class CartResponse
    {
        public int Status { get; set; }

        public string Message { get; set; } = "";
        public bool Is_Gift_Items { get; set; }
        public RestaurantDTO Restaurant_Details { get; set; }
        public List<CartDTO> CartItems { get; set; }

        public CartResponse()
        {
            Restaurant_Details = new RestaurantDTO();
            CartItems = new List<CartDTO>();
        }

    }
    public class CartDTO
    {
        public long Cart_Id { get; set; }
        public long Product_Id { get; set; }
        public string Product_Name { get; set; } = "";
        public string Image_Url { get; set; } = "";
        public int? Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public string Comments { get; set; } = "";
        public bool Is_Currently_Available { get; set; }
      
        public List<ProductAddOnDTO> Product_AddOns { get; set; }
        public List<CartCateringProductsDTO> Catering_Products { get; set; }

        public CartDTO()
        {
           
            Product_AddOns = new List<ProductAddOnDTO>();
            Catering_Products = new List<CartCateringProductsDTO>();
        }

    }
    public class ProductAddOnDTO
    {
        public long Product_AddOn_Id { get; set; }      
        public string Product_AddOn_Name { get; set; } = "";
        public decimal Product_AddOn_Price { get; set; }
    }
    public class CartCateringProductsDTO
    {
        public long Catering_Product_Id { get; set; }
        public string? Catering_Product { get; set; } = string.Empty;
        public int Qty { get; set; }
    }
    public class RestaurantDTO
    {
        public long Restaurant_Id { get; set; }
        public string Restaurant_Name { get; set; } = "";
        public decimal Delivery_Charges { get; set; }
        public List<BranchDTO> Branches { get; set; }

        public RestaurantDTO()
        {
            Branches = new List<BranchDTO>();
        }

    }
    public class BranchDTO
    {
        public long Branch_Id { get; set; }
        public string Branch_Name { get; set; } = "";
        public string Branch_Address { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Opening_Time { get; set; } = "";
        public string Closing_Time { get; set; } = "";
    }
}
