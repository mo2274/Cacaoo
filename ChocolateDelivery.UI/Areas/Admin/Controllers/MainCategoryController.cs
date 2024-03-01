using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers;

[Area("Admin")]
public class MainCategoryController : Controller
{
    private AppDbContext context;
    private readonly IConfiguration _config;
    private IWebHostEnvironment iwebHostEnvironment;
    private string logPath = "";
    CategoryService _categoryService;


    public MainCategoryController(AppDbContext cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
    {
        context = cc;
        _config = config;
        this.iwebHostEnvironment = iwebHostEnvironment;
        logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
        _categoryService = new CategoryService(context);

    }
    public IActionResult Create()
    {
        var list_id = Request.Query["List_Id"];
        ViewBag.List_Id = list_id;
        return View();
    }

    // HTTP POST VERSION  
    [HttpPost]
    public IActionResult Create(SM_Main_Categories category)
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

                    category.Created_By = Convert.ToInt16(user_cd);
                    category.Created_Datetime = StaticMethods.GetKuwaitTime();
                    _categoryService.CreateMainCategory(category);
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
            var areaexist = _categoryService.GetMainCategory(decryptedId);
            if (areaexist != null && areaexist.Category_Id != 0)
            {
                return View("Create", areaexist);
            }
            else
            {
                ModelState.AddModelError("name", "Category not exist");
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
    public IActionResult Update(SM_Main_Categories category, string Id)
    {
        try
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            if (ModelState.IsValid)
            {
                var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                var areaDM = _categoryService.GetMainCategory(decryptedId);
                if (areaDM != null && areaDM.Category_Id != 0)
                {

                    var user_cd = HttpContext.Session.GetInt32("UserCd");
                    if (user_cd != null)
                    {
                        category.Category_Id = decryptedId;
                        category.Updated_By = Convert.ToInt16(user_cd);
                        category.Updated_Datetime = StaticMethods.GetKuwaitTime();
                        _categoryService.CreateMainCategory(category);
                        return Redirect("/List/" + list_id);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Login");
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Category not exist");
                    return View("Create", category);
                }
            }
            else
            {
                return View("Create", category);
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