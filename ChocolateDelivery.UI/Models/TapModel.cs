namespace ChocolateDelivery.UI.Models;


public class Reference
{
    public string track { get; set; }
    public string payment { get; set; }
    public string gateway { get; set; }
    public string acquirer { get; set; }
    public string transaction { get; set; }
    public string order { get; set; }
}


public class TapChargeRequest
{
    public decimal amount { get; set; }
    public string currency { get; set; } = string.Empty;
    public bool threeDSecure { get; set; } = true;
    public bool save_card { get; set; }
    public string description { get; set; } = string.Empty;
    public string statement_descriptor { get; set; } = string.Empty;
    public Reference reference { get; set; }
    public Receipt receipt { get; set; }
    public TapCustomer customer { get; set; }
    public Source source { get; set; }
    public Redirect redirect { get; set; }
}

public class TapCustomer
{
    public string id { get; set; } = string.Empty;
    public string first_name { get; set; } = string.Empty;
    public string last_name { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public Phone phone { get; set; }
    public string currency { get; set; } = string.Empty;
}
public class Phone
{
    public int country_code { get; set; }
    public int number { get; set; }
}

public class Source
{

    public string type { get; set; } = string.Empty;
    public string payment_type { get; set; } = string.Empty;
    public string payment_method { get; set; } = string.Empty;
    public string channel { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
}
public class Receipt
{
    public bool email { get; set; }
    public bool sms { get; set; }
}

public class Redirect
{
    public string url { get; set; } = string.Empty;
}

public class TapChargeResponse
{
    public string id { get; set; }
    public bool live_mode { get; set; }
    public string api_version { get; set; }
    public string method { get; set; }
    public string status { get; set; }
    public decimal amount { get; set; }
    public string currency { get; set; }
    public bool threeDSecure { get; set; }
    public bool card_threeDSecure { get; set; }
    public bool save_card { get; set; }
    public string merchant_id { get; set; }
    public string product { get; set; }
    public string statement_descriptor { get; set; }
    public string description { get; set; }
    public Transaction transaction { get; set; }
    public Reference reference { get; set; }
    public Response response { get; set; }
    public Receipt receipt { get; set; }
    public TapCustomer customer { get; set; }
    public Merchant merchant { get; set; }
    public Source source { get; set; }
    public Redirect redirect { get; set; }
}
public class Transaction
{
    public string timezone { get; set; }
    public string created { get; set; }
    public string url { get; set; }
    public bool asynchronous { get; set; }
    public decimal amount { get; set; }
    public string currency { get; set; }
}
public class Merchant
{
    public string id { get; set; }
}
public class Response
{
    public string code { get; set; }
    public string message { get; set; }
}