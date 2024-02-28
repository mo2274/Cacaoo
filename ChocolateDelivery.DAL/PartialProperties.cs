using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace ChocolateDelivery.DAL
{
    internal class PartialProperties
    {
    }
    public partial class SM_USERS
    {
        [NotMapped]
        public string Group_Name_En { get; set; } = "";
        [NotMapped]
        public string Group_Name_Ar { get; set; } = "";
    }
    
    
    public partial class TXN_APPOINTMENTS {
        [NotMapped]
        public string Cust_Name { get; set; } = "";
        [NotMapped]
        public string Cust_Mobile { get; set; } = "";
        [NotMapped]
        public string Appointment_Time { get; set; } = "";

    }
    public partial class SM_LISTS
    {
        [NotMapped]
        public DataTable? Report_Data { get; set; }
        [NotMapped]
        public List<SM_LIST_FIELDS> Report_Fields { get; set; }

        public SM_LISTS()
        {
            Report_Fields = new List<SM_LIST_FIELDS>();
        }

    }
   
    public partial class SM_CUSTOMER_MEASUREMENT_DETAILS
    {
        [NotMapped]
        public string Item_Name { get; set; } = "";      
    }
    public partial class SM_USER_GROUPS
    {
        [NotMapped]
        public List<SM_USER_GROUP_RIGHTS> SM_USER_GROUP_RIGHTS { get; set; }

        public SM_USER_GROUPS()
        {
            SM_USER_GROUP_RIGHTS = new List<SM_USER_GROUP_RIGHTS>();
        }
    }
    public partial class SM_USER_GROUP_RIGHTS
    {
        [NotMapped]
        public string MenuName_En { get; set; } = "";
    }
    public partial class SM_Categories
    {      
        [NotMapped]
        public IFormFile? Image_File { get; set; }
    }
    public partial class SM_Chefs
    {
        [NotMapped]
        public IFormFile? Image_File { get; set; }
        [NotMapped]
        public List<SM_Chef_Products> SM_Chef_Products { get; set; }
        public SM_Chefs()
        {
            SM_Chef_Products = new List<SM_Chef_Products>();
        }
    }
    public partial class SM_Chef_Products {
        [NotMapped]
        public string Product_Name { get; set; } = "";
    }
    public partial class SM_Sub_Categories
    {
        [NotMapped]
        public IFormFile? Image_File { get; set; }

    }
    public partial class SM_Restaurants
    {
        [NotMapped]
        public IFormFile? Image_File { get; set; }
        [NotMapped]
        public string? Opening_Time_String { get; set; } = "";
        [NotMapped]
        public string? Closing_Time_String { get; set; } = "";
        [NotMapped]
        public string Categories { get; set; } = "";
    }
    public partial class SM_Restaurant_Branches
    {
        [NotMapped]
        public string Restaurant_Name { get; set; } = "";

        [NotMapped]
        public string Opening_Time_String { get; set; } = "";
        [NotMapped]
        public string Closing_Time_String { get; set; } = "";
    }
    public partial class SM_Brands
    {
        [NotMapped]
        public IFormFile? Image_File { get; set; }
        [NotMapped]
        public string Delivery_Time { get; set; } = "";
        [NotMapped]
        public decimal Delivery_Charge { get; set; }
        [NotMapped]
        public string Categories { get; set; } = "";
    }
    public partial class SM_Products
    {
        [NotMapped]
        public IFormFile? Image_File { get; set; }
        [NotMapped]
        public long Category_Id { get; set; }
        [NotMapped]
        public string? Brand_Name_E { get; set; }
        [NotMapped]
        public string? Brand_Name_A { get; set; }
        [NotMapped]
        public bool Is_Favorite { get; set; }
        [NotMapped]
        public bool Is_Gift_Product { get; set; }
        [NotMapped]
        public string[] Occasion_Ids { get; set; }
        [NotMapped]
        public List<SM_Product_AddOns> SM_Product_AddOns { get; set; }
        [NotMapped]
        public List<SM_Product_Branches> SM_Product_Branches { get; set; }
        [NotMapped]
        public List<SM_Product_Catering_Products> SM_Product_Catering_Products { get; set; }
        public SM_Products()
        {
            Occasion_Ids = new string[0];
            SM_Product_AddOns = new List<SM_Product_AddOns>();
            SM_Product_Branches = new List<SM_Product_Branches>();
            SM_Product_Catering_Products = new List<SM_Product_Catering_Products>();


        }
    }
    public partial class SM_Restaurant_AddOns
    {
        [NotMapped]
        public IFormFile? Image_File { get; set; }
      
    }
    public partial class SM_Product_AddOns
    {
        [NotMapped]
        public string AddOn_Name { get; set; } = "";
        [NotMapped]
        public string AddOn_Type { get; set; } = "";

    }
    public partial class SM_Product_Catering_Products {
        [NotMapped]
        public string Category_Name { get; set; } = "";
        [NotMapped]
        public string Product_Name { get; set; } = "";
    }
    public partial class SM_Home_Groups
    {
       
        [NotMapped]
        public List<SM_Home_Group_Details> SM_Home_Group_Details { get; set; }

        public SM_Home_Groups()
        {
            SM_Home_Group_Details = new List<SM_Home_Group_Details>();
           
        }
    }
    public partial class SM_Home_Group_Details
    {
        [NotMapped]
        public string Item_Name { get; set; } = "";
        [NotMapped]
        public string Group_Type_Name { get; set; } = "";    

    }
    public partial class TXN_Orders
    {
        [NotMapped]
        public string Status_Name { get; set; } = "";
        [NotMapped]
        public string Payment_Type_Name { get; set; } = "";
        [NotMapped]
        public string Delivery_Type_Name { get; set; } = "";
        [NotMapped]
        public string Full_Address { get; set; } = "";
        [NotMapped]
        public decimal User_Address_Latitude { get; set; }
        [NotMapped]
        public decimal User_Address_Longitude { get; set; }
        [NotMapped]
        public string Branch_Address { get; set; } = "";
        [NotMapped]
        public decimal Branch_Latitude { get; set; } 
        [NotMapped]
        public decimal Branch_Longitude { get; set; } 
        [NotMapped]
        public string Driver_Name { get; set; } = string.Empty;
        [NotMapped]
        public string Restaurant_Name { get; set; } = string.Empty;
        [NotMapped]
        public string Restaurant_Mobile { get; set; } = string.Empty;
        [NotMapped]
        public decimal Driver_Latitude { get; set; }
        [NotMapped]
        public decimal Driver_Longitude { get; set; }
        [NotMapped]
        public string Order_Type_Name { get; set; } = "";

        [NotMapped]
        public List<TXN_Order_Details> TXN_Order_Details { get; set; }
        public TXN_Orders()
        {
            TXN_Order_Details = new List<TXN_Order_Details>();
        }

    }
    public partial class TXN_Order_Details
    {
        [NotMapped]
        public decimal AddOn_Amount { get; set; }
        [NotMapped]
        public string Full_Product_Name { get; set; } = string.Empty;

        [NotMapped]
        public List<TXN_Order_Detail_AddOns> TXN_Order_Detail_AddOns { get; set; }
        public TXN_Order_Details()
        {
            TXN_Order_Detail_AddOns = new List<TXN_Order_Detail_AddOns>();
        }

    }
    public partial class TXN_Order_Detail_AddOns
    {
        [NotMapped]
        public string AddOn_Name { get; set; } = "";
    }
    public partial class App_User_Address {
        [NotMapped]
        public string Area_Name { get; set; } = "";
    }
    public partial class SM_Occasions
    {
        [NotMapped]
        public IFormFile? Image_File { get; set; }
    }
    public partial class SM_Product_Branches
    {
        [NotMapped]
        public string Branch_Name { get; set; } = "";
    }
    public partial class NOTIFICATION_INBOX {
        [NotMapped]
        public string Title { get; set; } = "";
        [NotMapped]
        public string Desc { get; set; } = "";
        [NotMapped]
        public DateTime Created_Datetime { get; set; }
    }
    public partial class TXN_Order_Detail_Catering_Products
    {
        [NotMapped]
        public string Product_Name { get; set; } = "";
    }
}
