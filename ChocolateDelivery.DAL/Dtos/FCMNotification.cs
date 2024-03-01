namespace ChocolateDelivery.DAL;

/*public class Notification
{
    public string body { get; set; }
    public string title { get; set; }
    public int badge { get; set; }
    public string sound { get; set; }
    public int content_available { get; set; }
    public bool mutable_content { get; set; }
    public string image { get; set; }
}

public class Data
{

    public string message { get; set; }
    public string flex1 { get; set; } // this denote promotion_id
    public string flex2 { get; set; }// this denote product_id
    public string flex3 { get; set; }// this denote service_id
    public string flex4 { get; set; }// this denote booking order id
    public string imageurl { get; set; }
    public string contentTitle { get; set; }
}

public class FCMNotification
{
    public Notification notification { get; set; }
    public Data data { get; set; }
    public string to { get; set; }
}

class FCMNotificationToken
{
    public Notification notification { get; set; }
    public Data data { get; set; }
    //public string to { get; set; }
    public List<string> registration_ids { get; set; }
    public FCMNotificationToken()
    {
        registration_ids = new List<string>();
    }
}*/
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Android
{
    public Notification notification { get; set; } = new();
}

public class Apns
{
    public Payload payload { get; set; }
    public Apns() {
        payload = new Payload();
    }
}

public class Aps
{
    public string category { get; set; } = "NEW_MESSAGE_CATEGORY";
}

public class Data
{
    public string story_id { get; set; } = string.Empty;
}

public class Message
{
    public string topic { get; set; } = "driver";
    public Notification notification { get; set; }
    public Data data { get; set; }
    public Android android { get; set; }
    public Apns apns { get; set; }
    public Message() {
        notification = new Notification();
        data = new Data();
        android = new Android();
        apns = new Apns();
    }
}

public class Notification
{
    public string title { get; set; } = string.Empty;
    public string body { get; set; } = string.Empty;
    public string click_action { get; set; } = "FLUTTER_NOTIFICATION_CLICK";
}

public class Payload
{
    public Aps aps { get; set; }
    public Payload() {
        aps = new Aps();
    }
}

public class FCMNotification
{
    public Message message { get; set; }
    public FCMNotification() {
        message = new Message();
    }
}


public class AccessTokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public string token_type { get; set; } = string.Empty;
}