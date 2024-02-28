using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MerchantController : Controller
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        RestaurantBC restaurantBC;


        public MerchantController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            restaurantBC = new RestaurantBC(context);

        }
        public IActionResult Create()
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            return View();
        }

        // HTTP POST VERSION  
        [HttpPost]
        public IActionResult Create(SM_Restaurants restaurant)
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
                        if (restaurant.Image_File != null)
                        {
                            var image_path_dir = "assets/images/categories/";
                            var fileName = Guid.NewGuid().ToString("N").Substring(0, 12) + "_" + restaurant.Image_File.FileName;
                            var path = Path.Combine(this.iwebHostEnvironment.WebRootPath, image_path_dir);
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            var filePath = Path.Combine(path, fileName);
                            var stream = new FileStream(filePath, FileMode.Create);
                            restaurant.Image_File.CopyToAsync(stream);

                            restaurant.Image_URL = image_path_dir + fileName;
                        }
                        restaurant.Row_Id = Guid.NewGuid();
                        restaurant.Created_By = Convert.ToInt16(user_cd);
                        restaurant.Created_Datetime = StaticMethods.GetKuwaitTime();
                        restaurantBC.CreateRestaurant(restaurant);
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
                var areaexist = restaurantBC.GetRestaurant(decryptedId);
                if (areaexist != null && areaexist.Restaurant_Id != 0)
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
        public IActionResult Update(SM_Restaurants restaurant, string Id)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                if (ModelState.IsValid)
                {
                    var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                    var areaDM = restaurantBC.GetRestaurant(decryptedId);
                    if (areaDM != null && areaDM.Restaurant_Id != 0)
                    {

                        var user_cd = HttpContext.Session.GetInt32("UserCd");
                        if (user_cd != null)
                        {
                            if (restaurant.Image_File != null)
                            {
                                var image_path_dir = "assets/images/categories/";
                                var fileName = Guid.NewGuid().ToString("N").Substring(0, 12) + "_" + restaurant.Image_File.FileName;
                                var path = Path.Combine(this.iwebHostEnvironment.WebRootPath, image_path_dir);
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                var filePath = Path.Combine(path, fileName);
                                var stream = new FileStream(filePath, FileMode.Create);
                                restaurant.Image_File.CopyToAsync(stream);

                                restaurant.Image_URL = image_path_dir + fileName;
                            }
                            if (!string.IsNullOrEmpty(restaurant.Opening_Time_String)) {
                                restaurant.Opening_Time = DateTime.ParseExact(restaurant.Opening_Time_String, "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                            }
                            if (!string.IsNullOrEmpty(restaurant.Closing_Time_String))
                            {
                                restaurant.Closing_Time = DateTime.ParseExact(restaurant.Closing_Time_String, "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
                            }
                            restaurant.Restaurant_Id = decryptedId;
                            restaurant.Updated_By = Convert.ToInt16(user_cd);
                            restaurant.Updated_Datetime = StaticMethods.GetKuwaitTime();
                            restaurantBC.CreateRestaurant(restaurant);
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
                    ModelState.AddModelError("", "Model State is not valid");
                    var message = string.Join(" | ", ModelState.Values
      .SelectMany(v => v.Errors)
      .Select(e => e.ErrorMessage));
                    globalCls.WriteToFile(logPath, "Model state invalid in invoice for field:" + message, true);
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
