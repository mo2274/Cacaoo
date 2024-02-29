using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.CustomFilters;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MySqlConnector;
using Newtonsoft.Json;
using System.Data;

namespace ChocolateDelivery.UI.Controllers
{
    public class KnetController : Controller
    {
        private readonly ChocolateDeliveryEntities _context;
        private readonly IConfiguration _config;
        private readonly string _logPath = "";
        private readonly OrderBC _orderBc;
        public KnetController(ChocolateDeliveryEntities cc, IConfiguration config)
        {
            _context = cc;
            _config = config;
            _logPath = _config.GetValue<string>("ErrorFilePath"); // "Information"
            _orderBc = new OrderBC(_context);

        }
        public IActionResult PaymentResponse()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Request.Query["tap_id"]))
                {
                    #region Tap 

                    string? tapId = null;

                    if (!string.IsNullOrWhiteSpace(Request.Query["tap_id"]))
                    {
                        tapId = Request.Query["tap_id"];
                    }

                    if (string.IsNullOrWhiteSpace(tapId))
                    {
                        return NotFound();
                    }

                    else
                    {

                        var response = TapPayment.GetChargeRequest(tapId, _config);

                        int resCount = 0;

                        while (resCount <= 10)
                        {
                            if (response == null)
                                response = TapPayment.GetChargeRequest(tapId, _config);
                            else
                                break;

                            resCount++;
                        }
                        ViewBag.Cust_Name = "";
                        ViewBag.Cust_Mobile = "";
                        ViewBag.LabelType = "Invoice #";
                        ViewBag.TypeNo = "";
                        if (response != null)
                        {

                            string trackId = response.id;
                            globalCls.WriteToFile(_logPath, JsonConvert.SerializeObject(response));
                            var paymentDm = _orderBc.GetPaymentByTrackId(trackId);
                            if (paymentDm != null && string.IsNullOrEmpty(paymentDm.Payment_Id))
                            {
                                if (response.reference != null)
                                {
                                    paymentDm.Reference_No = response.reference.acquirer;
                                    paymentDm.Payment_Id = response.reference.payment;
                                    paymentDm.Auth = response.reference.gateway;
                                    paymentDm.Trans_Id = response.reference.track;
                                }
                                if (response.source != null)
                                {
                                    paymentDm.Payment_Mode = response.source.payment_method;
                                }
                                paymentDm.Result = response.status;
                                paymentDm.Updated_Datetime = StaticMethods.GetKuwaitTime();
                                _orderBc.CreatePayment(paymentDm);


                                if (paymentDm.Order_Id != null && paymentDm.Order_Id != 0)
                                {
                                    var orderDm = _orderBc.GetOrder((long)paymentDm.Order_Id);
                                    if (orderDm != null)
                                    {

                                        ViewBag.TypeNo = orderDm.Order_Serial;
                                        ViewBag.Cust_Name = orderDm.Cust_Name;
                                        ViewBag.Cust_Mobile = orderDm.Mobile;
                                    }
                                    if (response.status != null && (response.status.ToUpper() == "CAPTURED" || response.status.ToUpper() == "SUCCESS"))
                                    {
                                        orderDm.Status_Id = OrderStatus.ORDER_PAID;
                                        _orderBc.SaveOrder(orderDm);
                                        SendOrderEmail(orderDm.Order_Id);

                                        #region clear cart after successful order
                                        CartBC cartBc = new CartBC(_context);
                                        var removeCart = cartBc.RemoveCart(orderDm.App_User_Id);
                                        #endregion

                                        NotificationBC notificationBc = new NotificationBC(_context, _logPath);
                                        notificationBc.SendNotificationToDriver(orderDm.Order_Serial);

                                        APP_PUSH_CAMPAIGN campaignDm = new APP_PUSH_CAMPAIGN();
                                        campaignDm.Title_E = "Pick up Request";
                                        campaignDm.Desc_E = "Please accept Order # " + orderDm.Order_Serial + " for delivery";
                                        campaignDm.Title_A = "Pick up Request";
                                        campaignDm.Desc_A = "Please accept Order # " + orderDm.Order_Serial + " for delivery";
                                        campaignDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        notificationBc.CreatePushCampaign(campaignDm);

                                        string connectionString = _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                                        using (MySqlConnection con = new MySqlConnection(connectionString))
                                        {
                                            con.Open();
                                            var time = con.ConnectionTimeout;
                                            using (MySqlCommand cmd = new MySqlCommand("InsertNotifications", con))
                                            {
                                                using (var da = new MySqlDataAdapter(cmd))
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.Parameters.AddWithValue("@Campaign_Id", campaignDm.Campaign_Id);
                                                    cmd.ExecuteReader();

                                                }

                                            }
                                        }
                                    }

                                }
                                
                            }
                            return View(paymentDm);
                        }
                        else
                        {

                            return NotFound();
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(_logPath, ex.ToString(), true);
            }
            return View();
        }

        public bool SendOrderEmail(long orderId)
        {
            bool bSuccess = false;
            try
            {

                var orderBc = new OrderBC(_context);
                RestaurantBC restaurantBc = new RestaurantBC(_context);
                decimal grossAmount = Decimal.Zero;

                var order = orderBc.GetOrder(Convert.ToInt32(orderId));
                var orderDetails = orderBc.GetOrderDetails(Convert.ToInt32(orderId));


                if (order != null)
                {
                    string bodyMessage = "";

                    var siteConfiguration = orderBc.GetSiteConfiguration(Email_Templates.COD_EMAIL_MESSAGE);

                    bodyMessage = siteConfiguration.Config_Value;
                    bodyMessage = bodyMessage.Replace("[CUST_NAME]", order.Cust_Name);
                    bodyMessage = bodyMessage.Replace("[ORDER_NO]", order.Order_Serial);
                    bodyMessage = bodyMessage.Replace("[ORDER_DATE]", Convert.ToDateTime(order.Order_Datetime).ToString("dd/MM/yy hh:mm tt"));
                    bodyMessage = bodyMessage.Replace("[TEL_NO]", order.Mobile);
                    bodyMessage = bodyMessage.Replace("[PAYMENT]", order.Payment_Type_Name);
                    if (order.Payment_Type_Id == PaymentTypes.Cash)
                    {
                        bodyMessage = bodyMessage.Replace("[KNET_DETAILS]", "");
                    }
                    bodyMessage = bodyMessage.Replace("[EARNED_POINTS]", "0");

                    bodyMessage = bodyMessage.Replace("[ADDRESS]", order.Full_Address);

                    string substring = "";

                    foreach (var orderdetail in order.TXN_Order_Details)
                    {


                        string col1 = " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" + orderdetail.Full_Product_Name + "<o:p></o:p></span></p></td>";
                        string col2 = " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" + Convert.ToDecimal(orderdetail.Rate).ToString("N3") + "<o:p></o:p></span></p></td>";

                        string col5 = " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" + orderdetail.Qty.ToString() + "<o:p></o:p></span></p></td>";
                        string col6 = " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" + Convert.ToDecimal(orderdetail.Gross_Amount).ToString("N3") + "<o:p></o:p></span></p></td>";
                        substring += " <tr>" + col1 + col2 + /*col3 +*/ /*col4 +*/ col5 + col6 + "</tr> ";
                        grossAmount += (decimal)orderdetail.Gross_Amount;
                    }


                    var netAmount = grossAmount;
                    decimal deliveryCharge = decimal.Zero;


                    deliveryCharge = order.Delivery_Charges;
                    netAmount += order.Delivery_Charges;
                    //substring += "</tr><tr><td><div style='text-align:right'>DELIVERY CHARGES :</div></td><td></td><td></td><td>" + "KWD" + "        &nbsp;" + Convert.ToDecimal(order.DELIVERY_CHARGES).ToString("N3") + "     </td></tr>";


                    //decimal redeemAmt = Decimal.Zero;
                    bodyMessage = bodyMessage.Replace("[REDEEM_LINE]", "");
                    // decimal walletAmt = Decimal.Zero;
                    bodyMessage = bodyMessage.Replace("[WALLET_LINE]", "");
                    //decimal tipAmt = Decimal.Zero;
                    bodyMessage = bodyMessage.Replace("[TIP_LINE]", "");


                    bodyMessage = bodyMessage.Replace("[ORDER_DETAIL]", substring);
                    bodyMessage = bodyMessage.Replace("[GROSS_AMOUNT]", grossAmount.ToString("N3"));
                    bodyMessage = bodyMessage.Replace("[DELIVERY_CHARGE]", deliveryCharge.ToString("N3"));
                    bodyMessage = bodyMessage.Replace("[NET_AMOUNT]", netAmount.ToString("N3"));

                    var subject = siteConfiguration.Subject;
                    subject = subject.Replace("[LOCATION_NAME]", "");
                    subject = subject.Replace("[DELIVERY_OPTION]", order.Delivery_Type_Name);
                    subject = subject.Replace("[orderId]", order.Order_Serial);
                    subject = subject.Replace("[CustomerName]", order.Cust_Name);

                    var channel = "APP";
                    if (order.Channel_Id == 1)
                    {
                        channel = "WEB";
                    }
                    subject = subject.Replace("[CHANNEL]", channel);



                    var restaurantDm = restaurantBc.GetRestaurant(order.Restaurant_Id);
                    if (restaurantDm != null && !string.IsNullOrEmpty(restaurantDm.Email))
                    {
                        siteConfiguration.BCC_Email = siteConfiguration.BCC_Email.Replace("[RESTAURANT_EMAIL]", restaurantDm.Email);
                    }
                    bSuccess = GetSendHtmlMail(order.Email, subject, bodyMessage, siteConfiguration.CC_Email ?? "", siteConfiguration.BCC_Email ?? "", siteConfiguration.From_Email ?? "", siteConfiguration.Password ?? "");
                    var emailMsg = "";
                    if (bSuccess)
                    {
                        emailMsg = "Email sent successfully for Order Id:" + orderId;
                    }
                    else
                    {
                        emailMsg = "Email not sent successfully for Order Id:" + orderId;
                    }

                    globalCls.WriteToFile(_logPath, emailMsg);
                }
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(_logPath, ex.ToString());
            }



            return bSuccess;
        }

        public bool GetSendHtmlMail(string to, string subject, string body, string cc, string bcc, string fromEmail, string password, string receiverName = "")
        {
            try
            {
                var server = _config.GetValue<string>("MailSettings:Server");
                var port = _config.GetValue<int>("MailSettings:Port");
                var senderName = _config.GetValue<string>("MailSettings:SenderName");

                using (MimeMessage emailMessage = new MimeMessage())
                {
                    MailboxAddress emailFrom = new MailboxAddress(senderName, fromEmail);
                    emailMessage.From.Add(emailFrom);

                    MailboxAddress emailTo = new MailboxAddress(receiverName, to);
                    emailMessage.To.Add(emailTo);

                    if (!string.IsNullOrEmpty(cc))
                        emailMessage.Cc.Add(new MailboxAddress("Cc Receiver", cc));
                    if (!string.IsNullOrEmpty(bcc))
                        emailMessage.Bcc.Add(new MailboxAddress("Bcc Receiver", bcc));

                    emailMessage.Subject = subject;


                    BodyBuilder emailBodyBuilder = new BodyBuilder();
                    emailBodyBuilder.HtmlBody = body;
                    emailBodyBuilder.TextBody = "Plain Text goes here to avoid marked as spam for some email servers.";

                    emailMessage.Body = emailBodyBuilder.ToMessageBody();

                    using (SmtpClient mailClient = new SmtpClient())
                    {
                        mailClient.Connect(server, port, true);
                        mailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                        mailClient.Authenticate(fromEmail, password);
                        mailClient.Send(emailMessage);
                        mailClient.Disconnect(true);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Exception Details
                globalCls.WriteToFile(_logPath, ex.ToString());
                return false;
            }

        }
    }
}
