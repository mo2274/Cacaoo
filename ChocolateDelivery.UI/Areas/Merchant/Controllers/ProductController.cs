using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.Areas.Merchant.Models;
using ChocolateDelivery.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ChocolateDelivery.UI.Areas.Merchant.Controllers
{
    [Area("Merchant")]
    public class ProductController : Controller
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        ProductBC productBC;
        SubCategoryBC subCategoryBC;


        public ProductController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            productBC = new ProductBC(context);
            subCategoryBC = new SubCategoryBC(context);
           
        }
        public IActionResult Create()
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            SetSubCategories();
            BranchBC branchBC = new BranchBC(context);
            var vendor_id = HttpContext.Session.GetInt32("VendorId");
            if (vendor_id != null) {
                var branches = branchBC.GetRestaurantBranches(vendor_id ?? 0);
                ViewBag.Branches = branches;
            }
            else
            {
                globalCls.WriteToFile(logPath, "Getting vendor id null while creating product", true);
                return RedirectToAction("Index", "Login");
            }
           
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


                    var vendor_id = HttpContext.Session.GetInt32("VendorId");
                    if (vendor_id != null)
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
                        product.Restaurant_Id = Convert.ToInt32(vendor_id);
                        product.Status_Id = 1;
                        product.Created_By = Convert.ToInt16(vendor_id);
                        product.Created_Datetime = StaticMethods.GetKuwaitTime();
                        product.Publish = true; // by default we are publishing the product
                        product.Is_Exclusive = false;
                        productBC.CreateProduct(product);

                        foreach (var detail in product.SM_Product_AddOns)
                        {
                            detail.Product_Id = product.Product_Id;
                            productBC.CreateProductAddOn(detail);
                        }
                        foreach (var detail in product.SM_Product_Branches)
                        {
                            detail.Product_Id = product.Product_Id;
                            productBC.CreateProductBranch(detail);
                        }
                        foreach (var occasion_id in product.Occasion_Ids)
                        {
                            var occasionDM = new SM_Product_Occasions();
                            occasionDM.Product_Id = product.Product_Id;
                            occasionDM.Occasion_Id = Convert.ToInt64(occasion_id);
                            productBC.CreateProductOccasion(occasionDM);
                        }
                        foreach (var detail in product.SM_Product_Catering_Products)
                        {
                            detail.Product_Id = product.Product_Id;
                            productBC.CreateProductCategoryProduct(detail);
                        }
                        return Redirect("/Merchant/List/" + list_id);
                    }
                    else
                    {
                        globalCls.WriteToFile(logPath,"Getting vendor id null while creating product", true);
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
                SetSubCategories();

                var decryptedId = Convert.ToInt32(StaticMethods.GetDecrptedString(Id));
                var areaexist = productBC.GetProduct(decryptedId);
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
                globalCls.WriteToFile(logPath, ex.ToString(), true);

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
                    var areaDM = productBC.GetProduct(decryptedId);
                    if (areaDM != null && areaDM.Product_Id != 0)
                    {

                        var vendor_id = HttpContext.Session.GetInt32("VendorId");
                        if (vendor_id != null)
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
                            product.Publish = areaDM.Publish; // we will keep `Publish` flag as it is as it will only updated by admin
                            product.Is_Exclusive = areaDM.Is_Exclusive; // we will keep `Is_Exclusive` flag as it is as it will only updated by admin
                            product.Product_Id = decryptedId;
                            product.Updated_By = Convert.ToInt16(vendor_id);
                            product.Updated_Datetime = StaticMethods.GetKuwaitTime();
                            productBC.CreateProduct(product);
                            foreach (var detail in product.SM_Product_AddOns)
                            {
                                detail.Product_Id = product.Product_Id;
                                productBC.CreateProductAddOn(detail);
                            }
                            foreach (var detail in product.SM_Product_Branches)
                            {
                                detail.Product_Id = product.Product_Id;
                                productBC.CreateProductBranch(detail);
                            }
                            productBC.DeleteProductOccasions(product.Product_Id);
                            foreach (var occasion_id in product.Occasion_Ids)
                            {
                                var occasionDM = new SM_Product_Occasions();
                                occasionDM.Product_Id = product.Product_Id;
                                occasionDM.Occasion_Id = Convert.ToInt64(occasion_id);
                                productBC.CreateProductOccasion(occasionDM);
                            }
                            foreach (var detail in product.SM_Product_Catering_Products)
                            {
                                detail.Product_Id = product.Product_Id;
                                productBC.CreateProductCategoryProduct(detail);
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
                globalCls.WriteToFile(logPath, ex.ToString(), true);

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
                    subCategories = subCategoryBC.GetAllSubCategories();
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "SubCategories", subCategories);
                    ViewBag.Sub_Categories = JsonSerializer.Serialize(subCategories);
                }
            }
            catch (Exception ex)
            {
                /* lblError.Visible = true;
                 lblError.Text = "Invalid username or password";*/
                ModelState.AddModelError("name", "Due to some technical error, data not saved");
                globalCls.WriteToFile(logPath, ex.ToString(), true);

            }
        }

        [HttpPost]
        public JsonResult DeleteProductAddOn(AddOnDeleteRequest request)
        {
            var response = new AddOnDeleteResponse();
            try
            {

                var detailDM = productBC.GetProductAddOn(request.Detail_Id);
                if (detailDM != null)
                {
                    detailDM.Deleted_By = request.Deleted_By;
                    detailDM.Deleted_Datetime = StaticMethods.GetKuwaitTime();
                    productBC.DeleteProductAddOn(detailDM);
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
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }

            return Json(response);
        }

        [HttpPost]
        public JsonResult DeleteProductCateringProduct(AddOnDeleteRequest request)
        {
            var response = new AddOnDeleteResponse();
            try
            {

                var detailDM = productBC.GetProductCateringProduct(request.Detail_Id);
                if (detailDM != null)
                {
                    detailDM.Deleted_By = request.Deleted_By;
                    detailDM.Deleted_Datetime = StaticMethods.GetKuwaitTime();
                    productBC.DeleteProductCateringProduct(detailDM);
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
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }

            return Json(response);
        }
    }
}
