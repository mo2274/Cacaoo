using ChocolateDelivery.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class DeviceBC
    {
        private ChocolateDeliveryEntities context;
        private string logPath;

        public DeviceBC(ChocolateDeliveryEntities benayaatEntities, string errorLogPath = "")
        {
            context = benayaatEntities;
            logPath = errorLogPath;

        }
        public Device_Registration RegisterDevice(Device_Registration menu)
        {
            try
            {
                if (!string.IsNullOrEmpty(menu.Device_Id))
                {
                    var query = (from o in context.device_registration
                                 where o.Device_Id.Equals(menu.Device_Id)
                                 select o).FirstOrDefault();

                    if (query != null)
                    {
                        if (menu.App_User_Id != null)
                            query.App_User_Id = menu.App_User_Id;
                        if (menu.Logged_In != null)
                            query.Logged_In = menu.Logged_In;
                        if (menu.Client_Key != null)
                            query.Client_Key = menu.Client_Key;
                        if (menu.Device_Type != null)
                            query.Device_Type = menu.Device_Type;
                        if (menu.Notification_Token != null)
                            query.Notification_Token = menu.Notification_Token;
                        if (menu.Notification_Enabled != null)
                            query.Notification_Enabled = menu.Notification_Enabled;
                        if (menu.NotificationSound_Enabled != null)
                            query.NotificationSound_Enabled = menu.NotificationSound_Enabled;
                        if (menu.Preferred_Language != null)
                            query.Preferred_Language = menu.Preferred_Language;
                        if (!string.IsNullOrEmpty(menu.Code))
                        {
                            query.Code = menu.Code;
                        }
                        context.SaveChanges();
                    }
                    else
                    {
                        context.device_registration.Add(menu);
                        context.SaveChanges();
                    }
                }
                else
                {
                    context.device_registration.Add(menu);
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return menu;

        }

        public Device_Registration Logout(Device_Registration menu)
        {

            try
            {

                if (!string.IsNullOrEmpty(menu.Device_Id))
                {
                    var query = (from o in context.device_registration
                                 where o.Device_Id.ToLower() == menu.Device_Id.ToLower()
                                 select o).FirstOrDefault();

                    if (query != null)
                    {

                        query.App_User_Id = menu.App_User_Id;
                        query.Mobile = menu.Mobile;
                        query.Code = menu.Code;
                        query.Logged_In = menu.Logged_In;

                        context.SaveChanges();
                    }
                    else
                    {
                        context.device_registration.Add(menu);
                        context.SaveChanges();
                    }
                }

                else
                {
                    context.device_registration.Add(menu);
                    context.SaveChanges();
                }

            }
            
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }


            return menu;

        }

        public Device_Registration? GetDevice(string uniquedeviceid)
        {
            var query = (from o in context.device_registration

                         where o.Device_Id == uniquedeviceid
                         select o).FirstOrDefault();

            return query;
        }

        public Device_Registration? GetDeviceByClientKey(string clientkey)
        {
            var query = (from o in context.device_registration
                         where o.Client_Key == clientkey
                         select o).FirstOrDefault();

            return query;
        }

        public List<Device_Registration> GetTotalDownloads(DateTime FromDate, DateTime ToDate)
        {

            var query = new List<Device_Registration>();
            try
            {
                query = (from o in context.device_registration
                         where o.Created_Datetime>= FromDate && o.Created_Datetime <= ToDate
                         && o.App_Type == AppTypes.CLIENT
                         select o).ToList();
                

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return query;
        }

        public List<Device_Registration> GetTotalMonthlyDownloads(DateTime FromDate, DateTime ToDate)
        {

            var query = new List<Device_Registration>();
            try
            {
                var firstDayOfMonthFromDate = new DateTime(FromDate.Year, FromDate.Month, 1);
                var firstDayOfMonthLastDate = new DateTime(ToDate.Year, ToDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonthLastDate.AddMonths(1).AddSeconds(-1);

                query = (from o in context.device_registration
                         where o.Created_Datetime >= firstDayOfMonthFromDate && o.Created_Datetime <= lastDayOfMonth
                         && o.App_Type == AppTypes.CLIENT
                         select o).ToList();


            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return query;
        }
    }
}
