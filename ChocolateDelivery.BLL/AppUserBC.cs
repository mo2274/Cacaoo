using ChocolateDelivery.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class AppUserBC
    {
        private ChocolateDeliveryEntities context;

        public AppUserBC(ChocolateDeliveryEntities chocolateDeliveryEntities)
        {
            context = chocolateDeliveryEntities;
        }
        public App_Users? GetAppUser(string email_id, short login_type)
        {
            var userMenu = new App_Users();
            try
            {
                userMenu = (from o in context.app_users
                            where o.Email == email_id && o.Login_Type == login_type
                            select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public App_Users? GetAppUser(long app_user_id)
        {
            var userMenu = new App_Users();
            try
            {
                userMenu = (from o in context.app_users
                            where o.App_User_Id == app_user_id
                            select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public App_Users CreateAppUser(App_Users PromoterDM)
        {
            try
            {
                if (PromoterDM.App_User_Id != 0)
                {
                    var Promoter = (from o in context.app_users
                                    where o.App_User_Id == PromoterDM.App_User_Id
                                    select o).FirstOrDefault();

                    if (Promoter != null)
                    {
                        Promoter.Name = PromoterDM.Name;
                        Promoter.Password = PromoterDM.Password;
                        //uncommenting below line so we can update mobile as we are using email also from 29/07/2023
                        Promoter.Mobile = PromoterDM.Mobile;  //Mobile number is unique. It cannot be updated.                      
                        Promoter.Login_Type = PromoterDM.Login_Type;
                        Promoter.Facebook_Id = PromoterDM.Facebook_Id;
                        Promoter.Google_Id = PromoterDM.Google_Id;
                        Promoter.Apple_Id = PromoterDM.Apple_Id;

                        Promoter.Show = PromoterDM.Show;
                        Promoter.Plate_Num = PromoterDM.Plate_Num;
                        context.SaveChanges();
                    }
                }
                else
                {
                    context.app_users.Add(PromoterDM);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }


            return PromoterDM;
        }

        public App_Users? ValidateAppUser(string email, string password)
        {
            var userId = new App_Users();
            try
            {
                userId = (from o in context.app_users
                          where o.Email.ToUpper() == email.ToUpper() && o.Password == password
                          select o).FirstOrDefault();


            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userId;
        }

        public App_Users? GetAppUserByRowId(Guid row_id)
        {
            var userMenu = new App_Users();
            try
            {
                userMenu = (from o in context.app_users
                            where o.Row_Id == row_id
                            select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        #region User Address

        public App_User_Address CreateAppUserAddress(App_User_Address PromoterDM)
        {
            try
            {
                if (PromoterDM.Address_Id != 0)
                {
                    var Promoter = (from o in context.app_user_address
                                    where o.Address_Id == PromoterDM.Address_Id
                                    select o).FirstOrDefault();

                    if (Promoter != null)
                    {
                        Promoter.Address_Name = PromoterDM.Address_Name;
                        Promoter.Block = PromoterDM.Block;
                        Promoter.Street = PromoterDM.Street;
                        Promoter.Building = PromoterDM.Building;
                        Promoter.Avenue = PromoterDM.Avenue;
                        Promoter.Floor = PromoterDM.Floor;
                        Promoter.Apartment = PromoterDM.Apartment;
                        Promoter.Area_Id = PromoterDM.Area_Id;
                        Promoter.Mobile = PromoterDM.Mobile;
                        Promoter.Extra_Direction = PromoterDM.Extra_Direction;
                        Promoter.House_No = PromoterDM.House_No;
                        Promoter.Latitude = PromoterDM.Latitude;
                        Promoter.Longitude = PromoterDM.Longitude;
                        Promoter.Paci_Number = PromoterDM.Paci_Number;
                        Promoter.Updated_Datetime = PromoterDM.Updated_Datetime;
                        context.SaveChanges();
                    }
                }
                else
                {
                    context.app_user_address.Add(PromoterDM);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }


            return PromoterDM;
        }

        public App_User_Address? GetUserAddress(long address_id)
        {
            var userMenu = new App_User_Address();
            try
            {
                var query = (from o in context.app_user_address
                             from a in context.sm_areas
                            where o.Address_Id == address_id && o.Area_Id == a.Area_Id
                            select new { o,a}).FirstOrDefault();

                if (query != null) {
                    userMenu = query.o;
                    userMenu.Area_Name = query.a.Area_Name_E;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public List<App_User_Address> GetUserAddresses(long app_user_id,string lang = "E")
        {
            var userMenu = new List<App_User_Address>();
            try
            {
                var query = (from o in context.app_user_address
                            join a in context.sm_areas on o.Area_Id equals a.Area_Id into area
                            from a in area.DefaultIfEmpty()
                            where !o.Is_Deleted && o.App_User_Id == app_user_id
                            select new { o, a }).ToList();

                foreach (var address in query) {
                    var addressDM = address.o;
                    if (address.a != null) {
                        addressDM.Area_Name = lang == "A" ? address.a.Area_Name_A ?? address.a.Area_Name_E : address.a.Area_Name_E;
                    }
                    userMenu.Add(addressDM);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public bool DeleteUserAddress(long userAddressId)
        {
            try
            {

                var user = (from o in context.app_user_address
                            where o.Address_Id == userAddressId
                            select o).FirstOrDefault();

                if (user != null)
                {
                    user.Is_Deleted = true;
                    user.Deleted_Datetime = StaticMethods.GetKuwaitTime();
                    context.SaveChanges();

                    return true;
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        #endregion
    }
}
