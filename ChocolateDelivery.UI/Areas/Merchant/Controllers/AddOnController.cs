using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Merchant.Controllers;

[Area("Merchant")]
public class AddOnController : Controller
{
    private AppDbContext context;
    private readonly IConfiguration _config;
    private IWebHostEnvironment iwebHostEnvironment;
    private string logPath = "";
    AddOnService _brandService;


    public AddOnController(AppDbContext cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
    {
        context = cc;
        _config = config;
        this.iwebHostEnvironment = iwebHostEnvironment;
        logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
        _brandService = new AddOnService(context);

    }
    public IActionResult Create()
    {
        var list_id = Request.Query["List_Id"];
        ViewBag.List_Id = list_id;
        return View();
    }

    // HTTP POST VERSION  
    [HttpPost]
    public IActionResult Create(SM_Restaurant_AddOns brand)
    {
        try
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            if (ModelState.IsValid)
            {


                var vendor_id = HttpContext.Session.GetInt32("VendorId");
                if (vendor_id != null)
                {
                    if (brand.Image_File != null)
                    {
                        var image_path_dir = "assets/images/categories/";
                        var fileName = Guid.NewGuid().ToString("N").Substring(0, 12) + "_" + brand.Image_File.FileName;
                        var path = Path.Combine(this.iwebHostEnvironment.WebRootPath, image_path_dir);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        var filePath = Path.Combine(path, fileName);
                        var stream = new FileStream(filePath, FileMode.Create);
                        brand.Image_File.CopyToAsync(stream);

                        brand.Image_URL = image_path_dir + fileName;
                    }
                    brand.Restaurant_Id = Convert.ToInt32(vendor_id);
                    brand.Created_By = Convert.ToInt16(vendor_id);
                    brand.Created_Datetime = StaticMethods.GetKuwaitTime();
                    _brandService.CreateAddOn(brand);
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
            var areaexist = _brandService.GetAddOn(decryptedId);
            if (areaexist != null && areaexist.AddOn_Id != 0)
            {
                return View("Create", areaexist);
            }
            else
            {
                ModelState.AddModelError("name", "AddOn not exist");
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
    public IActionResult Update(SM_Restaurant_AddOns brand, string Id)
    {
        try
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            if (ModelState.IsValid)
            {
                var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                var areaDM = _brandService.GetAddOn(decryptedId);
                if (areaDM != null && areaDM.AddOn_Id != 0)
                {

                    var vendor_id = HttpContext.Session.GetInt32("VendorId");
                    if (vendor_id != null)
                    {
                        if (brand.Image_File != null)
                        {
                            var image_path_dir = "assets/images/categories/";
                            var fileName = Guid.NewGuid().ToString("N").Substring(0, 12) + "_" + brand.Image_File.FileName;
                            var path = Path.Combine(this.iwebHostEnvironment.WebRootPath, image_path_dir);
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            var filePath = Path.Combine(path, fileName);
                            var stream = new FileStream(filePath, FileMode.Create);
                            brand.Image_File.CopyToAsync(stream);

                            brand.Image_URL = image_path_dir + fileName;
                        }
                        brand.AddOn_Id = decryptedId;
                        brand.Updated_By = Convert.ToInt16(vendor_id);
                        brand.Updated_Datetime = StaticMethods.GetKuwaitTime();
                        _brandService.CreateAddOn(brand);
                        return Redirect("/Merchant/List/" + list_id);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Login");
                    }

                }
                else
                {
                    ModelState.AddModelError("", "AddOn not exist");
                    return View("Create", brand);
                }
            }
            else
            {
                return View("Create", brand);
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