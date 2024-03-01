using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductTypeController : Controller
{
    private AppDbContext context;
    private readonly IConfiguration _config;
    private IWebHostEnvironment iwebHostEnvironment;
    private string logPath = "";
    ProductTypeService _productTypeService;


    public ProductTypeController(AppDbContext cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
    {
        context = cc;
        _config = config;
        this.iwebHostEnvironment = iwebHostEnvironment;
        logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
        _productTypeService = new ProductTypeService(context);

    }
    public IActionResult Create()
    {
        var list_id = Request.Query["List_Id"];
        ViewBag.List_Id = list_id;
        return View();
    }

    // HTTP POST VERSION  
    [HttpPost]
    public IActionResult Create(SM_Product_Types type)
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

                    type.Created_By = Convert.ToInt16(user_cd);
                    type.Created_Datetime = StaticMethods.GetKuwaitTime();
                    _productTypeService.CreateType(type);
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
            var areaexist = _productTypeService.GetType(decryptedId);
            if (areaexist != null && areaexist.Type_Id != 0)
            {
                return View("Create", areaexist);
            }
            else
            {
                ModelState.AddModelError("name", "Type not exist");
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
    public IActionResult Update(SM_Product_Types type, string Id)
    {
        try
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            if (ModelState.IsValid)
            {
                var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                var areaDM = _productTypeService.GetType(decryptedId);
                if (areaDM != null && areaDM.Type_Id != 0)
                {

                    var user_cd = HttpContext.Session.GetInt32("UserCd");
                    if (user_cd != null)
                    {
                        type.Type_Id = decryptedId;
                        type.Updated_By = Convert.ToInt16(user_cd);
                        type.Updated_Datetime = StaticMethods.GetKuwaitTime();
                        _productTypeService.CreateType(type);
                        return Redirect("/List/" + list_id);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Login");
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Type not exist");
                    return View("Create", type);
                }
            }
            else
            {
                return View("Create", type);
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