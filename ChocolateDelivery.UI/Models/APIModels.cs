using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.UI.Models;

public class RegisterDeviceDTO
{
    public string UniqueDeviceId { get; set; } = "";
    public string NotificationToken { get; set; } = "";
    public string ClientKey { get; set; } = "";
    public int DeviceType { get; set; }
    public string? Preferred_Language { get; set; }
    public short? AppType { get; set; }
}
public class RegisterDeviceResponse
{
    public int Status { get; set; }

    public string Message { get; set; } = "";

}
public class LabelResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public List<LabelDTO> Labels { get; set; }
    public LabelResponse()
    {
        Labels = new List<LabelDTO>();
    }
}
public class LabelDTO
{
    public long Label_Id { get; set; }
    public string Label_Code { get; set; } = "";
    public string Label_Name_E { get; set; } = "";
    public string Label_Name_A { get; set; } = "";

}
public class InvoiceResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public decimal Net_Amount { get; set; }
    public decimal Paid_Amount { get; set; }
    public decimal Remaining_Amount { get; set; }
    public List<ReceiptDTO> Receipts { get; set; }
    public List<ItemDetailDTO> Details { get; set; }
    public List<DocumentDTO> Documents { get; set; }
    public InvoiceResponse()
    {
        Receipts = new List<ReceiptDTO>();
        Details = new List<ItemDetailDTO>();
        Documents = new List<DocumentDTO>();
    }

}
public class ItemDetailDTO
{
    public long Item_Id { get; set; }
    public string Item_Name { get; set; } = "";
    public string Item_Status { get; set; } = "";
    public string Item_Desc { get; set; } = "";
}
public class DocumentDTO
{
    public string Document_Name { get; set; } = "";
    public string Document_Path { get; set; } = "";
}
public class ReceiptDTO
{
    public long Receipt_Id { get; set; }
    public string Receipt_Date { get; set; } = "";
    public decimal Receipt_Amount { get; set; }
    public string? Reference_No { get; set; } = "";
}

public class CustomerInvoicesResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public List<Select2DTO> results { get; set; }
    public CustomerInvoicesResponse()
    {
        results = new List<Select2DTO>();

    }
}

public class AppointmentResponse
{
    public int Status { get; set; }

    public string Message { get; set; } = "";
    public string Loc_Start_Time { get; set; } = "";
    public string Loc_End_Time { get; set; } = "";

    //public List<ResourceDTO> Resources { get; set; }
    public TelerikResourceDTO resources { get; set; }
    public List<EventDTO> Events { get; set; }
    public List<Select2DTO> select2data { get; set; }

    public AppointmentResponse()
    {
        //Resources = new List<ResourceDTO>();
        resources = new TelerikResourceDTO();
        Events = new List<EventDTO>();
        select2data = new List<Select2DTO>();
    }

}

public class ResourceDTO
{
    public long id { get; set; }
    public string title { get; set; } = "";
    public string eventColor { get; set; } = "";
}

public class TelerikResourceDTO
{
    public string name { get; set; } = "";
    public string field { get; set; } = "";
    public string title { get; set; } = "";
    public List<DataSourceDTO> dataSource { get; set; }

    public TelerikResourceDTO()
    {
        dataSource = new List<DataSourceDTO>();
    }
}

public class DataSourceDTO
{
    public int value { get; set; }
    public string text { get; set; } = "";
    public string color { get; set; } = "";
}

public class EventDTO
{

    public long id { get; set; }
    public int resourceId { get; set; }
    public string title { get; set; } = "";
    public DateTime start { get; set; }
    public DateTime end { get; set; }
    public bool allDay { get; set; }
    public string backgroundColor { get; set; } = "";
    public string url { get; set; } = "";
    public long appointmentid { get; set; }
}

