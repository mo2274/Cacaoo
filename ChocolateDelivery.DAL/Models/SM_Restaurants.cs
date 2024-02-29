using ChocolateDelivery.DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_Restaurants
    {
        [Key] public long Restaurant_Id { get; set; }
        public string Restaurant_Name_E { get; set; } = string.Empty;
        public string? Restaurant_Name_A { get; set; } = string.Empty;
        public string? Restaurant_Desc_E { get; set; } = string.Empty;
        public string? Restaurant_Desc_A { get; set; } = string.Empty;
        public string? Restaurant_Address_E { get; set; } = string.Empty;
        public string? Restaurant_Address_A { get; set; } = string.Empty;
        public string? Image_URL { get; set; } = string.Empty;
        public string? Logo { get; set; } = string.Empty;
        public decimal Commission { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool Show { get; set; }
        public int Sequence { get; set; } = 1;
        public string? Comments { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public TimeSpan? Opening_Time { get; set; }
        public TimeSpan? Closing_Time { get; set; }
        public int? Created_By { get; set; }
        public DateTime? Created_Datetime { get; set; }
        public int? Updated_By { get; set; }
        public DateTime? Updated_Datetime { get; set; }
        public decimal Ratings { get; set; } = decimal.Zero;
        public Guid Row_Id { get; set; }
        public string? Delivery_Time { get; set; } = string.Empty;
        public decimal Delivery_Charge { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? Mobile { get; set; } = string.Empty;
        public string? Background_Color { get; set; } = string.Empty;
        public ResturantStatus? RestaurantStatus { get; set; }
    }
}