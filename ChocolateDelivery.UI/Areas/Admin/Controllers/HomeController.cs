using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        OrderBC orderBC;
        public HomeController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            orderBC = new OrderBC(context);

        }
        public IActionResult Index()
        {
            var current_datetime = StaticMethods.GetKuwaitTime();
            ViewBag.From_Date = current_datetime.ToString("dd-MMM-yyyy");
            ViewBag.To_Date = current_datetime.ToString("dd-MMM-yyyy");
            LoadReport(current_datetime.Date, current_datetime);
            return View();
        }

        [HttpPost]
        public IActionResult Index(DateTime txtFromDate, DateTime txtToDate)
        {
          
            ViewBag.From_Date = txtFromDate.ToString("dd-MMM-yyyy");
            ViewBag.To_Date = txtToDate.ToString("dd-MMM-yyyy");
            var toDateString = txtToDate.ToString("dd-MMM-yyyy") + " 23:59:59";
            var toDate = DateTime.ParseExact(toDateString, "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            LoadReport(txtFromDate, toDate);
            return View();
        }

        public void LoadReport(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                var orders = orderBC.GetReportOrders(FromDate, ToDate, "E");
                var orderDetails = orderBC.GetReportOrderDetails(FromDate, ToDate, "E");
                var total_orders = orders.Count();
                var total_amount = orders.Sum(x => x.Gross_Amount+x.Delivery_Charges);
                var total_qty = orderDetails.Sum(x => x.Qty);

                ViewBag.Avg_Basket_Value = decimal.Zero;
                ViewBag.Avg_Basket_Size = decimal.Zero;
                if (total_orders > 0) {
                    ViewBag.Avg_Basket_Value = Math.Round( total_amount / total_orders,3);
                    ViewBag.Avg_Basket_Size =total_qty / total_orders;
                }
               
                ViewBag.Total_Sales = total_amount;
                DeviceBC deviceBC = new DeviceBC(context);
                var total_downloads = deviceBC.GetTotalDownloads(FromDate,ToDate);
                ViewBag.Total_Downloads = total_downloads.Count;
                var total_monthly_downloads = deviceBC.GetTotalMonthlyDownloads(FromDate, ToDate);
                ViewBag.Total_Monthly_Downloads = total_monthly_downloads.Count;
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("name", "Due to some technical error, data not saved");
                globalCls.WriteToFile(logPath, ex.ToString(), true);

            }
        }
    }
}