public class TimeFrameResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public List<TimeFrameDTO> TimeFrames { get; set; }
    public TimeFrameResponse()
    {
        TimeFrames = new List<TimeFrameDTO>();
    }
}
public class TimeFrameDTO
{
    public long TimeFrame_Id { get; set; }
    public string TimeFrame_Name { get; set; } = "";

}
public class InvoiceMediaRequest
{
    public string Session_Id { get; set; } = "";
    [FromForm(Name = "Files")]
    public List<IFormFile>? Files { get; set; }
}
public class InvoiceMediaResponse
{
    public bool success { get; set; }

}
public class DocumentDeleteRequest
{
    public long Document_Id { get; set; }
    public int Deleted_By { get; set; }

}
public class DeleteDocumentResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";

}
public class AppointmentDetailResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public long Appointment_Detail_Id { get; set; }
    public long Appointment_Id { get; set; }
    public int Staff_Id { get; set; }
    public long Service_Id { get; set; }

    public string Services_Name { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string Customer_Name { get; set; } = "";
    public long Customer_Id { get; set; }
    public int Appointment_Status_Id { get; set; }

    public string Area { get; set; } = "";
    public string Remarks { get; set; } = "";
    public string Appointment_Created { get; set; } = "";
    public string Appointment_Date { get; set; } = "";
    public string Appointment_No { get; set; } = "";
    public int Appointment_Type_Id { get; set; }
    public int Designer_Id { get; set; }
    public string Design_Type { get; set; } = "";
    public string Design_Place { get; set; } = "";
}
public class UnreadAppointmentResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public int Unread_Count { get; set; }
    public List<TXN_APPOINTMENTS> Appointments { get; set; } = new();
}
    
public class TXN_APPOINTMENTS {
    [NotMapped]
    public string Cust_Name { get; set; } = "";
    [NotMapped]
    public string Cust_Mobile { get; set; } = "";
    [NotMapped]
    public string Appointment_Time { get; set; } = "";

}
    
public class MeasurementResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public string Measurement_Name { get; set; } = string.Empty;
    public string Measurement_Content { get; set; } = string.Empty;
    public string Measurement_Date { get; set; } = "";
    public string Comments { get; set; } = "";
    public List<ItemDetailDTO> Details { get; set; }
    public List<DocumentDTO> Documents { get; set; }
    public MeasurementResponse()
    {

        Details = new List<ItemDetailDTO>();
        Documents = new List<DocumentDTO>();
    }
}


public class ProductOrderResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public int Order_Id { get; set; }
}
public class CategoryResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public List<CategoryDTO> Categories { get; set; }
    public CategoryResponse()
    {
        Categories = new List<CategoryDTO>();
    }
}
public class CategoryDTO
{
    public long Category_Id { get; set; }
    public string Category_Name { get; set; } = "";
    public string Image_URL { get; set; } = "";
    public string Background_Color { get; set; } = "";
    public List<SubCategoryDTO> SubCategories { get; set; }
    public CategoryDTO()
    {
        SubCategories = new List<SubCategoryDTO>();
    }
}
public class SubCategoryDTO
{
    public long Sub_Category_Id { get; set; }
    public string Sub_Category_Name { get; set; } = "";
    public string Image_URL { get; set; } = "";
    public string Background_Color { get; set; } = "";
}
public class SubCategoryResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public List<SubCategoryDTO> SubCategories { get; set; }
    public SubCategoryResponse()
    {
        SubCategories = new List<SubCategoryDTO>();
    }
}
public class HomePageResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<CarouselDTO> Carousels { get; set; }
    public List<GroupDTO> Groups { get; set; }
    public HomePageResponse()
    {
        Carousels = new List<CarouselDTO>();
        Groups = new List<GroupDTO>();
    }
}
public class CarouselDTO
{

    public long Carousel_Id { get; set; }
    public string Carousel_Name { get; set; } = string.Empty;
    public string Carousel_Title { get; set; } = string.Empty;
    public string Media_Url { get; set; } = string.Empty;
    public string ThumbNail_Url { get; set; } = string.Empty;
    public short Media_Type { get; set; }
    public short Media_From_Type { get; set; }
    public long Redirect_Id { get; set; }
}
public class GroupDTO
{
    public long Group_Id { get; set; }
    public string Group_Name { get; set; } = string.Empty;

    public int Display_Type { get; set; }
    public List<GeneralDTO> GroupItems { get; set; }
    public GroupDTO()
    {
        GroupItems = new List<GeneralDTO>();
    }

}
public class GeneralDTO
{

    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image_Url { get; set; } = string.Empty;

