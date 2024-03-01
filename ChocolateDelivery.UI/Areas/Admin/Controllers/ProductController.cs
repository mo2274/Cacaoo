using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.Areas.Merchant.Models;
using ChocolateDelivery.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private AppDbContext context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        ProductService _productService;
        SubCategoryService _subCategoryService;


        public ProductController(AppDbContext cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            _productService = new ProductService(context);
            _subCategoryService = new SubCategoryService(context);

        }
        public IActionResult Create()
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            SetSubCategories();
            return View();
        }

        // HTTP POST VERSION  
        [HttpPost]
        public IActionResult Create(SM_Products product)
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
                        if (product.Image_File != null)
                        {
                            var image_path_dir = "assets/images/categories/";
                            var fileName = Guid.NewGuid().ToString("N").Substring(0, 12) + "_" + product.Image_File.FileName;
                            var path = Path.Combine(this.iwebHostEnvironment.WebRootPath, image_path_dir);
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            var filePath = Path.Combine(path, fileName);
                            var stream = new FileStream(filePath, FileMode.Create);
                            product.Image_File.CopyToAsync(stream);

                            product.Image_URL = image_path_dir + fileName;
                        }
                        product.Main_Category_Id = 1;
                        product.Product_Type_Id = 1;
                        
                        product.Status_Id = 1;
                        product.Created_By = Convert.ToInt16(user_cd);
                        product.Created_Datetime = StaticMethods.GetKuwaitTime();
                        product.Show = true; // by default we are showing the product for vendor
                        _productService.CreateProduct(product);

                        foreach (var detail in product.SM_Product_AddOns)
                        {
                            detail.Product_Id = product.Product_Id;
                            _productService.CreateProductAddOn(detail);
                        }
                        foreach (var detail in product.SM_Product_Branches)
                        {
                            detail.Product_Id = product.Product_Id;
                            _productService.CreateProductBranch(detail);
                        }
                        foreach (var occasion_id in product.Occasion_Ids)
                        {
                            var occasionDM = new SM_Product_Occasions
                            {
                                Product_Id = product.Product_Id,
                                Occasion_Id = Convert.ToInt64(occasion_id)
                            };
                            _productService.CreateProductOccasion(occasionDM);
                        }
                        return Redirect("/Merchant/List/" + list_id);
                    }
                    else
                    {
                        Helpers.WriteToFile(logPath, "Getting user id null while creating product", true);
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
                SetSubCategories();

                var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                var areaexist = _productService.GetProduct(decryptedId);
                if (areaexist != null && areaexist.Product_Id != 0)
                {
                    return View("Create", areaexist);
                }
                else
                {
                    ModelState.AddModelError("name", "Product not exist");
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
        public IActionResult Update(SM_Products product, string Id)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                if (ModelState.IsValid)
                {
                    var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                    var areaDM = _productService.GetProduct(decryptedId);
                    if (areaDM != null && areaDM.Product_Id != 0)
                    {

                        var user_cd = HttpContext.Session.GetInt32("UserCd");
                        if (user_cd != null)
                        {
                            if (product.Image_File != null)
                            {
                                var image_path_dir = "assets/images/categories/";
                                var fileName = Guid.NewGuid().ToString("N").Substring(0, 12) + "_" + product.Image_File.FileName;
                                var path = Path.Combine(this.iwebHostEnvironment.WebRootPath, image_path_dir);
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                var filePath = Path.Combine(path, fileName);
                                var stream = new FileStream(filePath, FileMode.Create);
                                product.Image_File.CopyToAsync(stream);

                                product.Image_URL = image_path_dir + fileName;
                            }
                            product.Show = areaDM.Show; // we will keep `Show` flag as it is as it will only updated by vendor
                            product.Product_Id = decryptedId;
                            product.Updated_By = Convert.ToInt16(user_cd);
                            product.Updated_Datetime = StaticMethods.GetKuwaitTime();
                            _productService.CreateProduct(product);
                            foreach (var detail in product.SM_Product_AddOns)
                            {
                                detail.Product_Id = product.Product_Id;
                                _productService.CreateProductAddOn(detail);
                            }
                            _productService.CreateProduct(product);
                            foreach (var detail in product.SM_Product_Branches)
                            {
                                detail.Product_Id = product.Product_Id;
                                _productService.CreateProductBranch(detail);
                            }
                            _productService.DeleteProductOccasions(product.Product_Id);
                            foreach (var occasion_id in product.Occasion_Ids)
                            {
                                var occasionDM = new SM_Product_Occasions
                                {
                                    Product_Id = product.Product_Id,
                                    Occasion_Id = Convert.ToInt64(occasion_id)
                                };
                                _productService.CreateProductOccasion(occasionDM);
                            }
                            return Redirect("/Merchant/List/" + list_id);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Login");
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("", "Product not exist");
                        return View("Create", product);
                    }
                }
                else
                {
                    return View("Create", product);
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

        public void SetSubCategories()
        {
            try
            {
                var subCategories = SessionHelper.GetObjectFromJson<List<SM_Sub_Categories>>(HttpContext.Session, "SubCategories");
                if (subCategories != null)
                {
                    ViewBag.Sub_Categories = JsonSerializer.Serialize(subCategories);
                }
                else
                {
                    subCategories = _subCategoryService.GetAllSubCategories();
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "SubCategories", subCategories);
                    ViewBag.Sub_Categories = JsonSerializer.Serialize(subCategories);
                }
            }
            catch (Exception ex)
            {
                /* lblError.Visible = true;
                 lblError.Text = "Invalid username or password";*/
                ModelState.AddModelError("name", "Due to some technical error, data not saved");
                Helpers.WriteToFile(logPath, ex.ToString(), true);

            }
        }

        [HttpPost]
        public JsonResult DeleteProductAddOn(AddOnDeleteRequest request)
        {
            var response = new AddOnDeleteResponse();
            try
            {

                var detailDM = _productService.GetProductAddOn(request.Detail_Id);
                if (detailDM != null)
                {
                    detailDM.Deleted_By = request.Deleted_By;
                    detailDM.Deleted_Datetime = StaticMethods.GetKuwaitTime();
                    _productService.DeleteProductAddOn(detailDM);
                    response.Status = 0;
                    response.Message = ServiceResponse.Success;
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
}
