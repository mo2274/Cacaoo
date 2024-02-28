using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Route(nameof(Admin) + "/[controller]")]
    public class LoginController : Controller
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        public LoginController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
        }
        public IActionResult Index()
        {
            HttpContext.Session.Clear();
            return View();
        }
        // HTTP POST VERSION  
        [HttpPost]
        public IActionResult Index(SM_USERS user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(user.User_Id))
                    {
                        ModelState.AddModelError("name", "Enter Username");
                        return View();
                    }
                    if (string.IsNullOrEmpty(user.Password))
                    {
                        ModelState.AddModelError("name", "Enter Password");
                        return View();
                    }
                    var commonBC = new CommonBC(context,logPath);
                    var userBC = new UserBC(context, logPath);
                    if (user.User_Id == "HSAdmin" && user.Password == "Test123")
                    {
                        HttpContext.Session.SetString("UserName", "Admin");
                        HttpContext.Session.SetInt32("UserCd", 1);
                        HttpContext.Session.SetString("UserId", "Admin");
                        HttpContext.Session.SetString("Culture", user.Preferred_Language);
                        HttpContext.Session.SetInt32("GroupCd", 1);
                        HttpContext.Session.SetString("IsSuperAdmin", "true");
                        HttpContext.Session.SetInt32("Entity_Id", 1);
                        /*Session["UserName"] = "Admin";
                        Session["UserCd"] = 1;
                        Session["UserId"] = "Admin";
                        Session["Culture"] = cmbLanguage.SelectedValue;
                        Session["GroupCd"] = 1;
                        Session["IsSuperAdmin"] = true;
                        Response.Redirect("Dashboard.aspx", false);*/
                        //HttpContext.Current.ApplicationInstance.CompleteRequest();
                        var labels = commonBC.GetAllLabels();
                        SessionHelper.SetObjectAsJson(HttpContext.Session, "AppLabels", labels);
                        //List<SM_LABELS> _sessionList = SessionHelper.GetObjectFromJson<List<SM_LABELS>>(HttpContext.Session, "AppLabels");
                        if (string.IsNullOrEmpty(user.Default_Page))
                            return RedirectToAction("Index", "Home");
                        else
                            Response.Redirect(user.Default_Page);
                    }
                    else
                    {
                        var userDM = userBC.isUserExist(user.User_Id.Trim());
                        if (userDM != null && userDM.User_Cd != 0)
                        {
                            var userDMWeb = userBC.ValidateUser(user.User_Id, user.Password);
                            if (userDMWeb != null && userDMWeb.User_Cd != 0)
                            {
                                /* Session["UserName"] = userDMWeb.User_Name;
                                 Session["UserCd"] = userDMWeb.User_Cd;
                                 Session["UserId"] = userDMWeb.User_Id;
                                 Session["Culture"] = cmbLanguage.SelectedValue;
                                 Session["GroupCd"] = userDMWeb.Group_Cd;
                                 Response.Redirect("Dashboard.aspx", false);*/
                                HttpContext.Session.SetString("UserName", userDMWeb.User_Name);
                                HttpContext.Session.SetInt32("UserCd", userDMWeb.User_Cd);
                                HttpContext.Session.SetString("UserId", userDMWeb.User_Id);
                                HttpContext.Session.SetString("Culture", user.Preferred_Language);
                                HttpContext.Session.SetInt32("GroupCd", userDMWeb.Group_Cd);
                                HttpContext.Session.SetString("IsSuperAdmin", "false");
                                HttpContext.Session.SetInt32("Entity_Id", (int)userDMWeb.Entity_Id);
                                var labels = commonBC.GetAllLabels();
                                SessionHelper.SetObjectAsJson(HttpContext.Session, "AppLabels", labels);
                                if (string.IsNullOrEmpty(userDMWeb.Default_Page))
                                    return RedirectToAction("Index", "Home");
                                else
                                    Response.Redirect(userDMWeb.Default_Page);

                            }
                            else
                            {
                                /* lblError.Visible = true;
                                 lblError.Text = "Invalid username or password";*/
                                ModelState.AddModelError("name", "Invalid Username or Password");
                            }

                        }
                        else
                        {
                            /* lblError.Visible = true;
                             lblError.Text = "user doesn't exist";*/

                            //adding error message to ModelState
                            ModelState.AddModelError("name", "Invalid Username or Password");
                        }
                    }
                }
                else
                    return View();

            }
            catch (Exception ex)
            {
                /* lblError.Visible = true;
                 lblError.Text = "Invalid username or password";*/
                ModelState.AddModelError("name", "Due to some technical error, cannot login");
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
    }
}