    public short Group_Type_Id { get; set; }
}
public class ProductsResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public long Total_Products { get; set; }
    public List<ProductDTO> Products { get; set; }
    public ProductsResponse()
    {
        Products = new List<ProductDTO>();
    }
}
public class ProductDTO
{

    public long Product_Id { get; set; }
    public string Product_Name { get; set; } = "";
    public string Product_Desc { get; set; } = "";
    public string Image_Url { get; set; } = "";
    public string Brand_Name { get; set; } = "";
    public decimal Price { get; set; }
    public bool Is_Exclusive { get; set; }
    public bool Is_Catering { get; set; }
    public long? Brand_id { get; set; }
}

public class BrandsResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public List<BrandDTO> Brands { get; set; }
    public BrandsResponse()
    {
        Brands = new List<BrandDTO>();
    }
}
public class BrandDTO
{

    public long Brand_Id { get; set; }
    public string Brand_Name { get; set; } = "";
    public string Image_Url { get; set; } = "";
    public string Delivery_Time { get; set; } = "";
    public decimal Delivery_Charge { get; set; }
    public string Categories { get; set; } = "";
    public string Background_Color { get; set; } = "";

}
public class BrandDetailResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public string Brand_Name { get; set; } = "";
    public string Brand_Categories { get; set; } = "";
    public string Brand_Desc { get; set; } = "";
    public string Image_Url { get; set; } = "";
    public string Delivery_Time { get; set; } = "";
    public decimal Delivery_Charge { get; set; }
    public List<BrandCategoryDTO> Categories { get; set; }

    public BrandDetailResponse()
    {
        Categories = new List<BrandCategoryDTO>();
    }
}

public class BrandCategoryDTO
{
    public long Category_Id { get; set; }
    public string Category_Name { get; set; } = "";
    public List<ProductDTO> Products { get; set; }
    public BrandCategoryDTO()
    {
        Products = new List<ProductDTO>();
    }
}
public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;  
    public string Mobile { get; set; } = string.Empty;
    public string Image_Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public short Login_Type { get; set; }
    public string Facebook_Id { get; set; } = string.Empty;
    public string Google_Id { get; set; } = string.Empty;
    public string Apple_Id { get; set; } = string.Empty;

}
public class RegisterResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string App_User_Id { get; set; } = string.Empty;
    public string App_User_Name { get; set; } = string.Empty;

}
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class GeneralResponse
{
    public int Status { get; set; }

    public string Message { get; set; } = string.Empty;

}
public class CartRequest
{
    public long Cart_Id { get; set; }      
    public string App_User_Id { get; set; } = string.Empty;
    public long Product_Id { get; set; }
    public int Qty { get; set; }
    public string? Comments { get; set; } = string.Empty;
    public List<long> Product_AddOnIds { get; set; }
    public List<CartCateringProductsDTO> Catering_Products { get; set; }
    public CartRequest() {
        Product_AddOnIds = new List<long>();
        Catering_Products = new List<CartCateringProductsDTO>();
    }

}
   
public class AddCartResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public long Cart_Id { get; set; }

}
public class ProfileResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;    
    public string Email { get; set; } = string.Empty;      
    public short Login_Type { get; set; }
    public decimal Redeem_Points { get; set; }
    public string Plate_Num { get; set; } = string.Empty;

}
public class UpdatePofileRequest
{
    public string App_User_Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
      
}
public class AreaResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AreaDTO> Areas { get; set; }
    public AreaResponse()
    {
        Areas = new List<AreaDTO>();
    }
}
public class AreaDTO
{

    public long Area_Id { get; set; }
    public string Area_Name { get; set; } = string.Empty;
    public decimal Delivery_Charge { get; set; }


}
public class UpdateAddressRequest
{

    public long Address_Id { get; set; }

    public string App_User_Id { get; set; } = string.Empty;

    public string Address_Name { get; set; } = string.Empty;

