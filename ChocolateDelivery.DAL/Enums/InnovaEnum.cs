namespace ChocolateDelivery.DAL;

public static class List_Fields_Types
{
    public const string ZInt = "N";
    public const string ZString = "S";
    public const string ZDecimal = "F";
    public const string ZDatetime = "D";
    public const string ZImage = "I";
}


public static class List_Fields_Group_Types
{
    public const short Normal_Field = 1;
    public const short Sum_Field = 2;
    public const short Max_Field = 3;

}
public static class Language
{
    public const string English = "en-US";
    public const string Arabic = "ar-KW";

}
public static class Preferred_Language
{
    public const int English = 1;
    public const int Arabic = 2;

}
public static class Form_Element_Types
{
    public const string Text = "text";
    public const string Number = "number";
    public const string MultiLine = "multiline";
    public const string Password = "password";
    public const string Color = "color";
}
public static class Field_Language_Types
{
    public const string Undefined = "U";
    public const string English = "E";
    public const string Arabic = "A";
}
public static class SettingNames
{

    public const string Points_Per_KD = "Points_Per_KD";
    public const string Fils_Per_Point = "Fils_Per_Point";
    public const string Min_Point_For_Redemption = "Min_Point_For_Redemption";
    public const string Redeem_Lap = "Redeem_Lap";

}
public static class Advertisement_Types
{
    public const int Banners = 1;
    public const int Advertisement = 2;
}
public class DisplayTypes
{
    public const int Small_Horizontal_List = 1;
    public const int Square_Plus_Horizontal_List = 2;
    public const int Right_Horizontal_List = 3;
    public const int Left_Horizontal_List = 4;
    public const int Rectangle_Horizontal_List = 5;
    public const int Normal_Horizontal_List = 6;
    public const int Square_Title_Horizontal_List = 7;
}
public class PaymentMethods
{
    public const int Monthly = 1;
    public const int Quaterly = 2;
    public const int Yearly = 3;

}
public static class Media_From
{
    public const int Advertisement = 1;
    public const int Property = 2;
}

public static class PostStatus
{
    public const int Active = 1;
    public const int Inactive = 2;
    public const int Processing_Payment = 3;
    public const int Payment_Failed = 4;
}

public class SP_DB_Types
{
    public const string Int = "int";
    public const string BigInt = "bigint";
    public const string SmallInt = "smallint";
    public const string Datetime = "datetime";
    public const string Varchar = "varchar";
    public const string NVarchar = "nvarchar";
    public const string Decimal = "decimal";
}
public class Parameter_Types
{
    public const string TextBox = "T";
    public const string DropDownList = "D";
    public const string ListBox = "L";
    public const string Session = "S";
}
public class Parameter_Default_Values
{
    public const string FirstOfMonth = "FirstOfMonth";
    public const string LastOfMonth = "LastOfMonth";

}

public class Value_Types
{
    public const string String = "S";
    public const string DateTime = "D";
    public const string Number = "N";

}
public class Dropdown_Types
{
    public const short Normal_Dropdown = 1;
    public const short Select2_Dropdown = 2;

}
public class Date_Formats
{
    public const string MySqlDatetimeFormat = "yyyy-MM-dd HH:mm:ss";

}
public static class List_Render_Types
{
    public const short Table = 1;
    public const short Donut = 2;
    public const short Barchart = 3;

}
public static class Ledger_Types
{
    public const short Customer = 1;
    public const short Supplier = 2;

}
public static class TXN_Types
{
    public const short PURCHASE_INVOICE = 101;
    public const short PURCHASE_REQUEST = 102;
    public const short SALES_INVOICE = 201;
}
public static class Invoice_Status
{
    public const short SAVE = 1;
    public const short CONFIRM = 2;//After confirm invoice will be displayed to designer
    public const short FIRST_PAYMENT_RECEIVED = 3;//After designed invoice will be displayed to production user
    public const short WAITING_FOR_DESIGN = 4;
    public const short DESIGN_COMPLETED = 5;
    public const short IN_PRODUCTION = 6;
    public const short PRODUCTION_COMPLETED = 7;
    public const short READY_TO_DELIVER = 8;
    public const short DELIVERED = 9;
    public const short COMPLETED = 10;
}
public static class Appointment_Status
{

