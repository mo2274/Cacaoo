using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        UserBC userBC;


        public UserController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;           
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            userBC = new UserBC(context, logPath);
        }
        public IActionResult Create()
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            return View();
        }

        // HTTP POST VERSION  
        [HttpPost]
        public IActionResult Create(SM_USERS user)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                if (ModelState.IsValid)
                {
                    var areaexist = userBC.isUserExist(user.User_Id);
                    if (areaexist == null)
                    {
                        var user_cd = HttpContext.Session.GetInt32("UserCd");
                        if (user_cd != null)
                        {
                            var entity_id = Convert.ToInt32(HttpContext.Session.GetInt32("Entity_Id"));
                            user.Entity_Id = entity_id;
                            //user.User_Cd = Convert.ToInt16(user_cd);                            
                            userBC.CreateUser(user);
                            return Redirect("/List/" + list_id);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Login");
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("name", "User Id Already Exist . Please Add Different User Id.");
                    }
                }
                else
                    return View(user);

            }
            catch (Exception ex)
            {
                /* lblError.Visible = true;
                 lblError.Text = "Invalid username or password";*/
                ModelState.AddModelError("name", "Due to some technical error, data not saved");
                globalCls.WriteToFile(logPath, ex.ToString(), true);

            }
            return View(user);
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
                var decryptedId = Convert.ToInt16(StaticMethods.GetDecrptedString(Id));
                var areaexist = userBC.GetUser(decryptedId);
                if (areaexist != null && areaexist.User_Cd != 0)
                {
                    return View("Create", areaexist);
                }
                else
                {
                    ModelState.AddModelError("name", "User not exist");
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
        public IActionResult Update(SM_USERS user, string Id)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                if (ModelState.IsValid)
                {
                    var decryptedId = Convert.ToInt16(StaticMethods.GetDecrptedString(Id));
                    var areaDM = userBC.GetUser(decryptedId);
                    if (areaDM != null && areaDM.User_Cd != 0)
                    {
                        var areaexist = userBC.isUserExist(user.User_Id);
                        if (areaexist == null || (areaexist != null && areaexist.User_Cd == decryptedId))
                        {
                            user.User_Cd = decryptedId;
                            userBC.CreateUser(user);
                            return Redirect("/List/" + list_id);
                        }
                        else
                        {
                            ModelState.AddModelError("", "User Id Already Exist . Please Add Different User Id.");
                            return View("Create", user);
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("", "User not exist");
                        return View("Create", user);
                    }
                }
                else
                {
                    return View("Create", user);
                }


            }
            catch (Exception ex)
            {
                /* lblError.Visible = true;
                 lblError.Text = "Invalid username or password";*/
                ModelState.AddModelError("name", "Due to some technical error, data not saved");
                globalCls.WriteToFile(logPath, ex.ToString(), true);

            }
            return View("Create", user);
        }
    }
}
