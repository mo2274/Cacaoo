using ChocolateDelivery.DAL;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class NotificationBC
    {
        private ChocolateDeliveryEntities context;
        private string logPath;
        public NotificationBC(ChocolateDeliveryEntities benayaatEntities, string errorLogPath)
        {
            context = benayaatEntities;
            logPath = errorLogPath;

        }
        public void SendNotificationToDriver(string order_no)
        {
            try
            {
                BaseClient baseClient = new BaseClient();
                var jwt = GoogleOAuthUtility.CreateJwtForFirebaseMessaging();
                var jsonString = "{\"assertion\":\"" + jwt + "\",\"grant_type\":\"urn:ietf:params:oauth:grant-type:jwt-bearer\"}";
                var apiBaseUrl = "https://oauth2.googleapis.com";
                var apiSubUrl = "/token";
                var accessTokenResponse = baseClient.GetAccessToken(apiBaseUrl, apiSubUrl, jsonString);
                if (accessTokenResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (accessTokenResponse.Content != null)
                    {
                        var response = JsonConvert.DeserializeObject<AccessTokenResponse>(accessTokenResponse.Content);
                        if (response != null) {
                            var title = "New Order Received";
                            var msgText = "Please accept Order # " + order_no + " for delivery";
                            SendFCMNotificationToDriver(title, msgText, "", response.access_token);
                        }
                       
                    }
                }
                else
                {

                    globalCls.WriteToFile(logPath, "Error Generating Access Token " + accessTokenResponse.Content);
                }
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString());

            }
        }
        private bool SendFCMNotificationToDriver(string title, string msgText, string imageurl, string apiKey)
        {
            var sendToTopic = false;

            try
            {
                var baseclient = new BaseClient();
                try
                {
                    Message message = new Message();
                    Notification notification = new Notification();
                    notification.title = title;
                    notification.body = msgText;
                    notification.click_action = "FLUTTER_NOTIFICATION_CLICK";
                    message.notification = notification;
                    /*DAL.Notification notification = new DAL.Notification()
                    {
                        title = title,
                        body = msgText,
                        badge = 1,
                        sound = "default",
                        content_available = 1,
                        mutable_content = true,
                        image = imageurl
                    };

                    Data data = new Data()
                    {
                        message = msgText,
                        flex1 = "",
                        flex2 = "",
                        flex3 = "",
                        flex4 = "",
                        imageurl = imageurl,
                        contentTitle = "cacaoo"
                    };*/
                    var topic = "/topics/" + Regex.Replace("driver", @"\s+", "");
                    FCMNotification fCMNotification = new FCMNotification() {message = message };
                    //var json = JsonConvert.SerializeObject(fCMNotification);
                    var json = "{ \"message\": { \"topic\": \"driver\", \"notification\": { \"title\": \""+title+"\", \"body\": \""+ msgText + "\" }, \"data\": { \"story_id\": \"story_12345\" }, \"android\": { \"notification\": { \"click_action\": \"FLUTTER_NOTIFICATION_CLICK\" } }, \"apns\": { \"payload\": { \"aps\": { \"category\" : \"NEW_MESSAGE_CATEGORY\" } } } } }";
                    var apiUrl = "https://fcm.googleapis.com";
                    var apiBaseUrl = "/v1/projects/cacaoo-409514/messages:send";
                    globalCls.WriteToFile(logPath, "Notification Payload :" + json);

                    var response = baseclient.sendFCMNotification(apiUrl, apiBaseUrl, apiKey, json);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        sendToTopic = true;
                        if (response.Content != null)
                            globalCls.WriteToFile(logPath, response.Content);
                    }
                    else
                    {
                        sendToTopic = false;
                        globalCls.WriteToFile(logPath, "Error Sending Message " + response.Content);
                    }
                }
                catch (Exception ex)
                {

                    throw new Exception(ex.ToString());
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());

            }
            return sendToTopic;

        }

        public APP_PUSH_CAMPAIGN CreatePushCampaign(APP_PUSH_CAMPAIGN CustomerDM)
        {
            try
            {
                if (CustomerDM.Campaign_Id != 0)
                {
                    var Customer = (from o in context.app_push_campaign
                                    where o.Campaign_Id == CustomerDM.Campaign_Id
                                    select o).FirstOrDefault();

                    if (Customer != null)
                    {

                        context.SaveChanges();
                    }
                }
                else
                {
                    context.app_push_campaign.Add(CustomerDM);
                    context.SaveChanges();
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }


            return CustomerDM;
        }

        public List<NOTIFICATION_INBOX> GetNotifications(long app_user_id,string lang = "E")
        {

            var customer = new List<NOTIFICATION_INBOX>();
            try
            {
                var query = (from o in context.notification_inbox
                            from c in context.app_push_campaign
                             where o.Campaign_Id == c.Campaign_Id && o.App_User_Id == app_user_id && !o.Is_Deleted
                             orderby c.Created_Datetime descending
                             select new { o, c }).ToList();

                foreach (var detail in query)
                {
                    var detailDM = new NOTIFICATION_INBOX();
                    detailDM = detail.o;
                    detailDM.Title = lang == "A" ? detail.c.Title_A : detail.c.Title_E;
                    detailDM.Desc = lang == "A" ? detail.c.Desc_A ??"" : detail.c.Desc_E??"";
                    detailDM.Created_Datetime = detail.c.Created_Datetime;
                    customer.Add(detailDM);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }
    }
}
