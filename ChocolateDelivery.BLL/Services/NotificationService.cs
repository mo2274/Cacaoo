using ChocolateDelivery.DAL;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ChocolateDelivery.BLL;

public class NotificationService
{
    private readonly AppDbContext _context;
    private readonly string _logPath;
    public NotificationService(AppDbContext benayaatEntities, string errorLogPath)
    {
        _context = benayaatEntities;
        _logPath = errorLogPath;

    }
    public void SendNotificationToDriver(string order_no)
    {
        try
        {
            var baseClient = new BaseClient();
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

                Helpers.WriteToFile(_logPath, "Error Generating Access Token " + accessTokenResponse.Content);
            }
        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(_logPath, ex.ToString());

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
                var message = new Message();
                var notification = new Notification
                {
                    title = title,
                    body = msgText,
                    click_action = "FLUTTER_NOTIFICATION_CLICK"
                };
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
                _ = "/topics/" + Regex.Replace("driver", @"\s+", "");
                //var json = JsonConvert.SerializeObject(fCMNotification);
                var json = "{ \"message\": { \"topic\": \"driver\", \"notification\": { \"title\": \""+title+"\", \"body\": \""+ msgText + "\" }, \"data\": { \"story_id\": \"story_12345\" }, \"android\": { \"notification\": { \"click_action\": \"FLUTTER_NOTIFICATION_CLICK\" } }, \"apns\": { \"payload\": { \"aps\": { \"category\" : \"NEW_MESSAGE_CATEGORY\" } } } } }";
                var apiUrl = "https://fcm.googleapis.com";
                var apiBaseUrl = "/v1/projects/cacaoo-409514/messages:send";
                Helpers.WriteToFile(_logPath, "Notification Payload :" + json);

                var response = baseclient.sendFCMNotification(apiUrl, apiBaseUrl, apiKey, json);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    sendToTopic = true;
                    if (response.Content != null)
                        Helpers.WriteToFile(_logPath, response.Content);
                }
                else
                {
                    sendToTopic = false;
                    Helpers.WriteToFile(_logPath, "Error Sending Message " + response.Content);
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
                var Customer = (from o in _context.app_push_campaign
                    where o.Campaign_Id == CustomerDM.Campaign_Id
                    select o).FirstOrDefault();

                if (Customer != null)
                {

                    _context.SaveChanges();
                }
            }
            else
            {
                _context.app_push_campaign.Add(CustomerDM);
                _context.SaveChanges();
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
            var query = (from o in _context.notification_inbox
                from c in _context.app_push_campaign
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