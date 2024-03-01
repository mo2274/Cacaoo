using RestSharp;
using System.Net;

namespace ChocolateDelivery.BLL;

public class BaseClient
{
    public RestResponse GetAccessToken(string apiBaseUrl, string apiSubUrl, string jsonDataString)
    {

        var client = new RestClient(apiBaseUrl);
        var request = new RestRequest(apiSubUrl, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddHeader("Content-Type", "application/json");
        //request.AddHeader("authorization", "key=" + apiKey);
        request.AddParameter("application/json", jsonDataString, ParameterType.RequestBody);
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        var response = client.Execute(request);

        return response;
    }
    public RestResponse sendFCMNotification(string apiBaseUrl,string apiSubUrl, string accessToken, string jsonDataString)
    {

        var client = new RestClient(apiBaseUrl);
        var request = new RestRequest(apiSubUrl, Method.Post)
        {
            RequestFormat = DataFormat.Json
        };
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("authorization", "Bearer " + accessToken);
        request.AddParameter("application/json", jsonDataString, ParameterType.RequestBody);
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        var response = client.Execute(request);

        return response;
    }
}