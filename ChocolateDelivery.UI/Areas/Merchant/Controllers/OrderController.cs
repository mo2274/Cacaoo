using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ChocolateDelivery.UI.Areas.Merchant.Controllers
{
    [Area("Merchant")]
    public class OrderController : Controller
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        OrderBC orderBC;


        public OrderController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            orderBC = new OrderBC(context);


        }
        public IActionResult Update(string Id)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;

                var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                var areaexist = orderBC.GetOrder(decryptedId);
                if (areaexist != null && areaexist.Order_Id != 0)
                {
                    if (areaexist.Order_Type == OrderTypes.GIFT) {
                        var qrText = "https://cacaoo.com/Order.php?OrderId="+ decryptedId;
                        /*if (!string.IsNullOrEmpty(areaexist.Video_Link)) {
                            qrText = areaexist.Video_Link;
                        } else if (!string.IsNullOrEmpty(areaexist.Video_File_Path)) {
                            qrText = areaexist.Video_File_Path;
                        }*/

                        if (!string.IsNullOrEmpty(qrText)) {
                            QRCodeGenerator QrGenerator = new QRCodeGenerator();
                            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                            QRCode QrCode = new QRCode(QrCodeInfo);
                            Bitmap QrBitmap = QrCode.GetGraphic(60);
                            // use this when you want to show your logo in middle of QR Code and change color of qr code
                            //Bitmap logoImage = new Bitmap(@"wwwroot/assets/img/logo/cacaoo_favicon.png");
                            //var qrCodeAsBitmap = QrCode.GetGraphic(60, Color.Black, Color.White, logoImage);
                            byte[] BitmapArray = BitmapToByteArray(QrBitmap);
                            string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
                            ViewBag.QrCodeUri = QrUri;
                        }
                        
                    }
                    return View("Create", areaexist);
                }
                else
                {
                    ModelState.AddModelError("name", "Order not exist");
                }

            }
            catch (Exception ex)
            {
                /* lblError.Visible = true;
                 lblError.Text = "Invalid username or password";*/
                ModelState.AddModelError("name", "Due to some technical error, data not saved");
                globalCls.WriteToFile(logPath, ex.ToString(), true);

            }
            return View("Create");
        }

        [HttpPost]
        public IActionResult Update(TXN_Orders order, string Id, string button)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                if (ModelState.IsValid)
                {
                    var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                    var areaDM = orderBC.GetOrder(decryptedId);
                    if (areaDM != null && areaDM.Order_Id != 0)
                    {
                        var vendor_id = HttpContext.Session.GetInt32("VendorId");
                        if (vendor_id != null)
                        {
                            foreach (var detail in order.TXN_Order_Details) {
                                var prod_id = detail.Product_Id;
                                var prod_name = detail.Product_Name;
                            }
                            if (button == "accept_order")
                            {
                                order.Status_Id = OrderStatus.ORDER_PREPARING;
                            }
                            else if (button == "out_for_delivery")
                            {
                                order.Status_Id = OrderStatus.OUT_FOR_DELIVERY;
                            }
                            else if (button == "delivered")
                            {
                                order.Status_Id = OrderStatus.ORDER_DELIVERED;
                            }
                           
                            order.Order_Id = decryptedId;
                            orderBC.SaveOrder(order);

                            TXN_Order_Logs log = new TXN_Order_Logs();
                            log.Order_Id = order.Order_Id;
                            log.Status_Id = order.Status_Id;
                            log.Created_Datetime = StaticMethods.GetKuwaitTime();

                            if (button == "accept_order")
                            {
                                log.Comments = "Order Accepted";
                            }
                            else if (button == "out_for_delivery")
                            {
                                log.Comments = "Order is out for delivery";
                            }
                            else if (button == "delivered")
                            {
                                log.Comments = "Order is delivered";
                            }
                         
                            orderBC.CreateOrderLog(log);

                            return Redirect("/Merchant/List/" + list_id);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Login");
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("", "Order not exist");
                        return View("Create", order);
                    }
                }
                else
                {
                    return View("Create", order);
                }


            }
            catch (Exception ex)
            {
                /* lblError.Visible = true;
                 lblError.Text = "Invalid username or password";*/
                ModelState.AddModelError("name", "Due to some technical error, data not saved");
                globalCls.WriteToFile(logPath, ex.ToString(), true);

            }
            return View("Create");
        }

        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