    public const int BOOKED = 1;
    public const int CANCELLED = 2;

}
public static class SMS_Codes
{
    public const int Measurement_Confirm = 1;
    public const int Invoice_Generated = 2;
    public const int Book_Design = 3;
    public const int Design_Completed = 4;
    public const int Second_Payment = 5;
    public const int Production_Completed = 6;
    public const int Project_Delivered = 7;
    public const int Third_payment = 8;
    public const int Request_First_Payment = 9;
    public const int Request_Second_Payment = 10;
    public const int Design_Appointment = 11;
    public const int Delivery_Appointment = 12;
    public const int Forgot_Password = 13;
}
public static class ParameterNames
{

    public const string Show_Reference_No = "Show_Reference_No";
    public const string Show_Ledger = "Show_Ledger";
    public const string Show_Payment_Terms = "Show_Payment_Terms";
    public const string Show_Estimated_Delivery = "Show_Estimated_Delivery";
    public const string Show_Item_Div = "Show_Item_Div";
    public const string Show_Measurement = "Show_Measurement";
    public const string Show_Document_Upload = "Show_Document_Upload";
    public const string Send_Confirmed_SMS = "Send_Confirmed_SMS";
    public const string Send_Confirmed_Email = "Send_Confirmed_Email";
    public const string Show_Designer = "Show_Designer";

}
public static class Item_Status
{
    public const short UNDER_PRODUCTION = 1;
    public const short COMPLETED = 2;
}
public class Chef_Types
{
    public const short CHEF = 1;
    public const short BOUTIQUE = 2;

}
public class AddOn_Types
{
    public const short OPTIONAL = 1;
    public const short MANDATORY = 2;

}
public static class GroupType
{
    public const short CATEGORIES = 1;
    public const short SUB_CATEGORIES = 2;
    public const short CACAOO_CHEF = 3;
    public const short CACAOO_BOUTIQUE = 4;
    public const short PRODUCTS = 5;
    public const short RESTAURANTS = 6;
    public const short OCCASIONS = 7;
}
public class Login_Types
{
    public const short EMAIL = 1;
    public const short GOOGLE = 2;
    public const short FACEBOOK = 3;
    public const short APPLE = 4;
}
public class App_User_Types
{
    public const short APP_USER = 1;
    public const short DRIVER = 2;

}
public class Delivery_Types
{
    public const short DELIVERY = 1;
    public const short PICKUP = 2;

}
public static class PaymentTypes
{
    public const int Cash = 1;
    public const int KNET = 2;
    public const int CreditCard = 3;
    public const int ApplePay = 7;
}
public static class OrderStatus
{
    public const int ORDER_RECEIVED = 1;
    public const int ORDER_PROCESSING_PAYMENT = 2;
    public const int ORDER_PAID = 3;
    public const int ORDER_FAILED = 4;
    public const int ORDER_PREPARING = 5;
    public const int ORDER_DELIVERED = 6;
    public const int ORDER_REJECTED = 7;
    public const int ORDER_RETURN_REQUEST = 8;
    public const int ORDER_RETURNED = 9;
    public const int ORDER_CANCELLED_BY_USER = 10;
    public const int OUT_FOR_DELIVERY = 11;
    public const int ACCEPTED_BY_DRIVER = 12;
    public const int DECLINED_BY_DRIVER = 13;
    public const int CONFIRMED_BY_RESTAURANT = 14;
    public const int CONFIRMED_BY_CUSTOMER = 15;
    public const int NOT_DELIVERED = 16;
}
public static class Email_Templates
{
    public const string COD_EMAIL_MESSAGE = "COD_EMAIL_MESSAGE";
      
}
public static class OrderTypes {
    public const short NORMAL = 1;
    public const short GIFT = 2;
}
public class TXN_Point_Types
{
    public const short Add_Points_After_Payment = 1;
    public const short Add_Points_After_Return = 2;
    public const short Deduct_Points_After_Payment = 3;
    public const short Deduct_Points_After_Return = 4;
    public const short Add_Points_For_Sharing_App = 4;
}
public static class AppTypes
{
    public const short CLIENT = 1;
    public const short DRIVER = 2;
}
public static class DashboardOrderTypes
{
    public const short PENDING = 1;
    public const short PREPARING = 2;
    public const short OUT_FOR_DELIVERY = 3;
    public const short DELIVERED = 4;
    public const short SCHEDULED = 5;
    public const short REJECTED = 6;
    public const short CANCELLED = 7;
    public const short NOT_PICKED_30 = 8;
    public const short OFD_30 = 9;
    public const short OFD_60 = 10;
    public const short NO_DRIVERS_10 = 11;
    public const short DUPLICATE = 12;
    public const short EXPENSIVE = 13;
}