    public string Block { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string House_No { get; set; } = string.Empty;

    public int Area_Id { get; set; }       

    public string Extra_Direction { get; set; } = string.Empty;

    public string Avenue { get; set; } = string.Empty;

    public string Building { get; set; } = string.Empty;

    public string Floor { get; set; } = string.Empty;

    public string Apartment { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Paci_Number { get; set; } = "";
}
public class UpdateAddressResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public long Address_Id { get; set; }
}
public class UserAddressResponse
{

    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;

    public List<AddressDTO> UserAddresses { get; set; }

    public UserAddressResponse()
    {
        UserAddresses = new List<AddressDTO>();
    }

}
public class AddressDTO
{

    public long Address_Id { get; set; }     

    public string Address_Name { get; set; } = string.Empty;

    public string Block { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string House_No { get; set; } = string.Empty;

    public int Area_Id { get; set; }
    public string Area_Name { get; set; } = string.Empty;
    public string Extra_Direction { get; set; } = string.Empty;

    public string Avenue { get; set; } = string.Empty;

    public string Building { get; set; } = string.Empty;

    public string Floor { get; set; } = string.Empty;

    public string Apartment { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Paci_Number { get; set; } = "";
}

public class OrderResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public string Payment_Link { get; set; } = string.Empty;

}

public class OrderRequest
{       
    public string Remarks { get; set; } = string.Empty;
    public string App_User_Id { get; set; } = string.Empty;
    public short Payment_Type_Id { get; set; }                   
    public long? Address_Id { get; set; }
    public string Cust_Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Mobile { get; set; } = string.Empty;
    public short Delivery_Type { get; set; }     
    public short Channel_Id { get; set; }            
    public string Promo_Code { get; set; } = string.Empty;
    public long Branch_Id { get; set; }
    public string? Gift_Msg { get; set; } = string.Empty;
    public string? Recepient_Name { get; set; } = string.Empty;
    public string? Recepient_Mobile { get; set; } = string.Empty;
    public string? Video_File { get; set; } = string.Empty;
    public string? Video_Link { get; set; } = string.Empty;
    public bool? Show_Sender_Name { get; set; }
    public decimal? Redeem_Points { get; set; } = decimal.Zero;
    public decimal? Redeem_Amount { get; set; } = decimal.Zero;
    public string? Pickup_Datetime { get; set; }
    public List<OrderDetailDTO> OrderDetails { get; set; }

    public OrderRequest()
    {
        OrderDetails = new List<OrderDetailDTO>();

    }
}

public class OrderDetailDTO
{
    public long Prod_Id { get; set; }
    public string Prod_Name { get; set; } = string.Empty;
    public int Qty { get; set; }           
    public string Promo_Code { get; set; } = string.Empty;
    public List<long> Product_AddOn_Ids { get; set; }           
    public string Remarks { get; set; } = string.Empty;     
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal Gross_Amount { get; set; } // gross amt is sum of amount + addons amount
    public List<CartCateringProductsDTO> Catering_Products { get; set; }
    public OrderDetailDTO()
    {
        Product_AddOn_Ids = new List<long>();
        Catering_Products = new List<CartCateringProductsDTO>();
    }
}
public class DriverOrderResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal Cash_Collected { get; set; } = decimal.Zero;
    public int Total_Orders { get; set; }
    public List<DriverOrderDTO> Orders { get; set; }

    public DriverOrderResponse()
    {
        Orders = new List<DriverOrderDTO>();

    }
}
    
public class DriverOrderRequest
{
    public long Order_Id { get; set; }
    public string App_User_Id { get; set; } = string.Empty;
    public int Status_Id { get; set; } // only accept order - 11 , and decline order - 12 will be passed
    public string? Delivery_Image { get; set; }
    public string? Comments { get; set; } = string.Empty;

}
public class GetDriverOrdersRequest
{
    public string App_User_Id { get; set; } = string.Empty;

