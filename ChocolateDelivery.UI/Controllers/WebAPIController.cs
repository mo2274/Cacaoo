using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.CustomFilters;
using ChocolateDelivery.UI.Models;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MimeKit;
using MySqlConnector;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Crypto.Prng;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Transactions;
using static System.Net.WebRequestMethods;
//using static System.Net.Mime.MediaTypeNames;

namespace ChocolateDelivery.UI.Controllers
{
    [Route("api")]
    [ApiController]
    public class WebAPIController : ControllerBase
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private string logPath = "";
        private IWebHostEnvironment iwebHostEnvironment;

        public WebAPIController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment _iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            iwebHostEnvironment = _iwebHostEnvironment;
            logPath = Path.Combine(iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath"));


        }
        private Device_Registration? securityDTO = new Device_Registration();

        [Route("RegisterDevice")]
        [HttpPost]
        public RegisterDeviceResponse RegisterDevice(RegisterDeviceDTO registerDeviceDTO)
        {
            var response = new RegisterDeviceResponse();
            try
            {
                var entryBC = new DeviceBC(context, logPath);
                var deviceRegisterDM = new Device_Registration();

                deviceRegisterDM.Device_Id = registerDeviceDTO.UniqueDeviceId;
                deviceRegisterDM.Notification_Token = registerDeviceDTO.NotificationToken;
                deviceRegisterDM.Client_Key = StaticMethods.Base64Decode(registerDeviceDTO.ClientKey);
                deviceRegisterDM.Created_Datetime = StaticMethods.GetKuwaitTime(); ;
                deviceRegisterDM.Device_Type = registerDeviceDTO.DeviceType;
                deviceRegisterDM.Notification_Enabled = true;
                deviceRegisterDM.NotificationSound_Enabled = true;
                deviceRegisterDM.Preferred_Language = string.IsNullOrEmpty(registerDeviceDTO.Preferred_Language) ? "E" : registerDeviceDTO.Preferred_Language;
                if (registerDeviceDTO.AppType != null)
                {
                    deviceRegisterDM.App_Type = registerDeviceDTO.AppType ?? AppTypes.CLIENT;
                }
                else
                {
                    deviceRegisterDM.App_Type = AppTypes.CLIENT;
                }
                deviceRegisterDM = entryBC.RegisterDevice(deviceRegisterDM);
                response.Status = 0;
                response.Message = ServiceResponse.Success;

                #region Firebase Topic Messaging
                try
                {
                    /*FirebaseBC firebaseBC = new FirebaseBC();
                    var service_account_key_path = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["SERVICE_ACCOUNT_KEY_PATH"]);
                    var topic_name = "/topics/" + Regex.Replace("TestTopic", @"\s+", "");
                    var reg_tokens = new List<string> { registerDeviceDTO.NotificationToken };
                    var fcmresponse = firebaseBC.SubscribeToTopic(topic_name, reg_tokens, service_account_key_path);*/
                }
                catch (Exception ex)
                {
                    globalCls.WriteToFile(logPath, ex.ToString(), true);

                }

                #endregion
            }
            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }
            return response;
        }

        [Route("Register")]
        [HttpPost]
        public RegisterResponse Register(RegisterRequest registerRequest)
        {
            string securityKey = "";
            var response = new RegisterResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        if (string.IsNullOrEmpty(registerRequest.Email))
                        {
                            response.Status = 101;
                            response.Message = "Email cannot be empty";
                            return response;
                        }
                        if (string.IsNullOrEmpty(registerRequest.Google_Id) && registerRequest.Login_Type == Login_Types.GOOGLE)
                        {
                            response.Status = 101;
                            response.Message = "Google Id cannot be empty";
                            return response;
                        }
                        if (string.IsNullOrEmpty(registerRequest.Facebook_Id) && registerRequest.Login_Type == Login_Types.FACEBOOK)
                        {
                            response.Status = 101;
                            response.Message = "Facebook Id cannot be empty";
                            return response;
                        }
                        if (string.IsNullOrEmpty(registerRequest.Apple_Id) && registerRequest.Login_Type == Login_Types.APPLE)
                        {
                            response.Status = 101;
                            response.Message = "Apple Id cannot be empty";
                            return response;
                        }
                        var appuserBC = new AppUserBC(context);
                        var entryBC = new DeviceBC(context);

                        var existingUser = appuserBC.GetAppUser(registerRequest.Email, registerRequest.Login_Type);
                        var deviceRegisterDM = entryBC.GetDeviceByClientKey(Base64Decode(securityKey));

                        if (existingUser == null)
                        {
                            var appuserDM = new App_Users();
                            appuserDM.Name = registerRequest.Name;
                            appuserDM.Email = registerRequest.Email;
                            appuserDM.Password = registerRequest.Password;
                            appuserDM.Mobile = registerRequest.Mobile;
                            appuserDM.Login_Type = registerRequest.Login_Type;
                            appuserDM.App_User_Type = App_User_Types.APP_USER;
                            appuserDM.Facebook_Id = registerRequest.Facebook_Id;
                            appuserDM.Google_Id = registerRequest.Google_Id;
                            appuserDM.Apple_Id = registerRequest.Apple_Id;
                            appuserDM.Created_Datetime = StaticMethods.GetKuwaitTime();
                            appuserDM.Show = true;
                            appuserDM.Row_Id = Guid.NewGuid();

                            appuserDM = appuserBC.CreateAppUser(appuserDM);

                            #region Update Device details


                            deviceRegisterDM.App_User_Id = appuserDM.App_User_Id;
                            deviceRegisterDM.Logged_In = StaticMethods.GetKuwaitTime();
                            deviceRegisterDM = entryBC.RegisterDevice(deviceRegisterDM);
                            #endregion

                            response.Status = 0;
                            response.App_User_Id = appuserDM.Row_Id.ToString();
                            response.Message = ServiceResponse.Success;
                        }

                        else
                        {
                            //response.Status = 3;
                            //response.App_User_Id = appuserDM.App_User_Id;
                            //response.Message = ServiceResponse.UserExist;

                            if (registerRequest.Login_Type == 1)
                            {
                                response.Status = 3;
                                response.Message = ServiceResponse.UserExist;
                            }
                            else
                            {
                                existingUser.Name = registerRequest.Name;
                                existingUser.Created_Datetime = StaticMethods.GetKuwaitTime();
                                existingUser.Facebook_Id = registerRequest.Facebook_Id;
                                existingUser.Google_Id = registerRequest.Google_Id;
                                existingUser = appuserBC.CreateAppUser(existingUser);
                                deviceRegisterDM.App_User_Id = existingUser.App_User_Id;
                                deviceRegisterDM.Logged_In = StaticMethods.GetKuwaitTime();
                                deviceRegisterDM = entryBC.RegisterDevice(deviceRegisterDM);
                                response.App_User_Id = existingUser.Row_Id.ToString();
                                response.Status = 0;
                                response.Message = ServiceResponse.Success;
                            }
                        }



                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Login")]
        [HttpPost]
        public RegisterResponse Login(LoginRequest loginRequest)
        {
            string securityKey = "";
            var response = new RegisterResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        if (string.IsNullOrEmpty(loginRequest.Email))
                        {
                            response.Status = 101;
                            response.Message = "Email cannot be empty";
                            return response;
                        }

                        var userBC = new App_Users();
                        var appuserBC = new AppUserBC(context);
                        var entryBC = new DeviceBC(context);
                        var user = appuserBC.ValidateAppUser(loginRequest.Email, loginRequest.Password);

                        if (user != null && user.App_User_Id != 0)
                        {
                            #region Update Device details

                            var deviceRegisterDM = entryBC.GetDeviceByClientKey(Base64Decode(securityKey));
                            deviceRegisterDM.App_User_Id = user.App_User_Id;
                            deviceRegisterDM.Logged_In = StaticMethods.GetKuwaitTime();
                            deviceRegisterDM = entryBC.RegisterDevice(deviceRegisterDM);
                            #endregion


                            response.Status = 0;
                            response.App_User_Id = user.Row_Id.ToString();
                            response.App_User_Name = user.Name;
                            response.Message = ServiceResponse.Success;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "Invalid Password";
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Logout")]
        [HttpPost]
        public GeneralResponse Logout()
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var entryBC = new DeviceBC(context);
                        var appuserBC = new AppUserBC(context);
                        var deviceDM = entryBC.GetDevice(securityDTO.Device_Id);

                        deviceDM.App_User_Id = null;
                        deviceDM.Logged_In = null;
                        deviceDM.Mobile = null;
                        deviceDM.Code = null;
                        var isLogout = entryBC.Logout(deviceDM);

                        if (isLogout.App_User_Id == null && isLogout.Logged_In == null)
                        {
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "Error Login out";
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }


        [Route("Labels")]
        [HttpGet]
        public LabelResponse GetLabels()
        {
            var response = new LabelResponse();
            try
            {
                var promterBC = new CommonBC(context, logPath);
                var currentevents = promterBC.GetAppLabels();

                foreach (var currentevent in currentevents)
                {
                    var eventsDTO = new LabelDTO();
                    eventsDTO.Label_Id = currentevent.Label_Id;
                    eventsDTO.Label_Name_E = currentevent.L_Label_Name;
                    eventsDTO.Label_Name_A = currentevent.A_Label_Name ?? "";
                    eventsDTO.Label_Code = currentevent.Label_Code ?? "";
                    response.Labels.Add(eventsDTO);
                }

                //var emailSent = SendHTMLMail("yusuf.9116@gmail.com","Test Email","<strong>This is test email</strong>","","", "chocopedia.map@gmail.com", "scsvwozhinefmzre", "Cacaoo");
                response.Message = ServiceResponse.Success;
                response.Status = 0;

            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Categories")]
        [HttpGet]
        public CategoryResponse GetCategories()
        {
            string securityKey = "";
            var response = new CategoryResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var categoryBC = new CategoryBC(context);
                        var currentevents = categoryBC.GetCategories();
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        foreach (var currentevent in currentevents)
                        {
                            var eventsDTO = new CategoryDTO();
                            eventsDTO.Category_Id = currentevent.Category_Id;
                            eventsDTO.Category_Name = lang == "A" ? currentevent.Category_Name_A ?? currentevent.Category_Name_E : currentevent.Category_Name_E;
                            eventsDTO.Image_URL = currentevent.Image_URL ?? "";
                            eventsDTO.Background_Color = currentevent.Background_Color ?? "";
                            var sub_categories = categoryBC.GetSubCategories(currentevent.Category_Id);
                            foreach (SM_Sub_Categories sub_category in sub_categories)
                            {
                                var subCategoryDTO = new SubCategoryDTO();
                                subCategoryDTO.Sub_Category_Id = sub_category.Sub_Category_Id;
                                subCategoryDTO.Sub_Category_Name = lang == "A" ? sub_category.Sub_Category_Name_A ?? sub_category.Sub_Category_Name_E : sub_category.Sub_Category_Name_E;
                                subCategoryDTO.Image_URL = sub_category.Image_URL ?? "";
                                subCategoryDTO.Background_Color = sub_category.Background_Color ?? "";
                                eventsDTO.SubCategories.Add(subCategoryDTO);
                            }

                            response.Categories.Add(eventsDTO);
                        }


                        response.Message = ServiceResponse.Success;
                        response.Status = 0;
                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("SubCategories/{cat_id}")]
        [HttpGet]
        public SubCategoryResponse GetSubCategories(int cat_id)
        {
            string securityKey = "";
            var response = new SubCategoryResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var categoryBC = new CategoryBC(context);

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        var sub_categories = categoryBC.GetSubCategories(cat_id);
                        foreach (SM_Sub_Categories sub_category in sub_categories)
                        {
                            var subCategoryDTO = new SubCategoryDTO();
                            subCategoryDTO.Sub_Category_Id = sub_category.Sub_Category_Id;
                            subCategoryDTO.Sub_Category_Name = lang == "A" ? sub_category.Sub_Category_Name_A ?? sub_category.Sub_Category_Name_E : sub_category.Sub_Category_Name_E;
                            subCategoryDTO.Image_URL = sub_category.Image_URL ?? "";
                            subCategoryDTO.Background_Color = sub_category.Background_Color ?? "";
                            response.SubCategories.Add(subCategoryDTO);
                        }


                        response.Message = ServiceResponse.Success;
                        response.Status = 0;
                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Areas")]
        [HttpGet]
        public AreaResponse GetAreas()
        {
            string securityKey = "";
            var response = new AreaResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var areaBC = new AreaBC(context);
                        var currentevents = areaBC.GetAreas();
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        foreach (var currentevent in currentevents)
                        {
                            var eventsDTO = new AreaDTO();
                            eventsDTO.Area_Id = currentevent.Area_Id;
                            eventsDTO.Area_Name = lang == "A" ? currentevent.Area_Name_A ?? currentevent.Area_Name_E : currentevent.Area_Name_E;
                            response.Areas.Add(eventsDTO);
                        }

                        // var jwt = GoogleOAuthUtility.CreateJwtForFirebaseMessaging();
                        response.Message = ServiceResponse.Success;
                        response.Status = 0;
                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }


        [Route("GetHomePage")]
        [HttpGet]
        public HomePageResponse GetHomePage()
        {
            string securityKey = "";
            var response = new HomePageResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var carouselBC = new CarouselBC(context);
                        var carousels = carouselBC.GetCarousels();
                        foreach (var currentevent in carousels)
                        {
                            var eventsDTO = new CarouselDTO();
                            eventsDTO.Carousel_Id = currentevent.Carousel_Id;
                            eventsDTO.Carousel_Name = lang.ToUpper() == "A" ? currentevent.Title_A : !string.IsNullOrEmpty(currentevent.Title_E) ? currentevent.Title_E : currentevent.Title_A;
                            eventsDTO.Carousel_Title = lang.ToUpper() == "A" ? currentevent.Sub_Title_A : !string.IsNullOrEmpty(currentevent.Sub_Title_E) ? currentevent.Sub_Title_E : currentevent.Sub_Title_A;
                            eventsDTO.Media_Type = currentevent.Media_Type;
                            eventsDTO.Media_From_Type = currentevent.Media_From_Type ?? 0;
                            eventsDTO.Redirect_Id = currentevent.Redirect_Id ?? 0;
                            eventsDTO.Media_Url = currentevent.Media_URL ?? "";
                            eventsDTO.ThumbNail_Url = currentevent.Thumbnail_URL ?? "";
                            response.Carousels.Add(eventsDTO);
                        }
                        var groupBC = new HomeGroupBC(context);
                        var groups = groupBC.GetGroups();

                        var i = 0;
                        foreach (var group in groups)
                        {
                            var groupDM = new GroupDTO();
                            groupDM.Group_Id = group.Group_Id;
                            groupDM.Group_Name = lang.ToUpper() == "A" ? group.Group_Name_A : !string.IsNullOrEmpty(group.Group_Name_E) ? group.Group_Name_E : group.Group_Name_A;
                            groupDM.Display_Type = group.Display_Type;

                            #region Group Items


                            var groupItems = groupBC.GetGroupDetails(group.Group_Id);
                            groupItems = (from o in groupItems
                                          orderby o.Sequence
                                          select o).ToList();


                            foreach (var currentevent in groupItems)
                            {
                                var eventsDTO = new GeneralDTO();
                                eventsDTO.Id = currentevent.Id;
                                eventsDTO.Group_Type_Id = currentevent.Group_Type_Id;
                                eventsDTO.Name = currentevent.Item_Name;
                                eventsDTO.Image_Url = currentevent.Image_Url ?? "";
                                groupDM.GroupItems.Add(eventsDTO);

                            }

                            #endregion

                            response.Groups.Add(groupDM);
                            i++;

                        }

                        response.Message = ServiceResponse.Success;
                        response.Status = 0;
                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }


        [Route("GroupItems/{groupTypeId}")]
        [HttpGet]
        public CustomerInvoicesResponse GetGroupItems(short groupTypeId)
        {
            var response = new CustomerInvoicesResponse();
            try
            {
                var invoiceBC = new HomeGroupBC(context);
                var propertyUnits = invoiceBC.GetGroupItems(groupTypeId);
                var emptyselect2dto = new Select2DTO();
                emptyselect2dto.id = "";
                emptyselect2dto.text = "";
                response.results.Add(emptyselect2dto);
                response.results.AddRange(propertyUnits);
                response.Message = ServiceResponse.Success;
                response.Status = 0;
            }
            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }
            return response;
        }

        [Route("Products")]
        [HttpPost]
        public ProductsResponse GetProducts(ProductRequest productRequest)
        {
            string securityKey = "";
            var response = new ProductsResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var postBC = new ProductBC(context);
                        var posts = postBC.GetAppProducts(productRequest);


                        foreach (var currentevent in posts.Items)
                        {
                            var eventsDTO = new ProductDTO();
                            eventsDTO.Product_Id = currentevent.Product_Id;
                            eventsDTO.Product_Name = lang.ToUpper() == "E" ? currentevent.Product_Name_E : currentevent.Product_Name_A ?? "";
                            eventsDTO.Product_Desc = lang.ToUpper() == "E" ? currentevent.Product_Desc_E ?? "" : currentevent.Product_Desc_A ?? "";
                            eventsDTO.Price = currentevent.Price;
                            eventsDTO.Image_Url = currentevent.Image_URL ?? "";
                            eventsDTO.Brand_Name = lang.ToUpper() == "E" ? currentevent.Brand_Name_E ?? "" : currentevent.Brand_Name_A ?? "";
                            eventsDTO.Is_Exclusive = currentevent.Is_Exclusive;
                            eventsDTO.Is_Catering = currentevent.Is_Catering;
                            response.Products.Add(eventsDTO);
                        }
                        response.Total_Products = posts.Total_Items;
                        response.Message = ServiceResponse.Success;
                        response.Status = 0;


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Brands")]
        [HttpPost]
        public BrandsResponse GetBrands(ProductRequest productRequest)
        {
            string securityKey = "";
            var response = new BrandsResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var postBC = new BrandBC(context);
                        var posts = postBC.GetBrands(productRequest);


                        foreach (var currentevent in posts)
                        {
                            var eventsDTO = new BrandDTO();
                            eventsDTO.Brand_Id = currentevent.Restaurant_Id;
                            eventsDTO.Brand_Name = lang.ToUpper() == "E" ? currentevent.Restaurant_Name_E : currentevent.Restaurant_Name_A ?? "";
                            eventsDTO.Image_Url = currentevent.Image_URL ?? "";
                            eventsDTO.Delivery_Time = currentevent.Delivery_Time ?? "";
                            eventsDTO.Delivery_Charge = currentevent.Delivery_Charge;
                            eventsDTO.Categories = currentevent.Categories;
                            eventsDTO.Background_Color = currentevent.Background_Color ?? "";
                            response.Brands.Add(eventsDTO);
                        }

                        response.Message = ServiceResponse.Success;
                        response.Status = 0;


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Brand/{brandId}")]
        [HttpGet]
        public BrandDetailResponse GetBrandDetailResponse(int brandId)
        {
            string securityKey = "";
            var response = new BrandDetailResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var restaurantBC = new RestaurantBC(context);
                        var brandDM = restaurantBC.GetRestaurant(brandId);

                        BrandBC brandBC = new BrandBC(context);

                        if (brandDM != null)
                        {
                            response.Brand_Name = lang.ToUpper() == "E" ? brandDM.Restaurant_Name_E : brandDM.Restaurant_Name_A ?? "";
                            response.Brand_Desc = lang.ToUpper() == "E" ? brandDM.Restaurant_Desc_E ?? "" : brandDM.Restaurant_Desc_A ?? "";
                            response.Image_Url = brandDM.Image_URL ?? "";
                            response.Delivery_Charge = brandDM.Delivery_Charge;
                            response.Delivery_Time = brandDM.Delivery_Time ?? "";
                            var categories = brandBC.GetBrandCategories(brandDM.Restaurant_Id);
                            if (lang == "A")
                            {
                                response.Brand_Categories = string.Join(",", categories.Select(x => x.Sub_Category_Name_E).ToList());
                            }
                            else
                            {
                                response.Brand_Categories = string.Join(",", categories.Select(x => x.Sub_Category_Name_A).ToList());
                            }
                            foreach (var cat in categories)
                            {
                                var categoryDTO = new BrandCategoryDTO();
                                categoryDTO.Category_Id = cat.Category_Id;
                                categoryDTO.Category_Name = lang.ToUpper() == "E" ? cat.Sub_Category_Name_E : cat.Sub_Category_Name_A ?? "";
                                var products = brandBC.GetBrandCategoryProducts(brandId, cat.Sub_Category_Id);
                                foreach (var product in products)
                                {
                                    var productDTO = new ProductDTO();
                                    productDTO.Product_Id = product.Product_Id;
                                    productDTO.Product_Name = lang.ToUpper() == "E" ? product.Product_Name_E : product.Product_Name_A ?? "";
                                    productDTO.Product_Desc = lang.ToUpper() == "E" ? product.Product_Desc_E ?? "" : product.Product_Desc_A ?? "";
                                    productDTO.Price = product.Price;
                                    productDTO.Image_Url = product.Image_URL ?? "";
                                    productDTO.Is_Exclusive = product.Is_Exclusive;
                                    productDTO.Is_Catering = product.Is_Catering;
                                    categoryDTO.Products.Add(productDTO);
                                }
                                response.Categories.Add(categoryDTO);
                            }
                            response.Message = ServiceResponse.Success;
                            response.Status = 0;
                        }
                        else
                        {
                            response.Message = "Brand Not Found";
                            response.Status = 101;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Product/{productId}/{app_user_id?}")]
        [HttpGet]
        public ProductDetailResponse GetProductDetailResponse(int productId, string? app_user_id = "0")
        {
            string securityKey = "";
            var response = new ProductDetailResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        var actual_app_user_id = 0;
                        if (app_user_id != "0")
                        {
                            AppUserBC appUserBC = new AppUserBC(context);
                            var row_id = new Guid(app_user_id);
                            var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                            if (appUserDM != null)
                            {
                                actual_app_user_id = appUserDM.App_User_Id;
                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "App User Not Found";
                                return response;
                            }
                        }


                        var brandBC = new ProductBC(context);
                        var brandDM = brandBC.GetProduct(productId, actual_app_user_id);

                        if (brandDM != null)
                        {
                            response.Product_Name = lang.ToUpper() == "E" ? brandDM.Product_Name_E : brandDM.Product_Name_A ?? "";
                            response.Product_Desc = lang.ToUpper() == "E" ? brandDM.Product_Desc_E ?? "" : brandDM.Product_Desc_A ?? "";
                            response.Short_Desc = lang.ToUpper() == "E" ? brandDM.Short_Desc_E ?? "" : brandDM.Short_Desc_A ?? "";
                            response.Nutritional_Facts = lang.ToUpper() == "E" ? brandDM.Nutritional_Facts_E ?? "" : brandDM.Nutritional_Facts_A ?? "";
                            response.Image_Url = brandDM.Image_URL ?? "";
                            response.Price = brandDM.Price;
                            response.Avg_Rating = brandDM.Rating;
                            response.Total_Ratings = brandDM.Total_Ratings;
                            response.Is_Favorite = brandDM.Is_Favorite;
                            response.Is_Exclusive = brandDM.Is_Exclusive;
                            response.Is_Catering = brandDM.Is_Catering;
                            var addons = brandBC.GetProductAddOns(brandDM.Product_Id, lang);
                            response.AddOns.AddRange(addons);
                            var cateringProducts = brandBC.GetProductCateringProducts(brandDM.Product_Id, lang);
                            response.Catering_Categories.AddRange(cateringProducts);

                            response.Message = ServiceResponse.Success;
                            response.Status = 0;
                        }
                        else
                        {
                            response.Message = "Product Not Found";
                            response.Status = 101;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Chef/{chefId}")]
        [HttpGet]
        public ChefDetailResponse GetChefDetailResponse(int chefId)
        {
            string securityKey = "";
            var response = new ChefDetailResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var chefBC = new ChefBC(context);
                        var chefDM = chefBC.GetChef(chefId);

                        if (chefDM != null)
                        {
                            response.Chef_Name = lang.ToUpper() == "E" ? chefDM.Chef_Name_E : chefDM.Chef_Name_A ?? "";
                            response.Chef_Desc = lang.ToUpper() == "E" ? chefDM.Chef_Desc_E ?? "" : chefDM.Chef_Desc_A ?? "";
                            response.Image_Url = chefDM.Image_URL ?? "";


                            var products = chefBC.GetChefProducts(chefId);
                            foreach (var currentevent in products.Items)
                            {
                                var eventsDTO = new ProductDTO();
                                eventsDTO.Product_Id = currentevent.Product_Id;
                                eventsDTO.Product_Name = lang.ToUpper() == "E" ? currentevent.Product_Name_E : currentevent.Product_Name_A ?? "";
                                eventsDTO.Product_Desc = lang.ToUpper() == "E" ? currentevent.Product_Desc_E ?? "" : currentevent.Product_Desc_A ?? "";
                                eventsDTO.Price = currentevent.Price;
                                eventsDTO.Image_Url = currentevent.Image_URL ?? "";
                                eventsDTO.Brand_Name = lang.ToUpper() == "E" ? currentevent.Brand_Name_E ?? "" : currentevent.Brand_Name_A ?? "";
                                response.Products.Add(eventsDTO);
                            }

                            response.Message = ServiceResponse.Success;
                            response.Status = 0;
                        }
                        else
                        {
                            response.Message = "Chef Not Found";
                            response.Status = 101;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("AddToCart")]
        [HttpPost]
        public AddCartResponse AddToCart(CartRequest cartRequest)
        {
            string securityKey = "";
            var response = new AddCartResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(cartRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var cartBC = new CartBC(context);

                        var row_id = new Guid(cartRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            if (cartRequest.Cart_Id != 0)
                            {
                                var cartExist = cartBC.GetCart(cartRequest.Cart_Id);
                                if (cartExist == null)
                                {
                                    response.Status = 101;
                                    response.Message = "Cart Id not found";
                                    return response;
                                }
                            }
                            else
                            {
                                /*var cartExist = cartBC.GetCart(appUserDM.App_User_Id, cartRequest.Product_Id);
                                if (cartExist != null)
                                {
                                    response.Status = 101;
                                    response.Message = "This item is already added in cart. Kindly update the Qty";
                                    return response;
                                }*/
                            }
                            var isValidVendor = cartBC.IsValidVendor(appUserDM.App_User_Id, cartRequest.Product_Id);
                            if (!isValidVendor)
                            {
                                response.Status = 101;
                                response.Message = "You cannot add product from different vendor in cart";
                                return response;
                            }
                            var isValidCategory = cartBC.IsValidCategory(appUserDM.App_User_Id, cartRequest.Product_Id);
                            if (!isValidCategory)
                            {
                                response.Status = 101;
                                response.Message = "You cannot add gift and normal item together in cart";
                                return response;
                            }
                            var cartDM = new TXN_Cart();
                            cartDM.Cart_Id = cartRequest.Cart_Id;
                            cartDM.App_User_Id = appUserDM.App_User_Id;
                            cartDM.Product_Id = cartRequest.Product_Id;
                            cartDM.Qty = cartRequest.Qty;
                            cartDM.Created_Datetime = StaticMethods.GetKuwaitTime();
                            cartDM.Comments = cartRequest.Comments;
                            cartBC.AddtoCart(cartDM);

                            if (cartRequest.Cart_Id == 0)
                            {
                                foreach (var addonId in cartRequest.Product_AddOnIds)
                                {
                                    var addonDM = new TXN_Cart_AddOns();
                                    addonDM.Product_AddOnId = addonId;
                                    addonDM.Cart_Id = cartDM.Cart_Id;
                                    cartBC.CreateCartAddOn(addonDM);
                                }
                                foreach (var cateringProduct in cartRequest.Catering_Products)
                                {
                                    var addonDM = new TXN_Cart_Catering_Products();
                                    addonDM.Catering_Product_Id = cateringProduct.Catering_Product_Id;
                                    addonDM.Qty = cateringProduct.Qty;
                                    addonDM.Cart_Id = cartDM.Cart_Id;
                                    cartBC.CreateCartCateringProduct(addonDM);
                                }
                            }

                            response.Status = 0;
                            response.Cart_Id = cartDM.Cart_Id;
                            response.Message = ServiceResponse.Success;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Cart/{app_user_id}")]
        [HttpGet]
        public CartResponse GetCart(string app_user_id)
        {
            string securityKey = "";
            var response = new CartResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var cartBC = new CartBC(context);

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(app_user_id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);


                        var row_id = new Guid(app_user_id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            response = cartBC.GetCartItems(appUserDM.App_User_Id, lang);
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("RemoveCartItem")]
        [HttpPost]
        public GeneralResponse RemoveCartItem(CartRequest cartRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        if (string.IsNullOrEmpty(cartRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        if (cartRequest.Cart_Id == 0)
                        {
                            response.Status = 101;
                            response.Message = "Cart Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var cartBC = new CartBC(context);

                        var row_id = new Guid(cartRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var cartExist = cartBC.GetCart(cartRequest.Cart_Id);
                            if (cartExist == null)
                            {
                                response.Status = 101;
                                response.Message = "Cart Id not found";
                                return response;
                            }
                            else
                            {
                                var isRemoved = cartBC.RemoveCartItem(cartRequest.Cart_Id);
                                if (isRemoved)
                                {
                                    response.Status = 0;
                                    response.Message = ServiceResponse.Success;
                                }
                                else
                                {
                                    response.Status = 101;
                                    response.Message = "Due to technical error product not removed";
                                    return response;
                                }

                            }
                        }





                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("User/{app_user_id}")]
        [HttpGet]
        public ProfileResponse GetUserProfile(string app_user_id)
        {
            string securityKey = "";
            var response = new ProfileResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        AppUserBC appUserBC = new AppUserBC(context);

                        var row_id = new Guid(app_user_id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            response.Name = appUserDM.Name;
                            response.Mobile = appUserDM.Mobile;
                            response.Email = appUserDM.Email;
                            response.Login_Type = appUserDM.Login_Type;
                            response.Redeem_Points = appUserDM.Current_Points;
                            response.Plate_Num = appUserDM.Plate_Num;
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("UpdateProfile")]
        [HttpPost]
        public GeneralResponse UpdateProfile(UpdatePofileRequest updatePofileRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(updatePofileRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var cartBC = new CartBC(context);

                        var row_id = new Guid(updatePofileRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            appUserDM.Name = updatePofileRequest.Name;
                            if (updatePofileRequest.Mobile.Length == 8)
                            {
                                updatePofileRequest.Mobile = "+965" + updatePofileRequest.Mobile;
                            }
                            appUserDM.Mobile = updatePofileRequest.Mobile;
                            appUserBC.CreateAppUser(appUserDM);

                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("UpdatePassword")]
        [HttpPost]
        public GeneralResponse UpdatePassword(UpdatePasswordRequest updatePasswordRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(updatePasswordRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var cartBC = new CartBC(context);

                        var row_id = new Guid(updatePasswordRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {

                            if (appUserDM.Password != updatePasswordRequest.Old_Password)
                            {
                                response.Status = 101;
                                response.Message = "Old Password not matches with existing password";
                                return response;
                            }

                            appUserDM.Password = updatePasswordRequest.New_Password;
                            appUserBC.CreateAppUser(appUserDM);
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("UpdateUserAddress")]
        [HttpPost]
        public UpdateAddressResponse UpdateUserAddress(UpdateAddressRequest updateAddressRequest)
        {
            string securityKey = "";
            var response = new UpdateAddressResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(updateAddressRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        if (updateAddressRequest.Area_Id == 0)
                        {
                            response.Status = 101;
                            response.Message = "Please Select Area";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var cartBC = new CartBC(context);

                        var row_id = new Guid(updateAddressRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            App_User_Address addressDM = new App_User_Address();
                            addressDM.Address_Id = updateAddressRequest.Address_Id;
                            addressDM.App_User_Id = appUserDM.App_User_Id;
                            addressDM.Address_Name = updateAddressRequest.Address_Name;
                            addressDM.Block = updateAddressRequest.Block;
                            addressDM.Street = updateAddressRequest.Street;
                            addressDM.Building = updateAddressRequest.Building;
                            addressDM.Avenue = updateAddressRequest.Avenue;
                            addressDM.Floor = updateAddressRequest.Floor;
                            addressDM.Apartment = updateAddressRequest.Apartment;
                            addressDM.Area_Id = updateAddressRequest.Area_Id;
                            addressDM.Mobile = updateAddressRequest.Mobile;
                            addressDM.Extra_Direction = updateAddressRequest.Extra_Direction;
                            addressDM.House_No = updateAddressRequest.House_No;
                            addressDM.Latitude = updateAddressRequest.Latitude;
                            addressDM.Longitude = updateAddressRequest.Longitude;
                            addressDM.Paci_Number = updateAddressRequest.Paci_Number;
                            addressDM.Created_Datetime = StaticMethods.GetKuwaitTime();
                            addressDM.Updated_Datetime = StaticMethods.GetKuwaitTime();
                            appUserBC.CreateAppUserAddress(addressDM);

                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                            response.Address_Id = addressDM.Address_Id;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("UserAddresses/{app_user_id}")]
        [HttpGet]
        public UserAddressResponse GetUserAddresses(string app_user_id)
        {
            string securityKey = "";
            var response = new UserAddressResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var cartBC = new CartBC(context);

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(app_user_id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);


                        var row_id = new Guid(app_user_id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var addresses = appUserBC.GetUserAddresses(appUserDM.App_User_Id);
                            foreach (var address in addresses)
                            {
                                var addressDTO = new AddressDTO();
                                addressDTO.Address_Id = address.Address_Id;
                                addressDTO.Address_Name = address.Address_Name;
                                addressDTO.Block = address.Block;
                                addressDTO.Street = address.Street;
                                addressDTO.Building = address.Building;
                                addressDTO.Avenue = address.Avenue ?? "";
                                addressDTO.Floor = address.Floor ?? "";
                                addressDTO.Apartment = address.Apartment ?? "";
                                addressDTO.Area_Id = address.Area_Id;
                                addressDTO.Mobile = address.Mobile ?? "";
                                addressDTO.Extra_Direction = address.Extra_Direction ?? "";
                                addressDTO.House_No = address.House_No ?? "";
                                addressDTO.Area_Name = address.Area_Name;
                                addressDTO.Latitude = address.Latitude;
                                addressDTO.Longitude = address.Longitude;
                                addressDTO.Paci_Number = address.Paci_Number ?? "";
                                response.UserAddresses.Add(addressDTO);
                            }
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                            //return response;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }

            return response;
        }

        [Route("DeleteUserAddress/{userAddressId}")]
        [HttpPost]
        public GeneralResponse DeleteUserAddress(long userAddressId)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        AppUserBC appUserBC = new AppUserBC(context);
                        var deleteAddress = appUserBC.DeleteUserAddress(userAddressId);
                        if (deleteAddress)
                        {
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "Due to some error, address not deleted";
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("PaymentTypes")]
        [HttpGet]
        public PaymentTypeResponse GetPaymentTypes()
        {
            string securityKey = "";
            var response = new PaymentTypeResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var areaBC = new OrderBC(context);
                        var currentevents = areaBC.GetPaymentTypes();
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        foreach (var currentevent in currentevents)
                        {
                            var eventsDTO = new PaymentTypeDTO();
                            eventsDTO.Type_Id = currentevent.Payment_Type_Id;
                            eventsDTO.Type_Name = lang == "A" ? currentevent.Payment_Type_Name_A ?? currentevent.Payment_Type_Name_E : currentevent.Payment_Type_Name_E;
                            response.PaymentTypes.Add(eventsDTO);
                        }


                        response.Message = ServiceResponse.Success;
                        response.Status = 0;
                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("SaveOrder")]
        [HttpPost]
        public OrderResponse SaveOrder(OrderRequest orderRequest)
        {
            string securityKey = "";
            var response = new OrderResponse();
            using (var scope = new TransactionScope())
            {
                try
                {
                    if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                    {
                        response.Status = 105;
                        response.Message = ServiceResponse.NoSecurityKey;
                    }
                    else
                    {
                        securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                        //Check session token
                        bool isAuthorized = ValidateSecurityKey(securityKey);

                        if (isAuthorized == false)
                        {
                            response.Status = 106;
                            response.Message = ServiceResponse.Unauthorized;
                        }
                        else
                        {
                            globalCls.WriteToFile(logPath, "Order Request:" + JsonConvert.SerializeObject(orderRequest));
                            var lang = "E";
                            if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                            {
                                lang = "A";
                            }
                            if (string.IsNullOrEmpty(orderRequest.App_User_Id))
                            {
                                response.Status = 101;
                                response.Message = "App User Id cannot be empty";
                                return response;
                            }
                            if (orderRequest.Delivery_Type == Delivery_Types.DELIVERY && (orderRequest.Address_Id == null || orderRequest.Address_Id == 0))
                            {

                                response.Status = 101;
                                response.Message = "Please select the address";
                                return response;
                            }
                            if (orderRequest.OrderDetails.Count == 0)
                            {
                                response.Status = 101;
                                response.Message = "Order Details cannot be empty";
                                return response;
                            }
                            if (orderRequest.Branch_Id == 0)
                            {
                                response.Status = 101;
                                response.Message = "Branch cannot be empty";
                                return response;
                            }
                            if (orderRequest.Delivery_Type == Delivery_Types.PICKUP && string.IsNullOrEmpty(orderRequest.Pickup_Datetime))
                            {

                                response.Status = 101;
                                response.Message = "Please choose pick up time";
                                return response;
                            }
                            AppUserBC appUserBC = new AppUserBC(context);
                            var cartBC = new CartBC(context);
                            ProductBC productBC = new ProductBC(context);
                            RestaurantBC restaurantBC = new RestaurantBC(context);
                            OrderBC orderBC = new OrderBC(context);

                            var firstprodDM = productBC.GetProduct(orderRequest.OrderDetails.FirstOrDefault().Prod_Id);
                            if (orderRequest.Delivery_Type == Delivery_Types.DELIVERY && firstprodDM.Is_Gift_Product && string.IsNullOrEmpty(orderRequest.Pickup_Datetime))
                            {

                                response.Status = 101;
                                response.Message = "Please choose delivery time";
                                return response;
                            }

                            var row_id = new Guid(orderRequest.App_User_Id);
                            var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                            if (appUserDM != null)
                            {
                                short order_type = OrderTypes.NORMAL;
                                var gross_amt = decimal.Zero;
                                var delivery_charge = decimal.Zero;
                                var redeem_amt = orderRequest.Redeem_Amount ?? 0;
                                SM_Restaurants restaurantDM = new SM_Restaurants();
                                foreach (var detail in orderRequest.OrderDetails)
                                {
                                    var prodDM = productBC.GetProduct(detail.Prod_Id);
                                    if (prodDM == null)
                                    {
                                        response.Status = 101;
                                        response.Message = "Some or all products not found";
                                        return response;
                                    }
                                    else
                                    {
                                        restaurantDM = restaurantBC.GetRestaurant(prodDM.Restaurant_Id);
                                        if (orderRequest.Delivery_Type == Delivery_Types.DELIVERY)
                                            delivery_charge = restaurantDM.Delivery_Charge;

                                        gross_amt += prodDM.Price * detail.Qty;
                                        detail.Gross_Amount = prodDM.Price * detail.Qty;
                                        detail.Amount = prodDM.Price * detail.Qty;
                                        detail.Rate = prodDM.Price;
                                        detail.Prod_Name = prodDM.Product_Name_E;
                                        if (prodDM.Is_Gift_Product)
                                        {
                                            order_type = OrderTypes.GIFT;
                                        }
                                        foreach (var addon_id in detail.Product_AddOn_Ids)
                                        {
                                            var addOnDM = productBC.GetProductAddOn(addon_id);
                                            if (addOnDM == null)
                                            {
                                                response.Status = 101;
                                                response.Message = "Invalid AddonId for product :" + prodDM.Product_Name_E;
                                                return response;
                                            }
                                            else
                                            {
                                                gross_amt += addOnDM.Price;
                                                detail.Gross_Amount = detail.Gross_Amount + addOnDM.Price;
                                            }
                                        }
                                        foreach (var category_product in detail.Catering_Products)
                                        {
                                            var cateringProductDM = productBC.GetProductCateringProduct(category_product.Catering_Product_Id);
                                            if (cateringProductDM == null)
                                            {
                                                response.Status = 101;
                                                response.Message = "Invalid Catering_Product_Id : " + category_product.Catering_Product_Id + " for product :" + prodDM.Product_Name_E;
                                                return response;
                                            }
                                            else if (cateringProductDM.Product_Id != detail.Prod_Id)
                                            {
                                                response.Status = 101;
                                                response.Message = " Catering_Product_Id : " + category_product.Catering_Product_Id + " does not belong to product :" + prodDM.Product_Name_E;
                                                return response;
                                            }
                                        }
                                    }
                                }

                                var net_amt = gross_amt + delivery_charge - redeem_amt;

                                var orderDM = new TXN_Orders();
                                orderDM.Order_No = orderBC.GetNextOrderNo();
                                orderDM.Order_Serial = "ORD_" + orderDM.Order_No.ToString("D6");
                                orderDM.Order_Datetime = StaticMethods.GetKuwaitTime();
                                orderDM.App_User_Id = appUserDM.App_User_Id;
                                orderDM.Address_Id = orderRequest.Address_Id;
                                orderDM.Payment_Type_Id = orderRequest.Payment_Type_Id;
                                orderDM.Channel_Id = orderRequest.Channel_Id;
                                orderDM.Delivery_Type = orderRequest.Delivery_Type;
                                orderDM.Comments = orderRequest.Remarks;
                                orderDM.Cust_Name = orderRequest.Cust_Name;
                                orderDM.Email = orderRequest.Email;
                                orderDM.Mobile = orderRequest.Mobile;
                                orderDM.Gross_Amount = gross_amt;
                                orderDM.Discount_Amount = 0;
                                orderDM.Delivery_Charges = delivery_charge;
                                orderDM.Net_Amount = net_amt;
                                if (orderRequest.Payment_Type_Id == PaymentTypes.Cash)
                                {
                                    //orderDM.Status_Id = OrderStatus.ORDER_RECEIVED;
                                    orderDM.Status_Id = OrderStatus.ORDER_PREPARING;
                                }
                                else if (orderRequest.Payment_Type_Id == PaymentTypes.KNET || orderRequest.Payment_Type_Id == PaymentTypes.CreditCard || orderRequest.Payment_Type_Id == PaymentTypes.ApplePay)
                                {
                                    orderDM.Status_Id = OrderStatus.ORDER_PROCESSING_PAYMENT;
                                }
                                orderDM.Promo_Code = orderRequest.Promo_Code;
                                orderDM.Restaurant_Id = restaurantDM.Restaurant_Id;
                                orderDM.Row_Id = Guid.NewGuid();
                                orderDM.Branch_Id = orderRequest.Branch_Id;
                                orderDM.Order_Type = order_type;
                                orderDM.Gift_Msg = orderRequest.Gift_Msg;
                                orderDM.Recepient_Name = orderRequest.Recepient_Name;
                                orderDM.Recepient_Mobile = orderRequest.Recepient_Mobile;
                                orderDM.Video_Link = orderRequest.Video_Link;
                                orderDM.Show_Sender_Name = true;
                                if (orderRequest.Show_Sender_Name != null)
                                {
                                    orderDM.Show_Sender_Name = orderRequest.Show_Sender_Name ?? true;
                                }
                                if (!string.IsNullOrEmpty(orderRequest.Video_File))
                                {
                                    byte[] bytes = Convert.FromBase64String(orderRequest.Video_File);
                                    var video_path_dir = "assets/videos/";
                                    var fileName = StaticMethods.GetKuwaitTime().ToString("yyyyMMddHHmmssfff") + ".mp4";
                                    var path = Path.Combine(this.iwebHostEnvironment.WebRootPath, video_path_dir);
                                    var filePath = Path.Combine(path, fileName);
                                    System.IO.File.WriteAllBytes(filePath, bytes);

                                    orderDM.Video_File_Path = video_path_dir + fileName;
                                }
                                orderDM.Redeem_Amount = redeem_amt;
                                orderDM.Redeem_Points = orderRequest.Redeem_Points ?? 0;
                                if (!string.IsNullOrEmpty(orderRequest.Pickup_Datetime))
                                {
                                    orderDM.Pickup_Datetime = DateTime.ParseExact(orderRequest.Pickup_Datetime, "dd-MMM-yyyy hh:mm tt", CultureInfo.InvariantCulture);
                                }
                                orderBC.SaveOrder(orderDM);

                                foreach (var detail in orderRequest.OrderDetails)
                                {
                                    var orderDetailDM = new TXN_Order_Details();
                                    orderDetailDM.Order_Id = orderDM.Order_Id;
                                    orderDetailDM.Product_Id = detail.Prod_Id;
                                    orderDetailDM.Product_Name = detail.Prod_Name;
                                    orderDetailDM.Qty = detail.Qty;
                                    orderDetailDM.Rate = detail.Rate;
                                    orderDetailDM.Amount = detail.Amount;
                                    orderDetailDM.Gross_Amount = detail.Gross_Amount;
                                    orderDetailDM.Discount_Amount = 0;
                                    orderDetailDM.Net_Amount = orderDetailDM.Gross_Amount - orderDetailDM.Discount_Amount;
                                    orderDetailDM.Promo_Code = detail.Promo_Code;
                                    orderDetailDM.Comments = detail.Remarks;
                                    orderBC.SaveOrderDetail(orderDetailDM);

                                    foreach (var addon_id in detail.Product_AddOn_Ids)
                                    {
                                        var addOnDM = productBC.GetProductAddOn(addon_id);
                                        TXN_Order_Detail_AddOns addOns = new TXN_Order_Detail_AddOns();
                                        addOns.Order_Detail_Id = orderDetailDM.Order_Detail_Id;
                                        addOns.Product_AddOnId = addon_id;
                                        addOns.Price = addOnDM.Price;
                                        orderBC.SaveOrderDetailAddon(addOns);
                                    }
                                    foreach (var catering_product in detail.Catering_Products)
                                    {

                                        TXN_Order_Detail_Catering_Products addOns = new TXN_Order_Detail_Catering_Products();
                                        addOns.Detail_Id = orderDetailDM.Order_Detail_Id;
                                        addOns.Category_Product_Id = catering_product.Catering_Product_Id;
                                        addOns.Qty = catering_product.Qty;
                                        orderBC.SaveOrderDetailCateringProduct(addOns);
                                    }
                                }

                                TXN_Order_Logs log = new TXN_Order_Logs();
                                log.Order_Id = orderDM.Order_Id;
                                log.Status_Id = orderDM.Status_Id;
                                log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                log.Comments = "Order Placed with Order #" + orderDM.Order_Serial;
                                orderBC.CreateOrderLog(log);


                                if (orderRequest.Payment_Type_Id == PaymentTypes.Cash)
                                {

                                    SendOrderEmail(orderDM.Order_Id);

                                    #region clear cart after successful order
                                    var removeCart = cartBC.RemoveCart(appUserDM.App_User_Id);
                                    #endregion

                                    NotificationBC notificationBC = new NotificationBC(context, logPath);
                                    notificationBC.SendNotificationToDriver(orderDM.Order_Serial);

                                    APP_PUSH_CAMPAIGN campaignDM = new APP_PUSH_CAMPAIGN();
                                    campaignDM.Title_E = "Pick up Request";
                                    campaignDM.Desc_E = "Please accept Order # " + orderDM.Order_Serial + " for delivery";
                                    campaignDM.Title_A = "Pick up Request";
                                    campaignDM.Desc_A = "Please accept Order # " + orderDM.Order_Serial + " for delivery";
                                    campaignDM.Created_Datetime = StaticMethods.GetKuwaitTime();
                                    notificationBC.CreatePushCampaign(campaignDM);

                                    string connectionString = _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                                    using (MySqlConnection con = new MySqlConnection(connectionString))
                                    {
                                        con.Open();
                                        var time = con.ConnectionTimeout;
                                        using (MySqlCommand cmd = new MySqlCommand("InsertNotifications", con))
                                        {
                                            using (var da = new MySqlDataAdapter(cmd))
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.Parameters.AddWithValue("@Campaign_Id", campaignDM.Campaign_Id);
                                                cmd.ExecuteReader();

                                            }

                                        }
                                    }


                                }


                                if (orderRequest.Payment_Type_Id != PaymentTypes.Cash)
                                {
                                    TapChargeRequest tapChargeRequest = new TapChargeRequest();
                                    tapChargeRequest.amount = net_amt;
                                    tapChargeRequest.currency = "KWD";
                                    //tapChargeRequest.source = new Source { id = "src_kw.knet" };
                                    tapChargeRequest.source = new Source { id = "src_all" };
                                    tapChargeRequest.reference = new Reference { transaction = orderDM.Order_Serial, order = orderDM.Order_Id.ToString() };
                                    tapChargeRequest.receipt = new Receipt { email = true, sms = true };
                                    TapCustomer customer = new TapCustomer();
                                    customer.first_name = orderDM.Cust_Name;
                                    customer.email = orderDM.Email;
                                    Phone phone = new Phone();
                                    if (orderDM.Mobile.Length == 12)
                                    {
                                        phone.country_code = Convert.ToInt32(orderDM.Mobile.Substring(1, 3));
                                        phone.number = Convert.ToInt32(orderDM.Mobile.Substring(4));
                                    }
                                    else if (orderDM.Mobile.Length == 8)
                                    {
                                        phone.country_code = 965;
                                        phone.number = Convert.ToInt32(orderDM.Mobile);
                                    }
                                    customer.phone = phone;
                                    tapChargeRequest.customer = customer;
                                    var callBackURL = _config.GetValue<string>("TapPayment:CallBackURL");
                                    tapChargeRequest.redirect = new Redirect { url = callBackURL };
                                    var chargeResponse = TapPayment.CreateChargeRequest(tapChargeRequest, _config);
                                    if (chargeResponse != null)
                                    {

                                        string redirectURL = string.Empty;
                                        if (chargeResponse.transaction != null && !string.IsNullOrEmpty(chargeResponse.transaction.url))
                                            redirectURL = chargeResponse.transaction.url;

                                        if (!string.IsNullOrEmpty(redirectURL))
                                        {
                                            PAYMENTS paymentDM = new PAYMENTS();
                                            paymentDM.Order_Id = orderDM.Order_Id;
                                            paymentDM.Amount = net_amt;
                                            paymentDM.Track_Id = chargeResponse.id;
                                            paymentDM.Created_Datetime = StaticMethods.GetKuwaitTime();
                                            paymentDM.Comments = "";
                                            orderBC.CreatePayment(paymentDM);

                                            globalCls.WriteToFile(logPath, "Redirecting to :" + redirectURL, true);
                                            response.Payment_Link = redirectURL;
                                        }
                                    }
                                }

                                response.OrderId = orderDM.Order_Id;
                                response.Status = 0;
                                response.Message = ServiceResponse.Success;
                                scope.Complete();
                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "App User Not Found";
                                return response;
                            }


                        }
                    }
                }

                catch (Exception ex)
                {
                    scope.Dispose();
                    response.Status = 1;
                    response.Message = ServiceResponse.ServerError;
                    globalCls.WriteToFile(logPath, ex.ToString());
                }
                return response;
            }

        }

        [Route("Order/{orderId}")]
        [HttpGet]
        public OrderDetailResponse GetOrderDetail(int orderId)
        {
            string securityKey = "";
            var response = new OrderDetailResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var orderBC = new OrderBC(context);
                        var orderDM = orderBC.GetOrder(orderId);

                        if (orderDM != null)
                        {
                            response.Order_No = orderDM.Order_Serial;
                            response.Order_Date = orderDM.Order_Datetime.ToString("dd-MMM-yyyy hh:mm tt");
                            response.Cust_Name = orderDM.Cust_Name;
                            response.Mobile = orderDM.Mobile ?? "";
                            response.Email = orderDM.Email;

                            var pickAddressDM = new AddressCoordinatesDTO();
                            pickAddressDM.Address = orderDM.Branch_Address;
                            pickAddressDM.Latitude = orderDM.Branch_Latitude;
                            pickAddressDM.Longitude = orderDM.Branch_Longitude;
                            response.Pickup_Address = pickAddressDM;

                            var deliveryAddressDM = new AddressCoordinatesDTO();
                            deliveryAddressDM.Address = orderDM.Full_Address;
                            deliveryAddressDM.Latitude = orderDM.User_Address_Latitude;
                            deliveryAddressDM.Longitude = orderDM.User_Address_Longitude;
                            response.Delivery_Address = deliveryAddressDM;

                            response.Payment_Type = orderDM.Payment_Type_Name;
                            response.Order_Status_Id = orderDM.Status_Id;
                            response.Order_Status = orderDM.Status_Name;
                            response.Delivery_Option = orderDM.Delivery_Type_Name;
                            response.Sub_Total = orderDM.Gross_Amount;
                            response.Delivery_Charges = orderDM.Delivery_Charges;
                            response.Total = orderDM.Net_Amount;
                            response.Driver_Name = orderDM.Driver_Name;
                            response.Remarks = orderDM.Comments ?? "";
                            response.Current_Driver_Latitude = orderDM.Driver_Latitude;
                            response.Current_Driver_Longitude = orderDM.Driver_Longitude;

                            foreach (var detail in orderDM.TXN_Order_Details)
                            {
                                var detailDM = new DriverOrderDetailDTO();
                                detailDM.Order_Detail_Id = detail.Order_Detail_Id;
                                detailDM.Prod_Name = detail.Full_Product_Name;
                                detailDM.Qty = detail.Qty;
                                detailDM.Rate = detail.Rate;
                                detailDM.Gross_Amount = detail.Amount;
                                detailDM.AddOn_Amount = detail.Gross_Amount - detail.Amount;
                                detailDM.Net_Amount = detail.Gross_Amount;
                                detailDM.Remarks = detail.Comments ?? "";
                                var catering_products = orderBC.GetOrderDetailCategoryProducts(detail.Order_Detail_Id, lang);
                                foreach (var product in catering_products)
                                {
                                    CartCateringProductsDTO cateringProductsDTO = new CartCateringProductsDTO();
                                    cateringProductsDTO.Catering_Product_Id = product.Category_Product_Id;
                                    cateringProductsDTO.Catering_Product = product.Product_Name;
                                    cateringProductsDTO.Qty = product.Qty;
                                    detailDM.Catering_Products.Add(cateringProductsDTO);

                                }
                                response.OrderDetails.Add(detailDM);
                            }
                            //var addons = brandBC.GetProductAddOns(brandDM.Product_Id, lang);
                            // response.AddOns.AddRange(addons);


                            response.Message = ServiceResponse.Success;
                            response.Status = 0;
                        }
                        else
                        {
                            response.Message = "Order Not Found";
                            response.Status = 101;
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("OrderPaymentDetail/{orderId}")]
        [HttpGet]
        public OrderPaymentResponse GetOrderPaymentDetail(int orderId)
        {
            string securityKey = "";
            var response = new OrderPaymentResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var orderBC = new OrderBC(context);
                        var orderDM = orderBC.GetPaymentByOrderId(orderId);

                        if (orderDM != null)
                        {
                            response.Order_Id = orderDM.Order_Id ?? 0;
                            response.Payment_Date = Convert.ToDateTime(orderDM.Created_Datetime).ToString("dd-MMM-yyyy hh:mm tt");
                            response.Trans_Id = orderDM.Trans_Id ?? "";
                            response.Payment_Id = orderDM.Payment_Id ?? "";
                            response.Tap_Id = orderDM.Track_Id ?? "";
                            response.Auth = orderDM.Auth ?? "";
                            response.Reference_No = orderDM.Reference_No ?? "";
                            response.Result = orderDM.Result ?? "";
                            response.Payment_Mode = orderDM.Payment_Mode ?? "";
                            response.Order_Amount = orderDM.Amount;
                            response.Message = ServiceResponse.Success;
                            response.Status = 0;
                        }
                        else
                        {
                            response.Message = "Payment Not Found";
                            response.Status = 101;
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("OrderTrackingDetail/{orderId}")]
        [HttpGet]
        public TrackingResponse GetOrderTrackingDetail(int orderId)
        {
            string securityKey = "";
            var response = new TrackingResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var orderBC = new OrderBC(context);
                        var orderDM = orderBC.GetLatestTrackingDetail(orderId);

                        if (orderDM != null)
                        {
                            response.Latitude = orderDM.Latitude;
                            response.Longitude = orderDM.Longitude;
                            response.Message = ServiceResponse.Success;
                            response.Status = 0;
                        }
                        else
                        {
                            response.Message = "Tracking Detail Not Found";
                            response.Status = 101;
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Settings")]
        [HttpGet]
        public SettingResponse GetSettings()
        {
            string securityKey = "";
            var response = new SettingResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var areaBC = new CommonBC(context, logPath);
                        var currentevents = areaBC.GetSettings();
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        foreach (var currentevent in currentevents)
                        {
                            var eventsDTO = new SettingDTO();
                            eventsDTO.Setting_Id = currentevent.SETTING_ID;
                            eventsDTO.Setting_Name = currentevent.SETTING_NAME;
                            eventsDTO.Setting_Value = currentevent.SETTING_VALUE;
                            response.Settings.Add(eventsDTO);
                        }


                        response.Message = ServiceResponse.Success;
                        response.Status = 0;
                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        #region Favorite
        [Route("AddFavorite")]
        [HttpPost]
        public GeneralResponse AddFavorite(FavoriteRequest favoriteRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        if (string.IsNullOrEmpty(favoriteRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBC = new AppUserBC(context);
                        var commonBC = new CommonBC(context, logPath);

                        var row_id = new Guid(favoriteRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var isFavExist = commonBC.GetFavorite(appUserDM.App_User_Id, favoriteRequest.Product_Id);
                            if (isFavExist == null)
                            {
                                TXN_Favorite favoriteDM = new TXN_Favorite();
                                favoriteDM.Product_Id = favoriteRequest.Product_Id;
                                favoriteDM.App_User_Id = appUserDM.App_User_Id;
                                favoriteDM.Created_Datetime = StaticMethods.GetKuwaitTime();
                                commonBC.AddFavorite(favoriteDM);

                                response.Status = 0;
                                response.Message = ServiceResponse.Success;
                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "Product already exist in favorite list";
                                return response;
                            }


                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("FavoriteProducts/{app_user_id}")]
        [HttpGet]
        public ProductsResponse GetFavoriteProducts(string app_user_id)
        {
            string securityKey = "";
            var response = new ProductsResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        if (string.IsNullOrEmpty(app_user_id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);


                        var row_id = new Guid(app_user_id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var commonBC = new CommonBC(context, logPath);
                            var posts = commonBC.GetFavoriteProducts(appUserDM.App_User_Id);


                            foreach (var currentevent in posts.Items)
                            {
                                var eventsDTO = new ProductDTO();
                                eventsDTO.Product_Id = currentevent.Product_Id;
                                eventsDTO.Product_Name = lang.ToUpper() == "E" ? currentevent.Product_Name_E : currentevent.Product_Name_A ?? "";
                                eventsDTO.Product_Desc = lang.ToUpper() == "E" ? currentevent.Product_Desc_E ?? "" : currentevent.Product_Desc_A ?? "";
                                eventsDTO.Price = currentevent.Price;
                                eventsDTO.Image_Url = currentevent.Image_URL ?? "";
                                eventsDTO.Brand_Name = lang.ToUpper() == "E" ? currentevent.Brand_Name_E ?? "" : currentevent.Brand_Name_A ?? "";
                                response.Products.Add(eventsDTO);
                            }
                            response.Total_Products = posts.Total_Items;
                            response.Message = ServiceResponse.Success;
                            response.Status = 0;

                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("RemoveFavorite")]
        [HttpPost]
        public GeneralResponse RemoveFavorite(FavoriteRequest favoriteRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        if (string.IsNullOrEmpty(favoriteRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBC = new AppUserBC(context);
                        var commonBC = new CommonBC(context, logPath);

                        var row_id = new Guid(favoriteRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var isFavExist = commonBC.GetFavorite(appUserDM.App_User_Id, favoriteRequest.Product_Id);
                            if (isFavExist != null)
                            {

                                var isRemoved = commonBC.RemoveFavorite(isFavExist.Favorite_Id);
                                if (isRemoved)
                                {
                                    response.Status = 0;
                                    response.Message = ServiceResponse.Success;
                                }
                                else
                                {
                                    response.Status = 101;
                                    response.Message = "Due to technical error product not removed from favorite list";
                                    return response;
                                }

                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "Product not exist in favorite list";
                                return response;
                            }


                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        #endregion

        #region Driver

        [Route("DriverPendingOrders/{app_user_id}")]
        [HttpGet]
        public DriverOrderResponse GetDriverPendingOrders(string app_user_id)
        {
            string securityKey = "";
            var response = new DriverOrderResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(app_user_id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);


                        var row_id = new Guid(app_user_id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var areaBC = new OrderBC(context);
                            var currentevents = areaBC.GetPendingDriverOrders(lang, appUserDM.App_User_Id);

                            foreach (var currentevent in currentevents)
                            {
                                var eventsDTO = new DriverOrderDTO();
                                eventsDTO.Order_Id = currentevent.Order_Id;
                                eventsDTO.Order_No = currentevent.Order_Serial;
                                eventsDTO.Pickup_Address = currentevent.Branch_Address;
                                eventsDTO.Delivery_Address = currentevent.Full_Address;
                                response.Orders.Add(eventsDTO);
                            }
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                            //return response;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("UpdateOrder")]
        [HttpPost]
        public GeneralResponse UpdateOrder(DriverOrderRequest driverOrderRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(driverOrderRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var orderBC = new OrderBC(context);

                        var row_id = new Guid(driverOrderRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var orderDM = orderBC.GetOrder(driverOrderRequest.Order_Id);
                            if (orderDM != null)
                            {
                                if (orderDM.Status_Id == OrderStatus.ORDER_PREPARING || orderDM.Status_Id == OrderStatus.ORDER_PAID)
                                {
                                    var existingDeliveringOrder = orderBC.GetExistingDriverOrder(appUserDM.App_User_Id);
                                    if (existingDeliveringOrder != null && driverOrderRequest.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER)
                                    {
                                        response.Status = 101;
                                        response.Message = "Please first deliver Order # " + existingDeliveringOrder.Order_Serial + " and then accept this order";
                                        return response;
                                    }
                                    if (driverOrderRequest.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER)
                                    {
                                        orderDM.Driver_Id = appUserDM.App_User_Id;
                                        orderDM.Status_Id = OrderStatus.ACCEPTED_BY_DRIVER;
                                        orderBC.SaveOrder(orderDM);
                                    }

                                    if (driverOrderRequest.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER || driverOrderRequest.Status_Id == OrderStatus.DECLINED_BY_DRIVER)
                                    {
                                        TXN_Order_Logs log = new TXN_Order_Logs();
                                        log.Order_Id = orderDM.Order_Id;
                                        log.Status_Id = orderDM.Status_Id;
                                        log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        log.Driver_Id = appUserDM.App_User_Id;
                                        if (driverOrderRequest.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER)
                                            log.Comments = "Order Accepted By Driver";
                                        else if (driverOrderRequest.Status_Id == OrderStatus.DECLINED_BY_DRIVER)
                                            log.Comments = "Order Declined By Driver";
                                        orderBC.CreateOrderLog(log);
                                    }
                                    response.Status = 0;
                                    response.Message = ServiceResponse.Success;
                                }
                                else
                                {
                                    response.Status = 101;
                                    response.Message = "Order is already accepted by some other driver";
                                    return response;
                                }
                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "Order Not Found";
                                return response;
                            }


                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("PickupOrder")]
        [HttpPost]
        public GeneralResponse PickupOrder(DriverOrderRequest driverOrderRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(driverOrderRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        if (string.IsNullOrEmpty(driverOrderRequest.Delivery_Image))
                        {
                            response.Status = 101;
                            response.Message = "Please upload pickup image first";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var orderBC = new OrderBC(context);

                        var row_id = new Guid(driverOrderRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var orderDM = orderBC.GetOrder(driverOrderRequest.Order_Id);
                            if (orderDM != null)
                            {
                                if (orderDM.Driver_Id == appUserDM.App_User_Id)
                                {
                                    if (orderDM.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER)
                                    {
                                        byte[] bytes = Convert.FromBase64String(driverOrderRequest.Delivery_Image);
                                        var image_path_dir = "assets/images/delivery/";
                                        var fileName = StaticMethods.GetKuwaitTime().ToString("yyyyMMddHHmmssfff") + ".png";
                                        var path = Path.Combine(this.iwebHostEnvironment.WebRootPath, image_path_dir);
                                        var filePath = Path.Combine(path, fileName);
                                        System.IO.File.WriteAllBytes(filePath, bytes);

                                        orderDM.Pickup_Image = image_path_dir + fileName;
                                        orderDM.Status_Id = OrderStatus.OUT_FOR_DELIVERY;
                                        orderBC.SaveOrder(orderDM);

                                        TXN_Order_Logs log = new TXN_Order_Logs();
                                        log.Order_Id = orderDM.Order_Id;
                                        log.Status_Id = orderDM.Status_Id;
                                        log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        log.Driver_Id = appUserDM.App_User_Id;
                                        log.Comments = "Order Picked up By Driver";
                                        orderBC.CreateOrderLog(log);

                                        response.Status = 0;
                                        response.Message = ServiceResponse.Success;
                                    }
                                    else
                                    {
                                        response.Status = 101;
                                        response.Message = "Order is already picked up";
                                        return response;
                                    }
                                }
                                else
                                {
                                    response.Status = 101;
                                    response.Message = "You can't pick up this order as it is assigned to other driver";
                                    return response;
                                }

                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "Order Not Found";
                                return response;
                            }


                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("DeliverOrder")]
        [HttpPost]
        public GeneralResponse DeliverOrder(DriverOrderRequest driverOrderRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(driverOrderRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        if (string.IsNullOrEmpty(driverOrderRequest.Delivery_Image))
                        {
                            response.Status = 101;
                            response.Message = "Please upload delivery image first";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var orderBC = new OrderBC(context);

                        var row_id = new Guid(driverOrderRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var orderDM = orderBC.GetOrder(driverOrderRequest.Order_Id);
                            if (orderDM != null)
                            {
                                if (orderDM.Driver_Id == appUserDM.App_User_Id)
                                {
                                    if (orderDM.Status_Id == OrderStatus.OUT_FOR_DELIVERY)
                                    {
                                        byte[] bytes = Convert.FromBase64String(driverOrderRequest.Delivery_Image);
                                        var image_path_dir = "assets/images/delivery/";
                                        var fileName = StaticMethods.GetKuwaitTime().ToString("yyyyMMddHHmmssfff") + ".png";
                                        var path = Path.Combine(this.iwebHostEnvironment.WebRootPath, image_path_dir);
                                        var filePath = Path.Combine(path, fileName);
                                        System.IO.File.WriteAllBytes(filePath, bytes);

                                        orderDM.Delivery_Image = image_path_dir + fileName;
                                        orderDM.Status_Id = OrderStatus.ORDER_DELIVERED;
                                        orderBC.SaveOrder(orderDM);

                                        TXN_Order_Logs log = new TXN_Order_Logs();
                                        log.Order_Id = orderDM.Order_Id;
                                        log.Status_Id = orderDM.Status_Id;
                                        log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        log.Driver_Id = appUserDM.App_User_Id;
                                        log.Comments = "Order Delivered By Driver";
                                        orderBC.CreateOrderLog(log);

                                        CommonBC commonBC = new CommonBC(context, logPath);
                                        var points_per_kd = commonBC.GetSettingValue<int>(SettingNames.Points_Per_KD);

                                        LP_POINTS_TRANSACTION txnDM = new LP_POINTS_TRANSACTION();
                                        txnDM.Type_Id = TXN_Point_Types.Add_Points_After_Payment;
                                        txnDM.TXN_Date = StaticMethods.GetKuwaitTime().Date;
                                        txnDM.App_User_Id = orderDM.App_User_Id;
                                        txnDM.Order_Id = orderDM.Order_Id;
                                        txnDM.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        txnDM.Points = orderDM.Net_Amount * points_per_kd;

                                        string connectionString = _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                                        using (MySqlConnection con = new MySqlConnection(connectionString))
                                        {
                                            con.Open();
                                            var time = con.ConnectionTimeout;
                                            using (MySqlCommand cmd = new MySqlCommand("UpdateRedeemPoints", con))
                                            {
                                                using (var da = new MySqlDataAdapter(cmd))
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.Parameters.AddWithValue("@App_User_Id", orderDM.App_User_Id);
                                                    cmd.ExecuteReader();

                                                }

                                            }
                                        }

                                        response.Status = 0;
                                        response.Message = ServiceResponse.Success;
                                    }
                                    else
                                    {
                                        response.Status = 101;
                                        response.Message = "Order is not out for delivery so it can't be delivered";
                                        return response;
                                    }
                                }
                                else
                                {
                                    response.Status = 101;
                                    response.Message = "You can't deliver this order as it is assigned to other driver";
                                    return response;
                                }

                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "Order Not Found";
                                return response;
                            }


                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("NotDeliverOrder")]
        [HttpPost]
        public GeneralResponse NotDeliverOrder(DriverOrderRequest driverOrderRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(driverOrderRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        if (string.IsNullOrEmpty(driverOrderRequest.Comments))
                        {
                            response.Status = 101;
                            response.Message = "Cancellation reason cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var orderBC = new OrderBC(context);

                        var row_id = new Guid(driverOrderRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var orderDM = orderBC.GetOrder(driverOrderRequest.Order_Id);
                            if (orderDM != null)
                            {
                                if (orderDM.Driver_Id == appUserDM.App_User_Id)
                                {
                                    if (orderDM.Status_Id == OrderStatus.OUT_FOR_DELIVERY)
                                    {
                                        orderDM.Cancelled_Reason = driverOrderRequest.Comments;
                                        orderDM.Status_Id = OrderStatus.NOT_DELIVERED;
                                        orderBC.SaveOrder(orderDM);

                                        TXN_Order_Logs log = new TXN_Order_Logs();
                                        log.Order_Id = orderDM.Order_Id;
                                        log.Status_Id = orderDM.Status_Id;
                                        log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        log.Driver_Id = appUserDM.App_User_Id;
                                        log.Comments = "Order Not Delivered (" + driverOrderRequest.Comments + ")";
                                        orderBC.CreateOrderLog(log);

                                        CommonBC commonBC = new CommonBC(context, logPath);
                                        var points_per_kd = commonBC.GetSettingValue<int>(SettingNames.Points_Per_KD);

                                        LP_POINTS_TRANSACTION txnDM = new LP_POINTS_TRANSACTION();
                                        txnDM.Type_Id = TXN_Point_Types.Add_Points_After_Payment;
                                        txnDM.TXN_Date = StaticMethods.GetKuwaitTime().Date;
                                        txnDM.App_User_Id = orderDM.App_User_Id;
                                        txnDM.Order_Id = orderDM.Order_Id;
                                        txnDM.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        txnDM.Points = orderDM.Net_Amount * points_per_kd;

                                        string connectionString = _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                                        using (MySqlConnection con = new MySqlConnection(connectionString))
                                        {
                                            con.Open();
                                            var time = con.ConnectionTimeout;
                                            using (MySqlCommand cmd = new MySqlCommand("UpdateRedeemPoints", con))
                                            {
                                                using (var da = new MySqlDataAdapter(cmd))
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.Parameters.AddWithValue("@App_User_Id", orderDM.App_User_Id);
                                                    cmd.ExecuteReader();

                                                }

                                            }
                                        }

                                        response.Status = 0;
                                        response.Message = ServiceResponse.Success;
                                    }
                                    else
                                    {
                                        response.Status = 101;
                                        response.Message = "Order is not out for delivery so it can't be delivered";
                                        return response;
                                    }
                                }
                                else
                                {
                                    response.Status = 101;
                                    response.Message = "You can't deliver this order as it is assigned to other driver";
                                    return response;
                                }

                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "Order Not Found";
                                return response;
                            }


                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("DriverDeliveredOrders/{app_user_id}")]
        [HttpGet]
        public DriverOrderResponse GetDriverDeliveredOrders(string app_user_id)
        {
            string securityKey = "";
            var response = new DriverOrderResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(app_user_id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);


                        var row_id = new Guid(app_user_id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var areaBC = new OrderBC(context);
                            var currentevents = areaBC.GetDeliveredDriverOrders(lang, appUserDM.App_User_Id);

                            foreach (var currentevent in currentevents)
                            {
                                var eventsDTO = new DriverOrderDTO();
                                eventsDTO.Order_Id = currentevent.Order_Id;
                                eventsDTO.Order_No = currentevent.Order_Serial;
                                eventsDTO.Pickup_Address = currentevent.Branch_Address;
                                eventsDTO.Delivery_Address = currentevent.Full_Address;
                                eventsDTO.Order_Date = currentevent.Order_Datetime.ToString("dd-MM-yyyy hh:mm tt");
                                eventsDTO.Order_Amount = currentevent.Net_Amount;
                                eventsDTO.Order_Status = currentevent.Status_Name;
                                eventsDTO.Payment_Type = currentevent.Payment_Type_Name;
                                response.Orders.Add(eventsDTO);
                            }
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                            //return response;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("FilterDriverDeliveredOrders")]
        [HttpPost]
        public DriverOrderResponse GetFilterDriverDeliveredOrders(GetDriverOrdersRequest driverOrdersRequest)
        {
            string securityKey = "";
            var response = new DriverOrderResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(driverOrdersRequest.App_User_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);


                        var row_id = new Guid(driverOrdersRequest.App_User_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var areaBC = new OrderBC(context);
                            var fromDate = DateTime.ParseExact(driverOrdersRequest.From_Date, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                            var toDate = DateTime.ParseExact(driverOrdersRequest.To_Date + " 23:59:59", "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                            var currentevents = areaBC.GetDeliveredDriverOrders(lang, appUserDM.App_User_Id, fromDate, toDate);

                            response.Total_Orders = currentevents.Count;
                            response.Cash_Collected = currentevents.Where(x => x.Payment_Type_Id == PaymentTypes.Cash).Sum(x => x.Net_Amount);
                            foreach (var currentevent in currentevents)
                            {
                                var eventsDTO = new DriverOrderDTO();
                                eventsDTO.Order_Id = currentevent.Order_Id;
                                eventsDTO.Order_No = currentevent.Order_Serial;
                                eventsDTO.Pickup_Address = currentevent.Branch_Address;
                                eventsDTO.Delivery_Address = currentevent.Full_Address;
                                eventsDTO.Order_Date = currentevent.Order_Datetime.ToString("dd-MM-yyyy hh:mm tt");
                                eventsDTO.Order_Amount = currentevent.Net_Amount;
                                eventsDTO.Order_Status = currentevent.Status_Name;
                                eventsDTO.Payment_Type = currentevent.Payment_Type_Name;
                                response.Orders.Add(eventsDTO);
                            }
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                            //return response;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("DriverDeliveryOrder/{app_user_id}")]
        [HttpGet]
        public OrderDetailResponse GetDriverDeliveryOrderDetail(string app_user_id)
        {
            string securityKey = "";
            var response = new OrderDetailResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(app_user_id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);


                        var row_id = new Guid(app_user_id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var orderBC = new OrderBC(context);
                            var existingDeliveringOrder = orderBC.GetExistingDriverOrder(appUserDM.App_User_Id);


                            if (existingDeliveringOrder != null)
                            {
                                var orderDM = orderBC.GetOrder(existingDeliveringOrder.Order_Id);
                                response.Order_Id = orderDM.Order_Id;
                                response.Order_No = orderDM.Order_Serial;
                                response.Order_Date = orderDM.Order_Datetime.ToString("dd-MMM-yyyy hh:mm tt");
                                response.Cust_Name = orderDM.Cust_Name;
                                response.Mobile = orderDM.Mobile ?? "";
                                response.Email = orderDM.Email;

                                var pickAddressDM = new AddressCoordinatesDTO();
                                pickAddressDM.Address = orderDM.Branch_Address;
                                pickAddressDM.Latitude = orderDM.Branch_Latitude;
                                pickAddressDM.Longitude = orderDM.Branch_Longitude;
                                response.Pickup_Address = pickAddressDM;

                                var deliveryAddressDM = new AddressCoordinatesDTO();
                                deliveryAddressDM.Address = orderDM.Full_Address;
                                deliveryAddressDM.Latitude = orderDM.User_Address_Latitude;
                                deliveryAddressDM.Longitude = orderDM.User_Address_Longitude;
                                response.Delivery_Address = deliveryAddressDM;

                                response.Payment_Type = orderDM.Payment_Type_Name;
                                response.Order_Status_Id = orderDM.Status_Id;
                                response.Order_Status = orderDM.Status_Name;
                                response.Delivery_Option = orderDM.Delivery_Type_Name;
                                response.Sub_Total = orderDM.Gross_Amount;
                                response.Delivery_Charges = orderDM.Delivery_Charges;
                                response.Total = orderDM.Net_Amount;
                                response.Driver_Name = orderDM.Driver_Name;
                                response.Remarks = orderDM.Comments ?? "";
                                response.Restaurant_Name = orderDM.Restaurant_Name;
                                response.Restaurant_Mobile = orderDM.Mobile ?? "";
                                response.Current_Driver_Latitude = orderDM.Driver_Latitude;
                                response.Current_Driver_Longitude = orderDM.Driver_Longitude;

                                foreach (var detail in orderDM.TXN_Order_Details)
                                {
                                    var detailDM = new DriverOrderDetailDTO();
                                    detailDM.Prod_Name = detail.Full_Product_Name;
                                    detailDM.Qty = detail.Qty;
                                    detailDM.Rate = detail.Rate;
                                    detailDM.Gross_Amount = detail.Amount;
                                    detailDM.AddOn_Amount = detail.Gross_Amount - detail.Amount;
                                    detailDM.Net_Amount = detail.Gross_Amount;
                                    detailDM.Remarks = detail.Comments ?? "";
                                    response.OrderDetails.Add(detailDM);
                                }
                                //var addons = brandBC.GetProductAddOns(brandDM.Product_Id, lang);
                                // response.AddOns.AddRange(addons);


                                response.Message = ServiceResponse.Success;
                                response.Status = 0;
                            }
                            else
                            {
                                response.Message = lang == "A" ? "لم يتم العثور على طلب للتسليم" : "No Order found for delivery";
                                response.Status = 103;
                            }
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }


                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("TrackOrder")]
        [HttpPost]
        public GeneralResponse TrackOrder(TrackingRequest trackingRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(trackingRequest.Driver_Id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        var orderBC = new OrderBC(context);

                        var row_id = new Guid(trackingRequest.Driver_Id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            TXN_Order_Tracking_Details trackingDM = new TXN_Order_Tracking_Details();
                            trackingDM.Order_Id = trackingRequest.Order_Id;
                            trackingDM.Status_Id = trackingRequest.Status_Id;
                            trackingDM.Track_Datetime = StaticMethods.GetKuwaitTime();
                            trackingDM.Driver_Id = appUserDM.App_User_Id;
                            trackingDM.Latitude = trackingRequest.Latitude;
                            trackingDM.Longitude = trackingRequest.Longitude;
                            orderBC.CreateOrderTrackingDetail(trackingDM);


                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                            //return response;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }



                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }


        #endregion

        [Route("CacaooMaps")]
        [HttpGet]
        public CacaooMapResponse GetCacaooMaps()
        {
            string securityKey = "";
            var response = new CacaooMapResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        var branchBC = new BranchBC(context);
                        var currentevents = branchBC.GetAllBranches();
                        foreach (var currentevent in currentevents)
                        {
                            var eventsDTO = new MapDTO();
                            eventsDTO.Branch_Name = lang == "A" ? currentevent.Branch_Name_A ?? currentevent.Branch_Name_E : currentevent.Branch_Name_E;
                            eventsDTO.Branch_Address = lang == "A" ? currentevent.Address_A ?? currentevent.Address_E ?? "" : currentevent.Address_E ?? "";
                            eventsDTO.Restaurant_Name = currentevent.Restaurant_Name;
                            eventsDTO.Latitude = currentevent.Latitude ?? 0;
                            eventsDTO.Longitude = currentevent.Longitude ?? 0;
                            response.Branches.Add(eventsDTO);
                        }


                        response.Message = ServiceResponse.Success;
                        response.Status = 0;
                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("CustomerOrders/{app_user_id}")]
        [HttpGet]
        public DriverOrderResponse GetCustomerOrders(string app_user_id)
        {
            string securityKey = "";
            var response = new DriverOrderResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(app_user_id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);


                        var row_id = new Guid(app_user_id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var areaBC = new OrderBC(context);
                            var currentevents = areaBC.GetCustomerOrders(lang, appUserDM.App_User_Id);

                            foreach (var currentevent in currentevents)
                            {
                                var eventsDTO = new DriverOrderDTO();
                                eventsDTO.Order_Id = currentevent.Order_Id;
                                eventsDTO.Order_No = currentevent.Order_Serial;
                                eventsDTO.Pickup_Address = currentevent.Branch_Address;
                                eventsDTO.Delivery_Address = currentevent.Full_Address;
                                eventsDTO.Order_Date = currentevent.Order_Datetime.ToString("dd-MM-yyyy hh:mm tt");
                                eventsDTO.Order_Amount = currentevent.Net_Amount;
                                eventsDTO.Order_Status = currentevent.Status_Name;
                                response.Orders.Add(eventsDTO);
                            }
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                            //return response;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("RateProduct")]
        [HttpPost]
        public GeneralResponse RateProduct(RatingRequest ratingRequest)
        {
            string securityKey = "";
            var response = new GeneralResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {

                        var orderBC = new OrderBC(context);

                        var orderDetailDM = orderBC.GetOrderDetail(ratingRequest.Order_Detail_Id);
                        if (orderDetailDM != null)
                        {
                            if (orderDetailDM.Rating == null)
                            {
                                var orderDM = orderBC.GetOrderByOrderDetail(ratingRequest.Order_Detail_Id);
                                if (orderDM != null)
                                {
                                    if (orderDM.Status_Id == OrderStatus.ORDER_DELIVERED)
                                    {
                                        orderDetailDM.Rating = ratingRequest.Rating;
                                        orderBC.SaveOrderDetail(orderDetailDM);

                                        string connectionString = _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                                        using (MySqlConnection con = new MySqlConnection(connectionString))
                                        {
                                            con.Open();
                                            var time = con.ConnectionTimeout;
                                            using (MySqlCommand cmd = new MySqlCommand("SetProductRating", con))
                                            {
                                                using (var da = new MySqlDataAdapter(cmd))
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.Parameters.AddWithValue("@Prod_Id", orderDetailDM.Product_Id);
                                                    cmd.ExecuteReader();

                                                }

                                            }
                                        }


                                        response.Status = 0;
                                        response.Message = ServiceResponse.Success;
                                    }
                                    else
                                    {
                                        response.Status = 101;
                                        response.Message = "You can already rate product of delivered orders";
                                        return response;
                                    }
                                }
                                else
                                {
                                    response.Status = 101;
                                    response.Message = "Order Not Found";
                                    return response;
                                }

                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "Rating already exist";
                                return response;
                            }

                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "Order Detail Not Found";
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        [Route("Notification/{app_user_id}")]
        [HttpGet]
        public NotificationResponse GetNotifications(string app_user_id)
        {
            string securityKey = "";
            var response = new NotificationResponse();

            try
            {
                if (!Request.Headers.ContainsKey("X-Cacaoo-SecurityToken"))
                {
                    response.Status = 105;
                    response.Message = ServiceResponse.NoSecurityKey;
                }
                else
                {
                    securityKey = Request.Headers["X-Cacaoo-SecurityToken"];
                    //Check session token
                    bool isAuthorized = ValidateSecurityKey(securityKey);

                    if (isAuthorized == false)
                    {
                        response.Status = 106;
                        response.Message = ServiceResponse.Unauthorized;
                    }
                    else
                    {


                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }
                        if (string.IsNullOrEmpty(app_user_id))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }
                        AppUserBC appUserBC = new AppUserBC(context);
                        NotificationBC notificationBC = new NotificationBC(context, logPath);


                        var row_id = new Guid(app_user_id);
                        var appUserDM = appUserBC.GetAppUserByRowId(row_id);
                        if (appUserDM != null)
                        {
                            var notifications = notificationBC.GetNotifications(appUserDM.App_User_Id, lang);
                            foreach (var notification in notifications)
                            {
                                var notificationDTO = new NotificationDTO();
                                notificationDTO.Notification_Id = notification.Notification_Id;
                                notificationDTO.Title = notification.Title;
                                notificationDTO.Desc = notification.Desc;
                                notificationDTO.Time = "";
                                TimeSpan span = StaticMethods.GetKuwaitTime().Subtract(notification.Created_Datetime);
                                if (span.TotalSeconds < 60)
                                {
                                    notificationDTO.Time = "Just Now";
                                }
                                else if (span.TotalMinutes < 60)
                                {
                                    notificationDTO.Time = span.Minutes + "m ago";
                                }
                                else if (span.TotalHours < 24)
                                {
                                    notificationDTO.Time = span.Hours + "h ago";
                                }
                                else
                                {
                                    notificationDTO.Time = span.Days + "d ago";
                                }

                                response.Notifications.Add(notificationDTO);
                            }
                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                            //return response;
                        }
                        else
                        {
                            response.Status = 101;
                            response.Message = "App User Not Found";
                            return response;
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }

            return response;
        }


        #region Web Application API

        [Route("RestaurantAddOns/{restaurant_id}")]
        [HttpGet]
        public List<SM_Restaurant_AddOns> GetRestaurantAddOns(long restaurant_id)
        {
            var response = new List<SM_Restaurant_AddOns>();
            try
            {
                RestaurantBC restaurantBC = new RestaurantBC(context);
                response = restaurantBC.GetAllRestaurantAddOns(restaurant_id);

            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }

            return response;
        }

        [Route("DashboardOrders/{dashboard_order_type}")]
        [HttpGet]
        public DriverOrderResponse GetDashboardOrders(int dashboard_order_type)
        {
            var response = new DriverOrderResponse();

            try
            {
                var areaBC = new OrderBC(context);
                response.Orders = areaBC.GetDashboardOrders(dashboard_order_type);
                response.Status = 0;
                response.Message = ServiceResponse.Success;

            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(logPath, ex.ToString());
            }
            return response;
        }

        #endregion

        #region Non Action
        [NonAction]
        private bool ValidateSecurityKey(string securityKey)
        {
            bool isAuthorisedUser = false;
            try
            {
                var eisDAL = new DeviceBC(context, logPath);
                securityDTO = eisDAL.GetDeviceByClientKey(Base64Decode(securityKey));
                // globalCls.WriteToFile(HttpContext.Current.Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["ErrorFilePath"])), "Client Key : " + securityDTO.Client_Key, true);

                if (securityDTO != null && !string.IsNullOrEmpty(securityDTO.Client_Key))
                {
                    isAuthorisedUser = true;
                }
                else
                {
                    isAuthorisedUser = false;
                    //  LogError("Unauthorized request | Device not registered", "ValidateAuthKey");
                    //  Errorlog.WriteToFile(HttpContext.Current.Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["ErrorFilePath"])), "Unauthorized request | Device not registered", true);
                }
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString());
                isAuthorisedUser = false;
            }
            return isAuthorisedUser;
        }
        [NonAction]
        static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        [NonAction]
        public bool SendOrderEmail(long orderId)
        {
            bool bSuccess = false;
            try
            {

                var orderBC = new OrderBC(context);
                RestaurantBC restaurantBC = new RestaurantBC(context);
                decimal grossAmount = Decimal.Zero;

                var order = orderBC.GetOrder(Convert.ToInt32(orderId));
                var orderDetails = orderBC.GetOrderDetails(Convert.ToInt32(orderId));


                if (order != null)
                {
                    string bodyMessage = "";

                    var site_configuration = orderBC.GetSiteConfiguration(Email_Templates.COD_EMAIL_MESSAGE);

                    bodyMessage = site_configuration.Config_Value;
                    bodyMessage = bodyMessage.Replace("[CUST_NAME]", order.Cust_Name);
                    bodyMessage = bodyMessage.Replace("[ORDER_NO]", order.Order_Serial);
                    bodyMessage = bodyMessage.Replace("[ORDER_DATE]", Convert.ToDateTime(order.Order_Datetime).ToString("dd/MM/yy hh:mm tt"));
                    bodyMessage = bodyMessage.Replace("[TEL_NO]", order.Mobile);
                    bodyMessage = bodyMessage.Replace("[PAYMENT]", order.Payment_Type_Name);
                    if (order.Payment_Type_Id == PaymentTypes.Cash)
                    {
                        bodyMessage = bodyMessage.Replace("[KNET_DETAILS]", "");
                    }
                    bodyMessage = bodyMessage.Replace("[EARNED_POINTS]", "0");

                    bodyMessage = bodyMessage.Replace("[ADDRESS]", order.Full_Address);

                    string substring = "";

                    foreach (var orderdetail in order.TXN_Order_Details)
                    {


                        string col1 = " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" + orderdetail.Full_Product_Name + "<o:p></o:p></span></p></td>";
                        string col2 = " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" + Convert.ToDecimal(orderdetail.Rate).ToString("N3") + "<o:p></o:p></span></p></td>";

                        string col5 = " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" + orderdetail.Qty.ToString() + "<o:p></o:p></span></p></td>";
                        string col6 = " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" + Convert.ToDecimal(orderdetail.Gross_Amount).ToString("N3") + "<o:p></o:p></span></p></td>";
                        substring += " <tr>" + col1 + col2 + /*col3 +*/ /*col4 +*/ col5 + col6 + "</tr> ";
                        grossAmount += (decimal)orderdetail.Gross_Amount;
                    }


                    var netAmount = grossAmount;
                    decimal deliveryCharge = decimal.Zero;


                    deliveryCharge = order.Delivery_Charges;
                    netAmount += order.Delivery_Charges;
                    //substring += "</tr><tr><td><div style='text-align:right'>DELIVERY CHARGES :</div></td><td></td><td></td><td>" + "KWD" + "        &nbsp;" + Convert.ToDecimal(order.DELIVERY_CHARGES).ToString("N3") + "     </td></tr>";


                    //decimal redeemAmt = Decimal.Zero;
                    bodyMessage = bodyMessage.Replace("[REDEEM_LINE]", "");
                    // decimal walletAmt = Decimal.Zero;
                    bodyMessage = bodyMessage.Replace("[WALLET_LINE]", "");
                    //decimal tipAmt = Decimal.Zero;
                    bodyMessage = bodyMessage.Replace("[TIP_LINE]", "");


                    bodyMessage = bodyMessage.Replace("[ORDER_DETAIL]", substring);
                    bodyMessage = bodyMessage.Replace("[GROSS_AMOUNT]", grossAmount.ToString("N3"));
                    bodyMessage = bodyMessage.Replace("[DELIVERY_CHARGE]", deliveryCharge.ToString("N3"));
                    bodyMessage = bodyMessage.Replace("[NET_AMOUNT]", netAmount.ToString("N3"));

                    var subject = site_configuration.Subject;
                    subject = subject.Replace("[LOCATION_NAME]", "");
                    subject = subject.Replace("[DELIVERY_OPTION]", order.Delivery_Type_Name);
                    subject = subject.Replace("[orderId]", order.Order_Serial);
                    subject = subject.Replace("[CustomerName]", order.Cust_Name);

                    var channel = "APP";
                    if (order.Channel_Id == 1)
                    {
                        channel = "WEB";
                    }
                    subject = subject.Replace("[CHANNEL]", channel);



                    var restaurantDM = restaurantBC.GetRestaurant(order.Restaurant_Id);
                    if (restaurantDM != null && !string.IsNullOrEmpty(restaurantDM.Email))
                    {
                        site_configuration.BCC_Email = site_configuration.BCC_Email.Replace("[RESTAURANT_EMAIL]", restaurantDM.Email);
                    }
                    bSuccess = SendHTMLMail(order.Email, subject, bodyMessage, site_configuration.CC_Email ?? "", site_configuration.BCC_Email ?? "", site_configuration.From_Email ?? "", site_configuration.Password ?? "");
                    var emailMsg = "";
                    if (bSuccess)
                    {
                        emailMsg = "Email sent successfully for Order Id:" + orderId;
                    }
                    else
                    {
                        emailMsg = "Email not sent successfully for Order Id:" + orderId;
                    }

                    globalCls.WriteToFile(logPath, emailMsg);
                }
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString());
            }



            return bSuccess;
        }
        [NonAction]
        public bool SendHTMLMail(string to, string subject, string body, string cc, string bcc, string fromEmail, string password, string receiverName = "")
        {
            try
            {
                var server = _config.GetValue<string>("MailSettings:Server");
                var port = _config.GetValue<int>("MailSettings:Port");
                var senderName = _config.GetValue<string>("MailSettings:SenderName");

                using (MimeMessage emailMessage = new MimeMessage())
                {
                    MailboxAddress emailFrom = new MailboxAddress(senderName, fromEmail);
                    emailMessage.From.Add(emailFrom);

                    MailboxAddress emailTo = new MailboxAddress(receiverName, to);
                    emailMessage.To.Add(emailTo);

                    if (!string.IsNullOrEmpty(cc))
                        emailMessage.Cc.Add(new MailboxAddress("Cc Receiver", cc));
                    if (!string.IsNullOrEmpty(bcc))
                        emailMessage.Bcc.Add(new MailboxAddress("Bcc Receiver", bcc));

                    emailMessage.Subject = subject;


                    BodyBuilder emailBodyBuilder = new BodyBuilder();
                    emailBodyBuilder.HtmlBody = body;
                    emailBodyBuilder.TextBody = "Plain Text goes here to avoid marked as spam for some email servers.";

                    emailMessage.Body = emailBodyBuilder.ToMessageBody();

                    using (SmtpClient mailClient = new SmtpClient())
                    {
                        mailClient.Connect(server, port, true);
                        mailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                        mailClient.Authenticate(fromEmail, password);
                        mailClient.Send(emailMessage);
                        mailClient.Disconnect(true);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Exception Details
                globalCls.WriteToFile(logPath, ex.ToString());
                return false;
            }

        }


        #endregion
    }
}
