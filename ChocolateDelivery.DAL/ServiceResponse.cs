using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public static class ServiceResponse
    {
        public const string Success = "Success";// 0
        public const string ServerError = "Server Error"; // 1
        public const string NoRequestFound = "No Request Found"; // 2
        public const string UserExist = "User Already Exist"; // 3
        public const string InvalidRequestParamters = "InvalidRequestParamters"; // 4
        public const string QtyNotAvailable = "Quantity not available for one or more items"; //102
        public const string NoSecurityKey = "No Security key Provided"; //105
        public const string Unauthorized = "Unauthorized Request"; //106
        public const string NoPasswordChange = "Unable to Change Password"; //107
        public const string NoOpenEvent = "No Current Open Event for this Promoter"; //108
        public const string NoLocationFound = "No Location Found for Promoter"; //109
        public const string NoAppUserFound = "App User Not Found"; //110
        public const string NoFeedFound = "No Feeds Found"; //111
        public const string UserFeedNotDeleted = "User Feed Not Deleted"; //112
        public const string PostNotFound = "Post Not Found"; //113
        public const string UserNotFound = "User Not Found"; //114
        public const string UserFeedAlreadyAdded = "Feed Already added Before"; //115
        public const string FavouriteItemNotFound = "Favourite Item Not Found"; //116
        public const string NoMediaFound = "No Media Found"; //117
        public const string NoNotificationFound = "No Notification Found"; //118
        public const string NoItemFound = "No Item Found"; //119
        public const string NoOrderFound = "No Order Found"; //120
        public const string NoAddressFound = "No Address Found"; //121
        public const string NoDeviceFound = "No Device Found"; //122
        public const string NoEmailFound = "No Email Found"; //123
        public const string NoCountryFound = "Country Not Found"; //115
        public const string NoCartItemFound = "No Cart Item Found"; //124
        public const string NoStaffFound = "No Staff Found"; //125
        public const string NoServiceBooking = "Only Service Booking can be cancelled"; //126
        public const string ServiceDateElapsed = "You cannot cancel the service as booking datetime already passed"; //127
        public const string NoTimeFrameFound = "No TimeFrame Found"; //128
        public const string OrderAlreadyCancelled = "Order Already Cancelled Before"; //129
        public const string RequestedQtyNotAvailable = "Request Quantity {0} not available for this product"; //130
        public const string NoSizeFound = "Size not Found"; //131
        public const string NoAdFound = "Advertisement not Found"; //132
        public const string NoCivilIdFound = "Civil Id not Found"; //133
        public const string NoPropertyUnitFound = "Property Unit not Found"; //134
        public const string NoPaymentFound = "Payment not Found"; //135
        public const string NoInvoiceFound = "Invoice not Found"; //136
        public const string NoDocumentFound = "Document not Found"; //137
        public const string NoMeasurementFound = "Measurement not Found"; //138
        public const string NoInvoiceDetailFound = "Invoice Detail not Found"; //139
        public const string NoProductFound = "No Product Found"; //140
    }
}
