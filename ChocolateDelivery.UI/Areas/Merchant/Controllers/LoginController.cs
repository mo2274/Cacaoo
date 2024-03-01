using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Merchant.Controllers;

[Area(nameof(Merchant))]
[Route(nameof(Merchant) + "/[controller]")]
public class LoginController : Controller
{
    private AppDbContext context;
    private readonly IConfiguration _config;
    private IWebHostEnvironment iwebHostEnvironment;
    private string logPath = "";
    public LoginController(AppDbContext cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
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
    public IActionResult Index(SM_Restaurants user)
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(user.Username))
                {
                    ModelState.AddModelError("name", "Enter Username");
                    return View();
                }
                if (string.IsNullOrEmpty(user.Password))
                {
                    ModelState.AddModelError("name", "Enter Password");
                    return View();
                }
                var commonBC = new CommonService(context, logPath);
                var subCategoryBC = new SubCategoryService(context);
                var userBC = new RestaurantService(context);

                var userDMWeb = userBC.ValidateMerchant(user.Username, user.Password);
                if (userDMWeb != null && userDMWeb.Restaurant_Id != 0)
                {
                    /* Session["UserName"] = userDMWeb.User_Name;
                     Session["UserCd"] = userDMWeb.User_Cd;
                     Session["UserId"] = userDMWeb.User_Id;
                     Session["Culture"] = cmbLanguage.SelectedValue;
                     Session["GroupCd"] = userDMWeb.Group_Cd;
                     Response.Redirect("Dashboard.aspx", false);*/
                    HttpContext.Session.SetString("UserName", userDMWeb.Username);
                    HttpContext.Session.SetString("RestaurantName", userDMWeb.Restaurant_Name_E);
                    HttpContext.Session.SetInt32("VendorId", Convert.ToInt32(userDMWeb.Restaurant_Id));                      
                    HttpContext.Session.SetString("IsSuperAdmin", "false");
                       
                    var labels = commonBC.GetAllLabels();
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "AppLabels", labels);
                    var subCategories = subCategoryBC.GetAllSubCategories();
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "SubCategories", subCategories);
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    /* lblError.Visible = true;
                     lblError.Text = "Invalid username or password";*/
                    ModelState.AddModelError("name", "Invalid Username or Password");
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
}