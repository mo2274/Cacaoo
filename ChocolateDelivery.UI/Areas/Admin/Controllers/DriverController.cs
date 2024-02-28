using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DriverController : Controller
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        AppUserBC appUserBC;


        public DriverController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            appUserBC = new AppUserBC(context);

        }
        public IActionResult Create()
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            return View();
        }

        // HTTP POST VERSION  
        [HttpPost]
        public IActionResult Create(App_Users restaurant)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                if (ModelState.IsValid)
                {


                    var user_cd = HttpContext.Session.GetInt32("UserCd");
                    if (user_cd != null)
                    {                        
                        restaurant.Row_Id = Guid.NewGuid();
                        restaurant.App_User_Type = App_User_Types.DRIVER;
                        restaurant.Login_Type = Login_Types.EMAIL;
                        restaurant.Created_By = Convert.ToInt16(user_cd);
                        restaurant.Created_Datetime = StaticMethods.GetKuwaitTime();
                        appUserBC.CreateAppUser(restaurant);
                        return Redirect("/List/" + list_id);
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
                globalCls.WriteToFile(logPath, ex.ToString(), true);

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
                var areaexist = appUserBC.GetAppUser(decryptedId);
                if (areaexist != null && areaexist.App_User_Id != 0)
                {
                    return View("Create", areaexist);
                }
                else
                {
                    ModelState.AddModelError("name", "Restaurant not exist");
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
        public IActionResult Update(App_Users restaurant, string Id)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                if (ModelState.IsValid)
                {
                    var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                    var areaDM = appUserBC.GetAppUser(decryptedId);
                    if (areaDM != null && areaDM.App_User_Id != 0)
                    {

                        var user_cd = HttpContext.Session.GetInt32("UserCd");
                        if (user_cd != null)
                        {
                            
                            restaurant.App_User_Id = decryptedId;
                            restaurant.Updated_By = Convert.ToInt16(user_cd);
                            restaurant.Updated_Datetime = StaticMethods.GetKuwaitTime();
                            appUserBC.CreateAppUser(restaurant);
                            return Redirect("/List/" + list_id);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Login");
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("", "Restaurant not exist");
                        return View("Create", restaurant);
                    }
                }
                else
                {
                    return View("Create", restaurant);
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
    }
}
