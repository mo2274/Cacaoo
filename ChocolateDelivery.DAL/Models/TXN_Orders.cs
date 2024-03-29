﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChocolateDelivery.DAL;

public class TXN_Orders
{
    [Key]
    public long Order_Id { get; set; }
    public long Order_No { get; set; }
    public string Order_Serial { get; set; } = string.Empty;
    public long App_User_Id { get; set; }
    public long? Address_Id { get; set; }
    public decimal Gross_Amount { get; set; }
    public decimal Discount_Amount { get; set; }
    public decimal Net_Amount { get; set; }
    public DateTime Order_Datetime { get; set; }
    public short Status_Id { get; set; }
    public string Cust_Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Mobile { get; set; } = string.Empty;
    public short Payment_Type_Id { get; set; }
    public decimal Delivery_Charges { get; set; }
    public string? Promo_Code { get; set; } = string.Empty;
    public string? Comments { get; set; } = string.Empty;
    public short Delivery_Type { get; set; }
    public short Channel_Id { get; set; }
    public long? Branch_Id { get; set; }
    public long Restaurant_Id { get; set; }
    public Guid Row_Id { get; set; }
    public long? Driver_Id { get; set; }
    public string? Delivery_Image { get; set; } = string.Empty;
    public short Order_Type { get; set; }
    public string? Gift_Msg { get; set; } = string.Empty;
    public string? Recepient_Name { get; set; } = string.Empty;
    public string? Recepient_Mobile { get; set; } = string.Empty;
    public string? Video_File_Path { get; set; } = string.Empty;
    public string? Video_Link { get; set; } = string.Empty;
    public bool Show_Sender_Name { get; set; }
    public decimal Redeem_Points { get; set; }
    public decimal Redeem_Amount { get; set; }
    public DateTime? Pickup_Datetime { get; set; }
    public string? Cancelled_Reason { get; set; } = string.Empty;
    public string? Rejected_Reason { get; set; } = string.Empty;
    public string? Staff_Note { get; set; } = string.Empty;
    public string? Pickup_Image { get; set; } = string.Empty;
    
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
    public List<TXN_Order_Details> TXN_Order_Details { get; set; } = new();
}