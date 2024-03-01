using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeGroupController : Controller
{
    private AppDbContext context;
    private readonly IConfiguration _config;
    private IWebHostEnvironment iwebHostEnvironment;
    private string logPath = "";
    HomeGroupService _homeGroupService;


    public HomeGroupController(AppDbContext cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
    {
        context = cc;
        _config = config;
        this.iwebHostEnvironment = iwebHostEnvironment;
        logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
        _homeGroupService = new HomeGroupService(context);

    }
    public IActionResult Create()
    {
        var list_id = Request.Query["List_Id"];
        ViewBag.List_Id = list_id;
        return View();
    }

    // HTTP POST VERSION  
    [HttpPost]
    public IActionResult Create(SM_Home_Groups category)
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
                    _homeGroupService.CreateGroup(category);
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
            var areaexist = _homeGroupService.GetGroup(decryptedId);
            if (areaexist != null && areaexist.Group_Id != 0)
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
    public IActionResult Update(SM_Home_Groups group, string Id)
    {
        try
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            if (ModelState.IsValid)
            {
                var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                var areaDM = _homeGroupService.GetGroup(decryptedId);
                if (areaDM != null && areaDM.Group_Id != 0)
                {

                    var user_cd = HttpContext.Session.GetInt32("UserCd");
                    if (user_cd != null)
                    {
                            
                        group.Group_Id = decryptedId;
                        group.Updated_By = Convert.ToInt16(user_cd);
                        group.Updated_Datetime = StaticMethods.GetKuwaitTime();
                        _homeGroupService.CreateGroup(group);
                        foreach (var detail in group.SM_Home_Group_Details)
                        {
                            detail.Group_Id = group.Group_Id;
                            _homeGroupService.CreateGroupDetail(detail);
                        }
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
                    return View("Create", group);
                }
            }
            else
            {
                return View("Create", group);
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
    public JsonResult DeleteItem(ItemDeleteRequest request)
    {
        var response = new ItemDeleteResponse();
        try
        {

            var detailDM = _homeGroupService.GetGroupDetail(request.Detail_Id);
            if (detailDM != null)
            {
                   
                _homeGroupService.RemoveGroupdetail(request.Detail_Id);
                response.Status = 0;
                response.Message = ServiceResponse.Success;

                _homeGroupService.UpdateGroupDetails(detailDM.Line_No, detailDM.Group_Id);
                  
            }
            else
            {
                response.Status = 138;
                response.Message = ServiceResponse.NoInvoiceDetailFound;
            }
        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }

        return Json(response);
    }
}