using ChocolateDelivery.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class RestaurantBC
    {
        private ChocolateDeliveryEntities context;

        public RestaurantBC(ChocolateDeliveryEntities benayaatEntities)
        {
            context = benayaatEntities;

        }

        public SM_Restaurants CreateRestaurant(SM_Restaurants restaurantDM)
        {
            try
            {
                var query = (from o in context.sm_restaurants
                             where o.Restaurant_Id == restaurantDM.Restaurant_Id
                             select o).FirstOrDefault();

                if (query != null)
                {
                    query.Restaurant_Name_E = restaurantDM.Restaurant_Name_E;
                    query.Restaurant_Name_A = restaurantDM.Restaurant_Name_A;
                    query.Restaurant_Desc_E = restaurantDM.Restaurant_Desc_E;
                    query.Restaurant_Desc_A = restaurantDM.Restaurant_Desc_A;
                    query.Restaurant_Address_E = restaurantDM.Restaurant_Address_E;
                    query.Restaurant_Address_A = restaurantDM.Restaurant_Address_A;
                    query.Show = restaurantDM.Show;
                    query.Sequence = restaurantDM.Sequence;
                    query.Updated_By = restaurantDM.Updated_By;
                    query.Updated_Datetime = restaurantDM.Updated_Datetime;
                    if (!string.IsNullOrEmpty(restaurantDM.Image_URL))
                    {
                        query.Image_URL = restaurantDM.Image_URL;
                    }
                    query.Opening_Time = restaurantDM.Opening_Time;
                    query.Closing_Time = restaurantDM.Closing_Time;
                    query.Commission = restaurantDM.Commission;
                    query.Latitude = restaurantDM.Latitude;
                    query.Longitude = restaurantDM.Longitude;
                    query.Comments = restaurantDM.Comments;
                    query.Username = restaurantDM.Username;
                    query.Password = restaurantDM.Password;
                    query.Delivery_Time = restaurantDM.Delivery_Time;
                    query.Delivery_Charge = restaurantDM.Delivery_Charge;
                    query.Email = restaurantDM.Email;
                    query.Mobile = restaurantDM.Mobile;
                    query.Background_Color = restaurantDM.Background_Color;
                    query.RestaurantStatus = restaurantDM.RestaurantStatus;
                }
                else
                {
                    context.sm_restaurants.Add(restaurantDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return restaurantDM;
        }

        public SM_Restaurants? GetRestaurant(long restaurant_id)
        {
            var area = new SM_Restaurants();
            var currentTime = StaticMethods.GetKuwaitTime().TimeOfDay;

            try
            {
                area = (from o in context.sm_restaurants
                        where o.Restaurant_Id == restaurant_id
                        select o).FirstOrDefault();
                if (area != null)
                {
                    if (area.Opening_Time != null)
                    {
                        DateTime time = StaticMethods.GetKuwaitTime().Date.Add((TimeSpan)area.Opening_Time);
                        area.Opening_Time_String = time.ToString("hh:mm tt");
                    }
                    if (area.Closing_Time != null)
                    {
                        DateTime time = StaticMethods.GetKuwaitTime().Date.Add((TimeSpan)area.Closing_Time);
                        area.Closing_Time_String = time.ToString("hh:mm tt");
                    }

                    if (area.Opening_Time != null && area.Closing_Time != null && currentTime >= area.Opening_Time.Value && currentTime <= area.Closing_Time.Value)
                    {
                        area.RestaurantStatus = DAL.Models.Enums.ResturantStatus.Available;
                    }
                    else
                    {
                        area.RestaurantStatus = DAL.Models.Enums.ResturantStatus.Closed;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return area;
        }

        public SM_Restaurants? ValidateMerchant(string userName, string password)
        {
            var userId = new SM_Restaurants();
            try
            {
                userId = (from o in context.sm_restaurants
                          where o.Username == userName && o.Password == password
                          select o).FirstOrDefault();


            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userId;
        }

        public List<SM_Restaurant_AddOns> GetAllRestaurantAddOns(long restaurant_id)
        {

            var customer = new List<SM_Restaurant_AddOns>();
            try
            {
                customer = (from o in context.sm_restaurant_addons
                            where o.Restaurant_Id == restaurant_id
                            select o).ToList();


            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }

    }
}
