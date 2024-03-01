using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace ChocolateDelivery.UI.Areas.Merchant.Controllers;

[Area("Merchant")]
public class OrderController : Controller
{
    private AppDbContext context;
    private readonly IConfiguration _config;
    private IWebHostEnvironment iwebHostEnvironment;
    private string logPath = "";
    OrderService _orderService;


    public OrderController(AppDbContext cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
    {
        context = cc;
        _config = config;
        this.iwebHostEnvironment = iwebHostEnvironment;
        logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
        _orderService = new OrderService(context);


    }
    public IActionResult Update(string Id)
    {
        try
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;

            var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
            var areaexist = _orderService.GetOrder(decryptedId);
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
                        var QrGenerator = new QRCodeGenerator();
                        var QrCodeInfo = QrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                        var QrCode = new QRCode(QrCodeInfo);
                        var QrBitmap = QrCode.GetGraphic(60);
                        // use this when you want to show your logo in middle of QR Code and change color of qr code
                        //Bitmap logoImage = new Bitmap(@"wwwroot/assets/img/logo/cacaoo_favicon.png");
                        //var qrCodeAsBitmap = QrCode.GetGraphic(60, Color.Black, Color.White, logoImage);
                        var BitmapArray = BitmapToByteArray(QrBitmap);
                        var QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
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
            Helpers.WriteToFile(logPath, ex.ToString(), true);

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
                var areaDM = _orderService.GetOrder(decryptedId);
                if (areaDM != null && areaDM.Order_Id != 0)
                {
                    var vendor_id = HttpContext.Session.GetInt32("VendorId");
                    if (vendor_id != null)
                    {
                        foreach (var detail in order.TXN_Order_Details) {
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
                        _orderService.SaveOrder(order);

                        var log = new TXN_Order_Logs
                        {
                            Order_Id = order.Order_Id,
                            Status_Id = order.Status_Id,
                            Created_Datetime = StaticMethods.GetKuwaitTime()
                        };

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
                         
                        _orderService.CreateOrderLog(log);

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
            Helpers.WriteToFile(logPath, ex.ToString(), true);

        }
        return View("Create");
    }

    private byte[] BitmapToByteArray(Bitmap bitmap)
    {
        using (var ms = new MemoryStream())
        {
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}