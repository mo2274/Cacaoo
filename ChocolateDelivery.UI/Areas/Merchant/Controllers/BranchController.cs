using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ChocolateDelivery.UI.Areas.Merchant.Controllers
{
    [Area("Merchant")]
    public class BranchController : Controller
    {
        private AppDbContext context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        BranchService _branchService;
        decimal defaultLatitude = 29.3463985M;
        decimal defaultLongitude = 47.9785674M;

        public BranchController(AppDbContext cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            _branchService = new BranchService(context);

        }
        public IActionResult Create()
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            ViewBag.Latitude = defaultLatitude;
            ViewBag.Longitude = defaultLongitude;
            return View();
        }

        // HTTP POST VERSION  
        [HttpPost]
        public IActionResult Create(SM_Restaurant_Branches branch)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                ViewBag.Latitude = branch.Latitude;
                ViewBag.Longitude = branch.Longitude;
                if (ModelState.IsValid)
                {


                    var vendor_id = HttpContext.Session.GetInt32("VendorId");
                    if (vendor_id != null)
                    {
                        if (!string.IsNullOrEmpty(branch.Opening_Time_String))
                        {
                            branch.Opening_Time = DateTime.ParseExact(branch.Opening_Time_String, "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                        }
                        if (!string.IsNullOrEmpty(branch.Closing_Time_String))
                        {
                            branch.Closing_Time = DateTime.ParseExact(branch.Closing_Time_String, "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                        }
                        branch.Restaurant_Id = Convert.ToInt32(vendor_id);
                        branch.Created_By = Convert.ToInt16(vendor_id);
                        branch.Created_Datetime = StaticMethods.GetKuwaitTime();
                        _branchService.CreateBranch(branch);
                        return Redirect("/Merchant/List/" + list_id);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Login");
                    }



                }
                else
                    return View();

            }
            catch (Exception ex)
            {
                /* lblError.Visible = true;
                 lblError.Text = "Invalid username or password";*/
                ModelState.AddModelError("name", "Due to some technical error, data not saved");
                Helpers.WriteToFile(logPath, ex.ToString(), true);

            }
            return View();
            /*if (ModelState.IsValid)
            {

                var userDM = userBC.isUserExist(user.User_Id.Trim());
                //go to dashboard page
                return View("Thanks");
            }
            else
                return View();*/

        }

        public IActionResult Update(string Id)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                var areaexist = _branchService.GetBranch(decryptedId);
                ViewBag.Latitude = defaultLatitude;
                ViewBag.Longitude = defaultLongitude;
                if (areaexist != null && areaexist.Branch_Id != 0)
                {
                    ViewBag.Latitude = areaexist.Latitude ?? defaultLatitude;
                    ViewBag.Longitude = areaexist.Longitude ?? defaultLongitude;
                    return View("Create", areaexist);
                }
                else
                {
                    ModelState.AddModelError("name", "Branch not exist");
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
        public IActionResult Update(SM_Restaurant_Branches branch, string Id)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                ViewBag.Latitude = branch.Latitude;
                ViewBag.Longitude = branch.Longitude;
                if (ModelState.IsValid)
                {
                    var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                    var areaDM = _branchService.GetBranch(decryptedId);
                    if (areaDM != null && areaDM.Branch_Id != 0)
                    {

                        var vendor_id = HttpContext.Session.GetInt32("VendorId");
                        if (vendor_id != null)
                        {
                            if (!string.IsNullOrEmpty(branch.Opening_Time_String))
                            {
                                branch.Opening_Time = DateTime.ParseExact(branch.Opening_Time_String, "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                            }
                            if (!string.IsNullOrEmpty(branch.Closing_Time_String))
                            {
                                branch.Closing_Time = DateTime.ParseExact(branch.Closing_Time_String, "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                            }
                            branch.Branch_Id = decryptedId;
                            branch.Updated_By = Convert.ToInt16(vendor_id);
                            branch.Updated_Datetime = StaticMethods.GetKuwaitTime();
                            _branchService.CreateBranch(branch);
                            return Redirect("/Merchant/List/" + list_id);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Login");
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("", "Branch not exist");
                        return View("Create", branch);
                    }
                }
                else
                {
                    return View("Create", branch);
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
    }
}
