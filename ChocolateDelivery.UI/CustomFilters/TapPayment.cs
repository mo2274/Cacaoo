using ChocolateDelivery.BLL;
using ChocolateDelivery.UI.Models;
using Newtonsoft.Json;
using System.Net;

namespace ChocolateDelivery.UI.CustomFilters
{
    public class TapPayment
    {
      
       
        public static HttpWebResponse GetWebRequestResponse(string url, string method, string? contentType, string? accept, string? authorization, string? postData)
        {
            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.KeepAlive = false;

                if (!string.IsNullOrWhiteSpace(method) && method.Trim().ToLower() == "post")
                {
                    request.Method = "POST";
                }

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    request.ContentType = contentType;
                }

                if (!string.IsNullOrWhiteSpace(accept))
                {
                    request.Accept = accept;
                }

                if (!string.IsNullOrWhiteSpace(authorization))
                {
                    request.Headers.Add("Authorization", authorization);
                }

                if (!string.IsNullOrWhiteSpace(postData))
                {
                    var data = System.Text.Encoding.UTF8.GetBytes(postData);
                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                return request.GetResponse() as HttpWebResponse;
            }
            catch (Exception ex)
            {
                //Utilities.LogException(ex);
                return null;
            }
        }
        public static TapChargeResponse? CreateChargeRequest(TapChargeRequest tapChargeRequest, IConfiguration _config)
        {
            var url = _config.GetValue<string>("TapPayment:APIURL") + "/charges";
            var authorization = string.Format("Bearer {0}", _config.GetValue<string>("TapPayment:SecretKey"));
            var postData = JsonConvert.SerializeObject(tapChargeRequest);

            //var client = new System.Net.WebClient();           
            //client.Headers.Add("Authorization", authorization);
            //client.Headers.Add("Content-Type", "application/json");
            //var JSON_Response = client.UploadString(url, "POST", postData);
            //var response = JsonConvert.DeserializeObject<InvoiceResponseISO>(JSON_Response);
            //return response;
            var logPath = _config.GetValue<string>("ErrorFilePath");
            Helpers.WriteToFile(logPath, "API URL:"+url);
            Helpers.WriteToFile(logPath, "Authorization:" + authorization);
            Helpers.WriteToFile(logPath, "Body:" + postData);
            var response = GetWebRequestResponse(url, "POST", "application/json", null, authorization, postData);
            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                string? response_Str = null;
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    response_Str = reader.ReadToEnd();
                }

                return JsonConvert.DeserializeObject<TapChargeResponse>(response_Str);
            }

            return null;
        }

        public static TapChargeResponse? GetChargeRequest(string charge_id, IConfiguration _config)
        {
            var url = _config.GetValue<string>("TapPayment:APIURL") + "/charges/" + charge_id; ;
            var authorization = string.Format("Bearer {0}", _config.GetValue<string>("TapPayment:SecretKey"));


            //var client = new System.Net.WebClient();           
            //client.Headers.Add("Authorization", authorization);
            //client.Headers.Add("Content-Type", "application/json");
            //var JSON_Response = client.UploadString(url, "POST", postData);
            //var response = JsonConvert.DeserializeObject<InvoiceResponseISO>(JSON_Response);
            //return response;

            var response = GetWebRequestResponse(url, "GET", "application/json", null, authorization, "");
            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                string? response_Str = null;
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    response_Str = reader.ReadToEnd();
                }

                return JsonConvert.DeserializeObject<TapChargeResponse>(response_Str);
            }

            return null;
        }
    }
}