    public string From_Date { get; set; } = string.Empty;
    public string To_Date { get; set; } = string.Empty;

}
public class OrderDetailResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public long Order_Id { get; set; }
    public string Order_No { get; set; } = string.Empty;
    public AddressCoordinatesDTO Pickup_Address { get; set; }
    public AddressCoordinatesDTO Delivery_Address { get; set; }
    public string Order_Date { get; set; } = string.Empty;      
    public string Cust_Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Delivery_Option { get; set; } = string.Empty;
    public string Payment_Type { get; set; } = string.Empty;
    public decimal Sub_Total { get; set; }
    public decimal Delivery_Charges { get; set; }
    public decimal Total { get; set; }
    public int Order_Status_Id { get; set; }
    public string Order_Status { get; set; } = string.Empty;
    public string Driver_Name { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string Restaurant_Name { get; set; } = string.Empty;
    public string Restaurant_Mobile { get; set; } = string.Empty;
    public decimal Current_Driver_Latitude { get; set; }
    public decimal Current_Driver_Longitude { get; set; }
    public List<DriverOrderDetailDTO> OrderDetails { get; set; }

    public OrderDetailResponse()
    {
        OrderDetails = new List<DriverOrderDetailDTO>();
        Pickup_Address = new AddressCoordinatesDTO();
        Delivery_Address = new AddressCoordinatesDTO();

    }
}

public class DriverOrderDetailDTO
{
    public long Order_Detail_Id { get; set; }
    public string Prod_Name { get; set; } = string.Empty;
    public int Qty { get; set; }             
    public decimal Rate { get; set; }      
    public decimal Gross_Amount { get; set; } 
    public decimal AddOn_Amount { get; set; }
    public decimal Net_Amount { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public List<CartCateringProductsDTO> Catering_Products { get; set; }
    public DriverOrderDetailDTO() {
        Catering_Products = new List<CartCateringProductsDTO>();
    }
}
public class AddressCoordinatesDTO
{        
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

}
public class PaymentTypeResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<PaymentTypeDTO> PaymentTypes { get; set; }

    public PaymentTypeResponse()
    {
        PaymentTypes = new List<PaymentTypeDTO>();

    }
}
public class PaymentTypeDTO
{
    public int Type_Id { get; set; }
    public string Type_Name { get; set; } = string.Empty;
    public string Icon { get; set; }
}
public class OrderPaymentResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public long Order_Id { get; set; }
    public decimal Order_Amount { get; set; }
    public string Tap_Id { get; set; } = string.Empty;
    public string Payment_Id { get; set; } = string.Empty;
    public string Trans_Id { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string Reference_No { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Payment_Mode { get; set; } = string.Empty;
    public string Payment_Date { get; set; } = string.Empty;
}

public class FavoriteRequest {
    public long Product_Id { get; set; }
    public string App_User_Id { get; set; } = string.Empty;
}
public class ChefDetailResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public string Chef_Name { get; set; } = "";
    public string Chef_Desc { get; set; } = "";
    public string Image_Url { get; set; } = "";
    public List<ProductDTO> Products { get; set; }
    public ChefDetailResponse()
    {
        Products = new List<ProductDTO>();
    }
}
public class CacaooMapResponse {
    public int Status { get; set; }
    public string Message { get; set; } = "";
    public List<MapDTO> Branches { get; set; }
    public CacaooMapResponse()
    {
        Branches = new List<MapDTO>();
    }
}
public class MapDTO {
    public string Restaurant_Name { get; set; } = "";
    public string Branch_Name { get; set; } = "";
    public string Branch_Address { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

}
public class RatingRequest
{
    public long Order_Detail_Id { get; set; }
    public int Rating { get; set; }      

}
public class TrackingRequest
{
    public long Order_Id { get; set; }
    public short Status_Id { get; set; }
    public string Driver_Id { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

}
public class TrackingResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = "";       
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

}
public class SettingResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<SettingDTO> Settings { get; set; }

    public SettingResponse()
    {
        Settings = new List<SettingDTO>();

    }
}
public class SettingDTO
{
    public int Setting_Id { get; set; }
    public string Setting_Name { get; set; } = string.Empty;
    public string Setting_Value { get; set; } = string.Empty;

}
public class UpdatePasswordRequest
{
    public string App_User_Id { get; set; } = string.Empty;
    public string Old_Password { get; set; } = string.Empty;
    public string New_Password { get; set; } = string.Empty;
       

}
public class NotificationDTO
{
    public long Notification_Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;


}
public class NotificationResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<NotificationDTO> Notifications { get; set; }

    public NotificationResponse()
    {
        Notifications = new List<NotificationDTO>();

    }
}