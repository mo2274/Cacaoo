using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.CustomFilters;
using ChocolateDelivery.UI.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MySqlConnector;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

//using static System.Net.Mime.MediaTypeNames;

namespace ChocolateDelivery.UI.Controllers
{
    [Route("api")]
    [ApiController]
    public class WebApiController : ControllerBase
    {
        private readonly ChocolateDeliveryEntities _context;
        private readonly IConfiguration _config;
        private readonly string _logPath = "";
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WebApiController(ChocolateDeliveryEntities cc, IConfiguration config,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = cc;
            _config = config;
            _webHostEnvironment = webHostEnvironment;
            _logPath = Path.Combine(_webHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath"));
        }

        private Device_Registration? _securityDto = new Device_Registration();

        [Route("RegisterDevice")]
        [HttpPost]
        public RegisterDeviceResponse RegisterDevice(RegisterDeviceDTO registerDeviceDto)
        {
            var response = new RegisterDeviceResponse();
            try
            {
                var entryBc = new DeviceBC(_context, _logPath);
                var deviceRegisterDm = new Device_Registration();

                deviceRegisterDm.Device_Id = registerDeviceDto.UniqueDeviceId;
                deviceRegisterDm.Notification_Token = registerDeviceDto.NotificationToken;
                deviceRegisterDm.Client_Key = StaticMethods.Base64Decode(registerDeviceDto.ClientKey);
                deviceRegisterDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                ;
                deviceRegisterDm.Device_Type = registerDeviceDto.DeviceType;
                deviceRegisterDm.Notification_Enabled = true;
                deviceRegisterDm.NotificationSound_Enabled = true;
                deviceRegisterDm.Preferred_Language = string.IsNullOrEmpty(registerDeviceDto.Preferred_Language)
                    ? "E"
                    : registerDeviceDto.Preferred_Language;
                if (registerDeviceDto.AppType != null)
                {
                    deviceRegisterDm.App_Type = registerDeviceDto.AppType ?? AppTypes.CLIENT;
                }
                else
                {
                    deviceRegisterDm.App_Type = AppTypes.CLIENT;
                }

                deviceRegisterDm = entryBc.RegisterDevice(deviceRegisterDm);
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
                    globalCls.WriteToFile(_logPath, ex.ToString(), true);
                }

                #endregion
            }
            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(_logPath, ex.ToString(), true);
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

                        if (string.IsNullOrEmpty(registerRequest.Google_Id) &&
                            registerRequest.Login_Type == Login_Types.GOOGLE)
                        {
                            response.Status = 101;
                            response.Message = "Google Id cannot be empty";
                            return response;
                        }

                        if (string.IsNullOrEmpty(registerRequest.Facebook_Id) &&
                            registerRequest.Login_Type == Login_Types.FACEBOOK)
                        {
                            response.Status = 101;
                            response.Message = "Facebook Id cannot be empty";
                            return response;
                        }

                        if (string.IsNullOrEmpty(registerRequest.Apple_Id) &&
                            registerRequest.Login_Type == Login_Types.APPLE)
                        {
                            response.Status = 101;
                            response.Message = "Apple Id cannot be empty";
                            return response;
                        }

                        var appuserBc = new AppUserBC(_context);
                        var entryBc = new DeviceBC(_context);

                        var existingUser = appuserBc.GetAppUser(registerRequest.Email, registerRequest.Login_Type);
                        var deviceRegisterDm = entryBc.GetDeviceByClientKey(Base64Decode(securityKey));

                        if (existingUser == null)
                        {
                            var appuserDm = new App_Users();
                            appuserDm.Name = registerRequest.Name;
                            appuserDm.Email = registerRequest.Email;
                            appuserDm.Password = registerRequest.Password;
                            appuserDm.Mobile = registerRequest.Mobile;
                            appuserDm.Login_Type = registerRequest.Login_Type;
                            appuserDm.App_User_Type = App_User_Types.APP_USER;
                            appuserDm.Facebook_Id = registerRequest.Facebook_Id;
                            appuserDm.Google_Id = registerRequest.Google_Id;
                            appuserDm.Apple_Id = registerRequest.Apple_Id;
                            appuserDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                            appuserDm.Show = true;
                            appuserDm.Row_Id = Guid.NewGuid();

                            appuserDm = appuserBc.CreateAppUser(appuserDm);

                            #region Update Device details

                            deviceRegisterDm.App_User_Id = appuserDm.App_User_Id;
                            deviceRegisterDm.Logged_In = StaticMethods.GetKuwaitTime();
                            deviceRegisterDm = entryBc.RegisterDevice(deviceRegisterDm);

                            #endregion

                            response.Status = 0;
                            response.App_User_Id = appuserDm.Row_Id.ToString();
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
                                existingUser = appuserBc.CreateAppUser(existingUser);
                                deviceRegisterDm.App_User_Id = existingUser.App_User_Id;
                                deviceRegisterDm.Logged_In = StaticMethods.GetKuwaitTime();
                                deviceRegisterDm = entryBc.RegisterDevice(deviceRegisterDm);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var userBc = new App_Users();
                        var appuserBc = new AppUserBC(_context);
                        var entryBc = new DeviceBC(_context);
                        var user = appuserBc.ValidateAppUser(loginRequest.Email, loginRequest.Password);

                        if (user != null && user.App_User_Id != 0)
                        {
                            #region Update Device details

                            var deviceRegisterDm = entryBc.GetDeviceByClientKey(Base64Decode(securityKey));
                            deviceRegisterDm.App_User_Id = user.App_User_Id;
                            deviceRegisterDm.Logged_In = StaticMethods.GetKuwaitTime();
                            deviceRegisterDm = entryBc.RegisterDevice(deviceRegisterDm);

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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var entryBc = new DeviceBC(_context);
                        var appuserBc = new AppUserBC(_context);
                        var deviceDm = entryBc.GetDevice(_securityDto.Device_Id);

                        deviceDm.App_User_Id = null;
                        deviceDm.Logged_In = null;
                        deviceDm.Mobile = null;
                        deviceDm.Code = null;
                        var isLogout = entryBc.Logout(deviceDm);

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
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                var promterBc = new CommonBC(_context, _logPath);
                var currentevents = promterBc.GetAppLabels();

                foreach (var currentevent in currentevents)
                {
                    var eventsDto = new LabelDTO();
                    eventsDto.Label_Id = currentevent.Label_Id;
                    eventsDto.Label_Name_E = currentevent.L_Label_Name;
                    eventsDto.Label_Name_A = currentevent.A_Label_Name ?? "";
                    eventsDto.Label_Code = currentevent.Label_Code ?? "";
                    response.Labels.Add(eventsDto);
                }

                //var emailSent = SendHTMLMail("yusuf.9116@gmail.com","Test Email","<strong>This is test email</strong>","","", "chocopedia.map@gmail.com", "scsvwozhinefmzre", "Cacaoo");
                response.Message = ServiceResponse.Success;
                response.Status = 0;
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                        var categoryBc = new CategoryBC(_context);
                        var currentevents = categoryBc.GetCategories();
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        foreach (var currentevent in currentevents)
                        {
                            var eventsDto = new CategoryDTO();
                            eventsDto.Category_Id = currentevent.Category_Id;
                            eventsDto.Category_Name = lang == "A"
                                ? currentevent.Category_Name_A ?? currentevent.Category_Name_E
                                : currentevent.Category_Name_E;
                            eventsDto.Image_URL = currentevent.Image_URL ?? "";
                            eventsDto.Background_Color = currentevent.Background_Color ?? "";
                            var subCategories = categoryBc.GetSubCategories(currentevent.Category_Id);
                            foreach (SM_Sub_Categories subCategory in subCategories)
                            {
                                var subCategoryDto = new SubCategoryDTO();
                                subCategoryDto.Sub_Category_Id = subCategory.Sub_Category_Id;
                                subCategoryDto.Sub_Category_Name = lang == "A"
                                    ? subCategory.Sub_Category_Name_A ?? subCategory.Sub_Category_Name_E
                                    : subCategory.Sub_Category_Name_E;
                                subCategoryDto.Image_URL = subCategory.Image_URL ?? "";
                                subCategoryDto.Background_Color = subCategory.Background_Color ?? "";
                                eventsDto.SubCategories.Add(subCategoryDto);
                            }

                            response.Categories.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("SubCategories/{catId}")]
        [HttpGet]
        public SubCategoryResponse GetSubCategories(int catId)
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
                        var categoryBc = new CategoryBC(_context);

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        var subCategories = categoryBc.GetSubCategories(catId);
                        foreach (SM_Sub_Categories subCategory in subCategories)
                        {
                            var subCategoryDto = new SubCategoryDTO();
                            subCategoryDto.Sub_Category_Id = subCategory.Sub_Category_Id;
                            subCategoryDto.Sub_Category_Name = lang == "A"
                                ? subCategory.Sub_Category_Name_A ?? subCategory.Sub_Category_Name_E
                                : subCategory.Sub_Category_Name_E;
                            subCategoryDto.Image_URL = subCategory.Image_URL ?? "";
                            subCategoryDto.Background_Color = subCategory.Background_Color ?? "";
                            response.SubCategories.Add(subCategoryDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                        var areaBc = new AreaBC(_context);
                        var currentevents = areaBc.GetAreas();
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        foreach (var currentevent in currentevents)
                        {
                            var eventsDto = new AreaDTO();
                            eventsDto.Area_Id = currentevent.Area_Id;
                            eventsDto.Area_Name = lang == "A"
                                ? currentevent.Area_Name_A ?? currentevent.Area_Name_E
                                : currentevent.Area_Name_E;
                            response.Areas.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var carouselBc = new CarouselBC(_context);
                        var carousels = carouselBc.GetCarousels();
                        foreach (var currentevent in carousels)
                        {
                            var eventsDto = new CarouselDTO();
                            eventsDto.Carousel_Id = currentevent.Carousel_Id;
                            eventsDto.Carousel_Name = lang.ToUpper() == "A" ? currentevent.Title_A :
                                !string.IsNullOrEmpty(currentevent.Title_E) ? currentevent.Title_E :
                                currentevent.Title_A;
                            eventsDto.Carousel_Title = lang.ToUpper() == "A" ? currentevent.Sub_Title_A :
                                !string.IsNullOrEmpty(currentevent.Sub_Title_E) ? currentevent.Sub_Title_E :
                                currentevent.Sub_Title_A;
                            eventsDto.Media_Type = currentevent.Media_Type;
                            eventsDto.Media_From_Type = currentevent.Media_From_Type ?? 0;
                            eventsDto.Redirect_Id = currentevent.Redirect_Id ?? 0;
                            eventsDto.Media_Url = currentevent.Media_URL ?? "";
                            eventsDto.ThumbNail_Url = currentevent.Thumbnail_URL ?? "";
                            response.Carousels.Add(eventsDto);
                        }

                        var groupBc = new HomeGroupBC(_context);
                        var groups = groupBc.GetGroups();

                        var i = 0;
                        foreach (var group in groups)
                        {
                            var groupDm = new GroupDTO();
                            groupDm.Group_Id = group.Group_Id;
                            groupDm.Group_Name = lang.ToUpper() == "A" ? group.Group_Name_A :
                                !string.IsNullOrEmpty(group.Group_Name_E) ? group.Group_Name_E : group.Group_Name_A;
                            groupDm.Display_Type = group.Display_Type;

                            #region Group Items

                            var groupItems = groupBc.GetGroupDetails(group.Group_Id);
                            groupItems = (from o in groupItems
                                orderby o.Sequence
                                select o).ToList();


                            foreach (var currentevent in groupItems)
                            {
                                var eventsDto = new GeneralDTO();
                                eventsDto.Id = currentevent.Id;
                                eventsDto.Group_Type_Id = currentevent.Group_Type_Id;
                                eventsDto.Name = currentevent.Item_Name;
                                eventsDto.Image_Url = currentevent.Image_Url ?? "";
                                groupDm.GroupItems.Add(eventsDto);
                            }

                            #endregion

                            response.Groups.Add(groupDm);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                var invoiceBc = new HomeGroupBC(_context);
                var propertyUnits = invoiceBc.GetGroupItems(groupTypeId);
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
                globalCls.WriteToFile(_logPath, ex.ToString(), true);
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

                        var postBc = new ProductBC(_context);
                        var posts = postBc.GetAppProducts(productRequest);


                        foreach (var currentevent in posts.Items)
                        {
                            var eventsDto = new ProductDTO();
                            eventsDto.Product_Id = currentevent.Product_Id;
                            eventsDto.Product_Name = lang.ToUpper() == "E"
                                ? currentevent.Product_Name_E
                                : currentevent.Product_Name_A ?? "";
                            eventsDto.Product_Desc = lang.ToUpper() == "E"
                                ? currentevent.Product_Desc_E ?? ""
                                : currentevent.Product_Desc_A ?? "";
                            eventsDto.Price = currentevent.Price;
                            eventsDto.Image_Url = currentevent.Image_URL ?? "";
                            eventsDto.Brand_Name = lang.ToUpper() == "E"
                                ? currentevent.Brand_Name_E ?? ""
                                : currentevent.Brand_Name_A ?? "";
                            eventsDto.Is_Exclusive = currentevent.Is_Exclusive;
                            eventsDto.Is_Catering = currentevent.Is_Catering;
                            response.Products.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var postBc = new BrandBC(_context);
                        var posts = postBc.GetBrands(productRequest);


                        foreach (var currentevent in posts)
                        {
                            var eventsDto = new BrandDTO();
                            eventsDto.Brand_Id = currentevent.Restaurant_Id;
                            eventsDto.Brand_Name = lang.ToUpper() == "E"
                                ? currentevent.Restaurant_Name_E
                                : currentevent.Restaurant_Name_A ?? "";
                            eventsDto.Image_Url = currentevent.Image_URL ?? "";
                            eventsDto.Delivery_Time = currentevent.Delivery_Time ?? "";
                            eventsDto.Delivery_Charge = currentevent.Delivery_Charge;
                            eventsDto.Categories = currentevent.Categories;
                            eventsDto.Background_Color = currentevent.Background_Color ?? "";
                            response.Brands.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var restaurantBc = new RestaurantBC(_context);
                        var brandDm = restaurantBc.GetRestaurant(brandId);

                        BrandBC brandBc = new BrandBC(_context);

                        if (brandDm != null)
                        {
                            response.Brand_Name = lang.ToUpper() == "E"
                                ? brandDm.Restaurant_Name_E
                                : brandDm.Restaurant_Name_A ?? "";
                            response.Brand_Desc = lang.ToUpper() == "E"
                                ? brandDm.Restaurant_Desc_E ?? ""
                                : brandDm.Restaurant_Desc_A ?? "";
                            response.Image_Url = brandDm.Image_URL ?? "";
                            response.Delivery_Charge = brandDm.Delivery_Charge;
                            response.Delivery_Time = brandDm.Delivery_Time ?? "";
                            var categories = brandBc.GetBrandCategories(brandDm.Restaurant_Id);
                            if (lang == "A")
                            {
                                response.Brand_Categories = string.Join(",",
                                    categories.Select(x => x.Sub_Category_Name_E).ToList());
                            }
                            else
                            {
                                response.Brand_Categories = string.Join(",",
                                    categories.Select(x => x.Sub_Category_Name_A).ToList());
                            }

                            foreach (var cat in categories)
                            {
                                var categoryDto = new BrandCategoryDTO();
                                categoryDto.Category_Id = cat.Category_Id;
                                categoryDto.Category_Name = lang.ToUpper() == "E"
                                    ? cat.Sub_Category_Name_E
                                    : cat.Sub_Category_Name_A ?? "";
                                var products = brandBc.GetBrandCategoryProducts(brandId, cat.Sub_Category_Id);
                                foreach (var product in products)
                                {
                                    var productDto = new ProductDTO();
                                    productDto.Product_Id = product.Product_Id;
                                    productDto.Product_Name = lang.ToUpper() == "E"
                                        ? product.Product_Name_E
                                        : product.Product_Name_A ?? "";
                                    productDto.Product_Desc = lang.ToUpper() == "E"
                                        ? product.Product_Desc_E ?? ""
                                        : product.Product_Desc_A ?? "";
                                    productDto.Price = product.Price;
                                    productDto.Image_Url = product.Image_URL ?? "";
                                    productDto.Is_Exclusive = product.Is_Exclusive;
                                    productDto.Is_Catering = product.Is_Catering;
                                    categoryDto.Products.Add(productDto);
                                }

                                response.Categories.Add(categoryDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("Product/{productId}/{appUserId?}")]
        [HttpGet]
        public ProductDetailResponse GetProductDetailResponse(int productId, string? appUserId = "0")
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

                        var actualAppUserId = 0;
                        if (appUserId != "0")
                        {
                            AppUserBC appUserBc = new AppUserBC(_context);
                            var rowId = new Guid(appUserId);
                            var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                            if (appUserDm != null)
                            {
                                actualAppUserId = appUserDm.App_User_Id;
                            }
                            else
                            {
                                response.Status = 101;
                                response.Message = "App User Not Found";
                                return response;
                            }
                        }


                        var brandBc = new ProductBC(_context);
                        var brandDm = brandBc.GetProduct(productId, actualAppUserId);

                        if (brandDm != null)
                        {
                            response.Product_Name = lang.ToUpper() == "E"
                                ? brandDm.Product_Name_E
                                : brandDm.Product_Name_A ?? "";
                            response.Product_Desc = lang.ToUpper() == "E"
                                ? brandDm.Product_Desc_E ?? ""
                                : brandDm.Product_Desc_A ?? "";
                            response.Short_Desc = lang.ToUpper() == "E"
                                ? brandDm.Short_Desc_E ?? ""
                                : brandDm.Short_Desc_A ?? "";
                            response.Nutritional_Facts = lang.ToUpper() == "E"
                                ? brandDm.Nutritional_Facts_E ?? ""
                                : brandDm.Nutritional_Facts_A ?? "";
                            response.Image_Url = brandDm.Image_URL ?? "";
                            response.Price = brandDm.Price;
                            response.Avg_Rating = brandDm.Rating;
                            response.Total_Ratings = brandDm.Total_Ratings;
                            response.Is_Favorite = brandDm.Is_Favorite;
                            response.Is_Exclusive = brandDm.Is_Exclusive;
                            response.Is_Catering = brandDm.Is_Catering;
                            var addons = brandBc.GetProductAddOns(brandDm.Product_Id, lang);
                            response.AddOns.AddRange(addons);
                            var cateringProducts = brandBc.GetProductCateringProducts(brandDm.Product_Id, lang);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var chefBc = new ChefBC(_context);
                        var chefDm = chefBc.GetChef(chefId);

                        if (chefDm != null)
                        {
                            response.Chef_Name = lang.ToUpper() == "E" ? chefDm.Chef_Name_E : chefDm.Chef_Name_A ?? "";
                            response.Chef_Desc = lang.ToUpper() == "E"
                                ? chefDm.Chef_Desc_E ?? ""
                                : chefDm.Chef_Desc_A ?? "";
                            response.Image_Url = chefDm.Image_URL ?? "";


                            var products = chefBc.GetChefProducts(chefId);
                            foreach (var currentevent in products.Items)
                            {
                                var eventsDto = new ProductDTO();
                                eventsDto.Product_Id = currentevent.Product_Id;
                                eventsDto.Product_Name = lang.ToUpper() == "E"
                                    ? currentevent.Product_Name_E
                                    : currentevent.Product_Name_A ?? "";
                                eventsDto.Product_Desc = lang.ToUpper() == "E"
                                    ? currentevent.Product_Desc_E ?? ""
                                    : currentevent.Product_Desc_A ?? "";
                                eventsDto.Price = currentevent.Price;
                                eventsDto.Image_Url = currentevent.Image_URL ?? "";
                                eventsDto.Brand_Name = lang.ToUpper() == "E"
                                    ? currentevent.Brand_Name_E ?? ""
                                    : currentevent.Brand_Name_A ?? "";
                                response.Products.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var cartBc = new CartBC(_context);

                        var rowId = new Guid(cartRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            if (cartRequest.Cart_Id != 0)
                            {
                                var cartExist = cartBc.GetCart(cartRequest.Cart_Id);
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

                            var isValidVendor = cartBc.IsValidVendor(appUserDm.App_User_Id, cartRequest.Product_Id);
                            if (!isValidVendor)
                            {
                                response.Status = 101;
                                response.Message = "You cannot add product from different vendor in cart";
                                return response;
                            }

                            var isValidCategory = cartBc.IsValidCategory(appUserDm.App_User_Id, cartRequest.Product_Id);
                            if (!isValidCategory)
                            {
                                response.Status = 101;
                                response.Message = "You cannot add gift and normal item together in cart";
                                return response;
                            }

                            var cartDm = new TXN_Cart();
                            cartDm.Cart_Id = cartRequest.Cart_Id;
                            cartDm.App_User_Id = appUserDm.App_User_Id;
                            cartDm.Product_Id = cartRequest.Product_Id;
                            cartDm.Qty = cartRequest.Qty;
                            cartDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                            cartDm.Comments = cartRequest.Comments;
                            cartBc.AddtoCart(cartDm);

                            if (cartRequest.Cart_Id == 0)
                            {
                                foreach (var addonId in cartRequest.Product_AddOnIds)
                                {
                                    var addonDm = new TXN_Cart_AddOns();
                                    addonDm.Product_AddOnId = addonId;
                                    addonDm.Cart_Id = cartDm.Cart_Id;
                                    cartBc.CreateCartAddOn(addonDm);
                                }

                                foreach (var cateringProduct in cartRequest.Catering_Products)
                                {
                                    var addonDm = new TXN_Cart_Catering_Products();
                                    addonDm.Catering_Product_Id = cateringProduct.Catering_Product_Id;
                                    addonDm.Qty = cateringProduct.Qty;
                                    addonDm.Cart_Id = cartDm.Cart_Id;
                                    cartBc.CreateCartCateringProduct(addonDm);
                                }
                            }

                            response.Status = 0;
                            response.Cart_Id = cartDm.Cart_Id;
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("Cart/{appUserId}")]
        [HttpGet]
        public CartResponse GetCart(string appUserId)
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
                        var cartBc = new CartBC(_context);

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        if (string.IsNullOrEmpty(appUserId))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBc = new AppUserBC(_context);


                        var rowId = new Guid(appUserId);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            response = cartBc.GetCartItems(appUserDm.App_User_Id, lang);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var cartBc = new CartBC(_context);

                        var rowId = new Guid(cartRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var cartExist = cartBc.GetCart(cartRequest.Cart_Id);
                            if (cartExist == null)
                            {
                                response.Status = 101;
                                response.Message = "Cart Id not found";
                                return response;
                            }
                            else
                            {
                                var isRemoved = cartBc.RemoveCartItem(cartRequest.Cart_Id);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("User/{appUserId}")]
        [HttpGet]
        public ProfileResponse GetUserProfile(string appUserId)
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

                        AppUserBC appUserBc = new AppUserBC(_context);

                        var rowId = new Guid(appUserId);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            response.Name = appUserDm.Name;
                            response.Mobile = appUserDm.Mobile;
                            response.Email = appUserDm.Email;
                            response.Login_Type = appUserDm.Login_Type;
                            response.Redeem_Points = appUserDm.Current_Points;
                            response.Plate_Num = appUserDm.Plate_Num;
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var cartBc = new CartBC(_context);

                        var rowId = new Guid(updatePofileRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            appUserDm.Name = updatePofileRequest.Name;
                            if (updatePofileRequest.Mobile.Length == 8)
                            {
                                updatePofileRequest.Mobile = "+965" + updatePofileRequest.Mobile;
                            }

                            appUserDm.Mobile = updatePofileRequest.Mobile;
                            appUserBc.CreateAppUser(appUserDm);

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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var cartBc = new CartBC(_context);

                        var rowId = new Guid(updatePasswordRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            if (appUserDm.Password != updatePasswordRequest.Old_Password)
                            {
                                response.Status = 101;
                                response.Message = "Old Password not matches with existing password";
                                return response;
                            }

                            appUserDm.Password = updatePasswordRequest.New_Password;
                            appUserBc.CreateAppUser(appUserDm);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var cartBc = new CartBC(_context);

                        var rowId = new Guid(updateAddressRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            App_User_Address addressDm = new App_User_Address();
                            addressDm.Address_Id = updateAddressRequest.Address_Id;
                            addressDm.App_User_Id = appUserDm.App_User_Id;
                            addressDm.Address_Name = updateAddressRequest.Address_Name;
                            addressDm.Block = updateAddressRequest.Block;
                            addressDm.Street = updateAddressRequest.Street;
                            addressDm.Building = updateAddressRequest.Building;
                            addressDm.Avenue = updateAddressRequest.Avenue;
                            addressDm.Floor = updateAddressRequest.Floor;
                            addressDm.Apartment = updateAddressRequest.Apartment;
                            addressDm.Area_Id = updateAddressRequest.Area_Id;
                            addressDm.Mobile = updateAddressRequest.Mobile;
                            addressDm.Extra_Direction = updateAddressRequest.Extra_Direction;
                            addressDm.House_No = updateAddressRequest.House_No;
                            addressDm.Latitude = updateAddressRequest.Latitude;
                            addressDm.Longitude = updateAddressRequest.Longitude;
                            addressDm.Paci_Number = updateAddressRequest.Paci_Number;
                            addressDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                            addressDm.Updated_Datetime = StaticMethods.GetKuwaitTime();
                            appUserBc.CreateAppUserAddress(addressDm);

                            response.Status = 0;
                            response.Message = ServiceResponse.Success;
                            response.Address_Id = addressDm.Address_Id;
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("UserAddresses/{appUserId}")]
        [HttpGet]
        public UserAddressResponse GetUserAddresses(string appUserId)
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
                        var cartBc = new CartBC(_context);

                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        if (string.IsNullOrEmpty(appUserId))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBc = new AppUserBC(_context);


                        var rowId = new Guid(appUserId);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var addresses = appUserBc.GetUserAddresses(appUserDm.App_User_Id);
                            foreach (var address in addresses)
                            {
                                var addressDto = new AddressDTO();
                                addressDto.Address_Id = address.Address_Id;
                                addressDto.Address_Name = address.Address_Name;
                                addressDto.Block = address.Block;
                                addressDto.Street = address.Street;
                                addressDto.Building = address.Building;
                                addressDto.Avenue = address.Avenue ?? "";
                                addressDto.Floor = address.Floor ?? "";
                                addressDto.Apartment = address.Apartment ?? "";
                                addressDto.Area_Id = address.Area_Id;
                                addressDto.Mobile = address.Mobile ?? "";
                                addressDto.Extra_Direction = address.Extra_Direction ?? "";
                                addressDto.House_No = address.House_No ?? "";
                                addressDto.Area_Name = address.Area_Name;
                                addressDto.Latitude = address.Latitude;
                                addressDto.Longitude = address.Longitude;
                                addressDto.Paci_Number = address.Paci_Number ?? "";
                                response.UserAddresses.Add(addressDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                        AppUserBC appUserBc = new AppUserBC(_context);
                        var deleteAddress = appUserBc.DeleteUserAddress(userAddressId);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                        var areaBc = new OrderBC(_context);
                        var currentevents = areaBc.GetPaymentTypes();
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        foreach (var currentevent in currentevents)
                        {
                            var eventsDto = new PaymentTypeDTO
                            {
                                Type_Id = currentevent.Payment_Type_Id,
                                Type_Name = lang == "A"
                                    ? currentevent.Payment_Type_Name_A ?? currentevent.Payment_Type_Name_E
                                    : currentevent.Payment_Type_Name_E,
                                Icon = $"{Request.Scheme}://{Request.Host}{currentevent.icon}"
                            };
                            response.PaymentTypes.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                            globalCls.WriteToFile(_logPath,
                                "Order Request:" + JsonConvert.SerializeObject(orderRequest));
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

                            if (orderRequest.Delivery_Type == Delivery_Types.DELIVERY &&
                                (orderRequest.Address_Id == null || orderRequest.Address_Id == 0))
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

                            if (orderRequest.Delivery_Type == Delivery_Types.PICKUP &&
                                string.IsNullOrEmpty(orderRequest.Pickup_Datetime))
                            {
                                response.Status = 101;
                                response.Message = "Please choose pick up time";
                                return response;
                            }

                            AppUserBC appUserBc = new AppUserBC(_context);
                            var cartBc = new CartBC(_context);
                            ProductBC productBc = new ProductBC(_context);
                            RestaurantBC restaurantBc = new RestaurantBC(_context);
                            OrderBC orderBc = new OrderBC(_context);

                            var firstprodDm = productBc.GetProduct(orderRequest.OrderDetails.FirstOrDefault().Prod_Id);
                            if (orderRequest.Delivery_Type == Delivery_Types.DELIVERY && firstprodDm.Is_Gift_Product &&
                                string.IsNullOrEmpty(orderRequest.Pickup_Datetime))
                            {
                                response.Status = 101;
                                response.Message = "Please choose delivery time";
                                return response;
                            }

                            var rowId = new Guid(orderRequest.App_User_Id);
                            var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                            if (appUserDm != null)
                            {
                                short orderType = OrderTypes.NORMAL;
                                var grossAmt = decimal.Zero;
                                var deliveryCharge = decimal.Zero;
                                var redeemAmt = orderRequest.Redeem_Amount ?? 0;
                                SM_Restaurants restaurantDm = new SM_Restaurants();
                                foreach (var detail in orderRequest.OrderDetails)
                                {
                                    var prodDm = productBc.GetProduct(detail.Prod_Id);
                                    if (prodDm == null)
                                    {
                                        response.Status = 101;
                                        response.Message = "Some or all products not found";
                                        return response;
                                    }
                                    else
                                    {
                                        restaurantDm = restaurantBc.GetRestaurant(prodDm.Restaurant_Id);
                                        if (orderRequest.Delivery_Type == Delivery_Types.DELIVERY)
                                            deliveryCharge = restaurantDm.Delivery_Charge;

                                        grossAmt += prodDm.Price * detail.Qty;
                                        detail.Gross_Amount = prodDm.Price * detail.Qty;
                                        detail.Amount = prodDm.Price * detail.Qty;
                                        detail.Rate = prodDm.Price;
                                        detail.Prod_Name = prodDm.Product_Name_E;
                                        if (prodDm.Is_Gift_Product)
                                        {
                                            orderType = OrderTypes.GIFT;
                                        }

                                        foreach (var addonId in detail.Product_AddOn_Ids)
                                        {
                                            var addOnDm = productBc.GetProductAddOn(addonId);
                                            if (addOnDm == null)
                                            {
                                                response.Status = 101;
                                                response.Message = "Invalid AddonId for product :" +
                                                                   prodDm.Product_Name_E;
                                                return response;
                                            }
                                            else
                                            {
                                                grossAmt += addOnDm.Price;
                                                detail.Gross_Amount = detail.Gross_Amount + addOnDm.Price;
                                            }
                                        }

                                        foreach (var categoryProduct in detail.Catering_Products)
                                        {
                                            var cateringProductDm =
                                                productBc.GetProductCateringProduct(categoryProduct
                                                    .Catering_Product_Id);
                                            if (cateringProductDm == null)
                                            {
                                                response.Status = 101;
                                                response.Message = "Invalid Catering_Product_Id : " +
                                                                   categoryProduct.Catering_Product_Id +
                                                                   " for product :" + prodDm.Product_Name_E;
                                                return response;
                                            }
                                            else if (cateringProductDm.Product_Id != detail.Prod_Id)
                                            {
                                                response.Status = 101;
                                                response.Message = " Catering_Product_Id : " +
                                                                   categoryProduct.Catering_Product_Id +
                                                                   " does not belong to product :" +
                                                                   prodDm.Product_Name_E;
                                                return response;
                                            }
                                        }
                                    }
                                }

                                var netAmt = grossAmt + deliveryCharge - redeemAmt;

                                var orderDm = new TXN_Orders();
                                orderDm.Order_No = orderBc.GetNextOrderNo();
                                orderDm.Order_Serial = "ORD_" + orderDm.Order_No.ToString("D6");
                                orderDm.Order_Datetime = StaticMethods.GetKuwaitTime();
                                orderDm.App_User_Id = appUserDm.App_User_Id;
                                orderDm.Address_Id = orderRequest.Address_Id;
                                orderDm.Payment_Type_Id = orderRequest.Payment_Type_Id;
                                orderDm.Channel_Id = orderRequest.Channel_Id;
                                orderDm.Delivery_Type = orderRequest.Delivery_Type;
                                orderDm.Comments = orderRequest.Remarks;
                                orderDm.Cust_Name = orderRequest.Cust_Name;
                                orderDm.Email = orderRequest.Email;
                                orderDm.Mobile = orderRequest.Mobile;
                                orderDm.Gross_Amount = grossAmt;
                                orderDm.Discount_Amount = 0;
                                orderDm.Delivery_Charges = deliveryCharge;
                                orderDm.Net_Amount = netAmt;
                                if (orderRequest.Payment_Type_Id == PaymentTypes.Cash)
                                {
                                    //orderDM.Status_Id = OrderStatus.ORDER_RECEIVED;
                                    orderDm.Status_Id = OrderStatus.ORDER_PREPARING;
                                }
                                else if (orderRequest.Payment_Type_Id == PaymentTypes.KNET ||
                                         orderRequest.Payment_Type_Id == PaymentTypes.CreditCard ||
                                         orderRequest.Payment_Type_Id == PaymentTypes.ApplePay)
                                {
                                    orderDm.Status_Id = OrderStatus.ORDER_PROCESSING_PAYMENT;
                                }

                                orderDm.Promo_Code = orderRequest.Promo_Code;
                                orderDm.Restaurant_Id = restaurantDm.Restaurant_Id;
                                orderDm.Row_Id = Guid.NewGuid();
                                orderDm.Branch_Id = orderRequest.Branch_Id;
                                orderDm.Order_Type = orderType;
                                orderDm.Gift_Msg = orderRequest.Gift_Msg;
                                orderDm.Recepient_Name = orderRequest.Recepient_Name;
                                orderDm.Recepient_Mobile = orderRequest.Recepient_Mobile;
                                orderDm.Video_Link = orderRequest.Video_Link;
                                orderDm.Show_Sender_Name = true;
                                if (orderRequest.Show_Sender_Name != null)
                                {
                                    orderDm.Show_Sender_Name = orderRequest.Show_Sender_Name ?? true;
                                }

                                if (!string.IsNullOrEmpty(orderRequest.Video_File))
                                {
                                    byte[] bytes = Convert.FromBase64String(orderRequest.Video_File);
                                    var videoPathDir = "assets/videos/";
                                    var fileName = StaticMethods.GetKuwaitTime().ToString("yyyyMMddHHmmssfff") + ".mp4";
                                    var path = Path.Combine(_webHostEnvironment.WebRootPath, videoPathDir);
                                    var filePath = Path.Combine(path, fileName);
                                    System.IO.File.WriteAllBytes(filePath, bytes);

                                    orderDm.Video_File_Path = videoPathDir + fileName;
                                }

                                orderDm.Redeem_Amount = redeemAmt;
                                orderDm.Redeem_Points = orderRequest.Redeem_Points ?? 0;
                                if (!string.IsNullOrEmpty(orderRequest.Pickup_Datetime))
                                {
                                    orderDm.Pickup_Datetime = DateTime.ParseExact(orderRequest.Pickup_Datetime,
                                        "dd-MMM-yyyy hh:mm tt", CultureInfo.InvariantCulture);
                                }

                                orderBc.SaveOrder(orderDm);

                                foreach (var detail in orderRequest.OrderDetails)
                                {
                                    var orderDetailDm = new TXN_Order_Details();
                                    orderDetailDm.Order_Id = orderDm.Order_Id;
                                    orderDetailDm.Product_Id = detail.Prod_Id;
                                    orderDetailDm.Product_Name = detail.Prod_Name;
                                    orderDetailDm.Qty = detail.Qty;
                                    orderDetailDm.Rate = detail.Rate;
                                    orderDetailDm.Amount = detail.Amount;
                                    orderDetailDm.Gross_Amount = detail.Gross_Amount;
                                    orderDetailDm.Discount_Amount = 0;
                                    orderDetailDm.Net_Amount =
                                        orderDetailDm.Gross_Amount - orderDetailDm.Discount_Amount;
                                    orderDetailDm.Promo_Code = detail.Promo_Code;
                                    orderDetailDm.Comments = detail.Remarks;
                                    orderBc.SaveOrderDetail(orderDetailDm);

                                    foreach (var addonId in detail.Product_AddOn_Ids)
                                    {
                                        var addOnDm = productBc.GetProductAddOn(addonId);
                                        TXN_Order_Detail_AddOns addOns = new TXN_Order_Detail_AddOns();
                                        addOns.Order_Detail_Id = orderDetailDm.Order_Detail_Id;
                                        addOns.Product_AddOnId = addonId;
                                        addOns.Price = addOnDm.Price;
                                        orderBc.SaveOrderDetailAddon(addOns);
                                    }

                                    foreach (var cateringProduct in detail.Catering_Products)
                                    {
                                        TXN_Order_Detail_Catering_Products addOns =
                                            new TXN_Order_Detail_Catering_Products();
                                        addOns.Detail_Id = orderDetailDm.Order_Detail_Id;
                                        addOns.Category_Product_Id = cateringProduct.Catering_Product_Id;
                                        addOns.Qty = cateringProduct.Qty;
                                        orderBc.SaveOrderDetailCateringProduct(addOns);
                                    }
                                }

                                TXN_Order_Logs log = new TXN_Order_Logs();
                                log.Order_Id = orderDm.Order_Id;
                                log.Status_Id = orderDm.Status_Id;
                                log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                log.Comments = "Order Placed with Order #" + orderDm.Order_Serial;
                                orderBc.CreateOrderLog(log);


                                if (orderRequest.Payment_Type_Id == PaymentTypes.Cash)
                                {
                                    SendOrderEmail(orderDm.Order_Id);

                                    #region clear cart after successful order

                                    var removeCart = cartBc.RemoveCart(appUserDm.App_User_Id);

                                    #endregion

                                    NotificationBC notificationBc = new NotificationBC(_context, _logPath);
                                    notificationBc.SendNotificationToDriver(orderDm.Order_Serial);

                                    APP_PUSH_CAMPAIGN campaignDm = new APP_PUSH_CAMPAIGN();
                                    campaignDm.Title_E = "Pick up Request";
                                    campaignDm.Desc_E = "Please accept Order # " + orderDm.Order_Serial +
                                                        " for delivery";
                                    campaignDm.Title_A = "Pick up Request";
                                    campaignDm.Desc_A = "Please accept Order # " + orderDm.Order_Serial +
                                                        " for delivery";
                                    campaignDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                                    notificationBc.CreatePushCampaign(campaignDm);

                                    string connectionString =
                                        _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                                    using (MySqlConnection con = new MySqlConnection(connectionString))
                                    {
                                        con.Open();
                                        var time = con.ConnectionTimeout;
                                        using (MySqlCommand cmd = new MySqlCommand("InsertNotifications", con))
                                        {
                                            using (var da = new MySqlDataAdapter(cmd))
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;
                                                cmd.Parameters.AddWithValue("@Campaign_Id", campaignDm.Campaign_Id);
                                                cmd.ExecuteReader();
                                            }
                                        }
                                    }
                                }


                                if (orderRequest.Payment_Type_Id != PaymentTypes.Cash)
                                {
                                    var paymentId = _context.sm_payment_types
                                        .Where(s => s.Payment_Type_Id == orderRequest.Payment_Type_Id)
                                        .Select(s => s.tap_payment_id)
                                        .First();

                                    TapChargeRequest tapChargeRequest = new TapChargeRequest();
                                    tapChargeRequest.amount = netAmt;
                                    tapChargeRequest.currency = "KWD";
                                    //tapChargeRequest.source = new Source { id = "src_kw.knet" };
                                    tapChargeRequest.source = new Source { id = paymentId };
                                    tapChargeRequest.reference = new Reference
                                        { transaction = orderDm.Order_Serial, order = orderDm.Order_Id.ToString() };
                                    tapChargeRequest.receipt = new Receipt { email = true, sms = true };
                                    TapCustomer customer = new TapCustomer();
                                    customer.first_name = orderDm.Cust_Name;
                                    customer.email = orderDm.Email;
                                    Phone phone = new Phone();
                                    if (orderDm.Mobile.Length == 12)
                                    {
                                        phone.country_code = Convert.ToInt32(orderDm.Mobile.Substring(1, 3));
                                        phone.number = Convert.ToInt32(orderDm.Mobile.Substring(4));
                                    }
                                    else if (orderDm.Mobile.Length == 8)
                                    {
                                        phone.country_code = 965;
                                        phone.number = Convert.ToInt32(orderDm.Mobile);
                                    }

                                    customer.phone = phone;
                                    tapChargeRequest.customer = customer;
                                    var callBackUrl = _config.GetValue<string>("TapPayment:CallBackURL");
                                    tapChargeRequest.redirect = new Redirect { url = callBackUrl };
                                    var chargeResponse = TapPayment.CreateChargeRequest(tapChargeRequest, _config);
                                    if (chargeResponse != null)
                                    {
                                        string redirectUrl = string.Empty;
                                        if (chargeResponse.transaction != null &&
                                            !string.IsNullOrEmpty(chargeResponse.transaction.url))
                                            redirectUrl = chargeResponse.transaction.url;

                                        if (!string.IsNullOrEmpty(redirectUrl))
                                        {
                                            PAYMENTS paymentDm = new PAYMENTS();
                                            paymentDm.Order_Id = orderDm.Order_Id;
                                            paymentDm.Amount = netAmt;
                                            paymentDm.Track_Id = chargeResponse.id;
                                            paymentDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                                            paymentDm.Comments = "";
                                            orderBc.CreatePayment(paymentDm);

                                            globalCls.WriteToFile(_logPath, "Redirecting to :" + redirectUrl, true);
                                            response.Payment_Link = redirectUrl;
                                        }
                                    }
                                }

                                response.OrderId = orderDm.Order_Id;
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
                    globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var orderBc = new OrderBC(_context);
                        var orderDm = orderBc.GetOrder(orderId);

                        if (orderDm != null)
                        {
                            response.Order_No = orderDm.Order_Serial;
                            response.Order_Date = orderDm.Order_Datetime.ToString("dd-MMM-yyyy hh:mm tt");
                            response.Cust_Name = orderDm.Cust_Name;
                            response.Mobile = orderDm.Mobile ?? "";
                            response.Email = orderDm.Email;

                            var pickAddressDm = new AddressCoordinatesDTO();
                            pickAddressDm.Address = orderDm.Branch_Address;
                            pickAddressDm.Latitude = orderDm.Branch_Latitude;
                            pickAddressDm.Longitude = orderDm.Branch_Longitude;
                            response.Pickup_Address = pickAddressDm;

                            var deliveryAddressDm = new AddressCoordinatesDTO();
                            deliveryAddressDm.Address = orderDm.Full_Address;
                            deliveryAddressDm.Latitude = orderDm.User_Address_Latitude;
                            deliveryAddressDm.Longitude = orderDm.User_Address_Longitude;
                            response.Delivery_Address = deliveryAddressDm;

                            response.Payment_Type = orderDm.Payment_Type_Name;
                            response.Order_Status_Id = orderDm.Status_Id;
                            response.Order_Status = orderDm.Status_Name;
                            response.Delivery_Option = orderDm.Delivery_Type_Name;
                            response.Sub_Total = orderDm.Gross_Amount;
                            response.Delivery_Charges = orderDm.Delivery_Charges;
                            response.Total = orderDm.Net_Amount;
                            response.Driver_Name = orderDm.Driver_Name;
                            response.Remarks = orderDm.Comments ?? "";
                            response.Current_Driver_Latitude = orderDm.Driver_Latitude;
                            response.Current_Driver_Longitude = orderDm.Driver_Longitude;

                            foreach (var detail in orderDm.TXN_Order_Details)
                            {
                                var detailDm = new DriverOrderDetailDTO();
                                detailDm.Order_Detail_Id = detail.Order_Detail_Id;
                                detailDm.Prod_Name = detail.Full_Product_Name;
                                detailDm.Qty = detail.Qty;
                                detailDm.Rate = detail.Rate;
                                detailDm.Gross_Amount = detail.Amount;
                                detailDm.AddOn_Amount = detail.Gross_Amount - detail.Amount;
                                detailDm.Net_Amount = detail.Gross_Amount;
                                detailDm.Remarks = detail.Comments ?? "";
                                var cateringProducts =
                                    orderBc.GetOrderDetailCategoryProducts(detail.Order_Detail_Id, lang);
                                foreach (var product in cateringProducts)
                                {
                                    CartCateringProductsDTO cateringProductsDto = new CartCateringProductsDTO();
                                    cateringProductsDto.Catering_Product_Id = product.Category_Product_Id;
                                    cateringProductsDto.Catering_Product = product.Product_Name;
                                    cateringProductsDto.Qty = product.Qty;
                                    detailDm.Catering_Products.Add(cateringProductsDto);
                                }

                                response.OrderDetails.Add(detailDm);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var orderBc = new OrderBC(_context);
                        var orderDm = orderBc.GetPaymentByOrderId(orderId);

                        if (orderDm != null)
                        {
                            response.Order_Id = orderDm.Order_Id ?? 0;
                            response.Payment_Date = Convert.ToDateTime(orderDm.Created_Datetime)
                                .ToString("dd-MMM-yyyy hh:mm tt");
                            response.Trans_Id = orderDm.Trans_Id ?? "";
                            response.Payment_Id = orderDm.Payment_Id ?? "";
                            response.Tap_Id = orderDm.Track_Id ?? "";
                            response.Auth = orderDm.Auth ?? "";
                            response.Reference_No = orderDm.Reference_No ?? "";
                            response.Result = orderDm.Result ?? "";
                            response.Payment_Mode = orderDm.Payment_Mode ?? "";
                            response.Order_Amount = orderDm.Amount;
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var orderBc = new OrderBC(_context);
                        var orderDm = orderBc.GetLatestTrackingDetail(orderId);

                        if (orderDm != null)
                        {
                            response.Latitude = orderDm.Latitude;
                            response.Longitude = orderDm.Longitude;
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                        var areaBc = new CommonBC(_context, _logPath);
                        var currentevents = areaBc.GetSettings();
                        var lang = "E";
                        if (Request.Headers.ContainsKey("X-Cacaoo-Lang") && Request.Headers["X-Cacaoo-Lang"] == "A")
                        {
                            lang = "A";
                        }

                        foreach (var currentevent in currentevents)
                        {
                            var eventsDto = new SettingDTO();
                            eventsDto.Setting_Id = currentevent.SETTING_ID;
                            eventsDto.Setting_Name = currentevent.SETTING_NAME;
                            eventsDto.Setting_Value = currentevent.SETTING_VALUE;
                            response.Settings.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var commonBc = new CommonBC(_context, _logPath);

                        var rowId = new Guid(favoriteRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var isFavExist = commonBc.GetFavorite(appUserDm.App_User_Id, favoriteRequest.Product_Id);
                            if (isFavExist == null)
                            {
                                TXN_Favorite favoriteDm = new TXN_Favorite();
                                favoriteDm.Product_Id = favoriteRequest.Product_Id;
                                favoriteDm.App_User_Id = appUserDm.App_User_Id;
                                favoriteDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                                commonBc.AddFavorite(favoriteDm);

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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("FavoriteProducts/{appUserId}")]
        [HttpGet]
        public ProductsResponse GetFavoriteProducts(string appUserId)
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

                        if (string.IsNullOrEmpty(appUserId))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBc = new AppUserBC(_context);


                        var rowId = new Guid(appUserId);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var commonBc = new CommonBC(_context, _logPath);
                            var posts = commonBc.GetFavoriteProducts(appUserDm.App_User_Id);


                            foreach (var currentevent in posts.Items)
                            {
                                var eventsDto = new ProductDTO();
                                eventsDto.Product_Id = currentevent.Product_Id;
                                eventsDto.Product_Name = lang.ToUpper() == "E"
                                    ? currentevent.Product_Name_E
                                    : currentevent.Product_Name_A ?? "";
                                eventsDto.Product_Desc = lang.ToUpper() == "E"
                                    ? currentevent.Product_Desc_E ?? ""
                                    : currentevent.Product_Desc_A ?? "";
                                eventsDto.Price = currentevent.Price;
                                eventsDto.Image_Url = currentevent.Image_URL ?? "";
                                eventsDto.Brand_Name = lang.ToUpper() == "E"
                                    ? currentevent.Brand_Name_E ?? ""
                                    : currentevent.Brand_Name_A ?? "";
                                response.Products.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var commonBc = new CommonBC(_context, _logPath);

                        var rowId = new Guid(favoriteRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var isFavExist = commonBc.GetFavorite(appUserDm.App_User_Id, favoriteRequest.Product_Id);
                            if (isFavExist != null)
                            {
                                var isRemoved = commonBc.RemoveFavorite(isFavExist.Favorite_Id);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        #endregion

        #region Driver

        [Route("DriverPendingOrders/{appUserId}")]
        [HttpGet]
        public DriverOrderResponse GetDriverPendingOrders(string appUserId)
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

                        if (string.IsNullOrEmpty(appUserId))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBc = new AppUserBC(_context);


                        var rowId = new Guid(appUserId);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var areaBc = new OrderBC(_context);
                            var currentevents = areaBc.GetPendingDriverOrders(lang, appUserDm.App_User_Id);

                            foreach (var currentevent in currentevents)
                            {
                                var eventsDto = new DriverOrderDTO();
                                eventsDto.Order_Id = currentevent.Order_Id;
                                eventsDto.Order_No = currentevent.Order_Serial;
                                eventsDto.Pickup_Address = currentevent.Branch_Address;
                                eventsDto.Delivery_Address = currentevent.Full_Address;
                                response.Orders.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var orderBc = new OrderBC(_context);

                        var rowId = new Guid(driverOrderRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var orderDm = orderBc.GetOrder(driverOrderRequest.Order_Id);
                            if (orderDm != null)
                            {
                                if (orderDm.Status_Id == OrderStatus.ORDER_PREPARING ||
                                    orderDm.Status_Id == OrderStatus.ORDER_PAID)
                                {
                                    var existingDeliveringOrder = orderBc.GetExistingDriverOrder(appUserDm.App_User_Id);
                                    if (existingDeliveringOrder != null &&
                                        driverOrderRequest.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER)
                                    {
                                        response.Status = 101;
                                        response.Message = "Please first deliver Order # " +
                                                           existingDeliveringOrder.Order_Serial +
                                                           " and then accept this order";
                                        return response;
                                    }

                                    if (driverOrderRequest.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER)
                                    {
                                        orderDm.Driver_Id = appUserDm.App_User_Id;
                                        orderDm.Status_Id = OrderStatus.ACCEPTED_BY_DRIVER;
                                        orderBc.SaveOrder(orderDm);
                                    }

                                    if (driverOrderRequest.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER ||
                                        driverOrderRequest.Status_Id == OrderStatus.DECLINED_BY_DRIVER)
                                    {
                                        TXN_Order_Logs log = new TXN_Order_Logs();
                                        log.Order_Id = orderDm.Order_Id;
                                        log.Status_Id = orderDm.Status_Id;
                                        log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        log.Driver_Id = appUserDm.App_User_Id;
                                        if (driverOrderRequest.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER)
                                            log.Comments = "Order Accepted By Driver";
                                        else if (driverOrderRequest.Status_Id == OrderStatus.DECLINED_BY_DRIVER)
                                            log.Comments = "Order Declined By Driver";
                                        orderBc.CreateOrderLog(log);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var orderBc = new OrderBC(_context);

                        var rowId = new Guid(driverOrderRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var orderDm = orderBc.GetOrder(driverOrderRequest.Order_Id);
                            if (orderDm != null)
                            {
                                if (orderDm.Driver_Id == appUserDm.App_User_Id)
                                {
                                    if (orderDm.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER)
                                    {
                                        byte[] bytes = Convert.FromBase64String(driverOrderRequest.Delivery_Image);
                                        var imagePathDir = "assets/images/delivery/";
                                        var fileName = StaticMethods.GetKuwaitTime().ToString("yyyyMMddHHmmssfff") +
                                                       ".png";
                                        var path = Path.Combine(_webHostEnvironment.WebRootPath, imagePathDir);
                                        var filePath = Path.Combine(path, fileName);
                                        System.IO.File.WriteAllBytes(filePath, bytes);

                                        orderDm.Pickup_Image = imagePathDir + fileName;
                                        orderDm.Status_Id = OrderStatus.OUT_FOR_DELIVERY;
                                        orderBc.SaveOrder(orderDm);

                                        TXN_Order_Logs log = new TXN_Order_Logs();
                                        log.Order_Id = orderDm.Order_Id;
                                        log.Status_Id = orderDm.Status_Id;
                                        log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        log.Driver_Id = appUserDm.App_User_Id;
                                        log.Comments = "Order Picked up By Driver";
                                        orderBc.CreateOrderLog(log);

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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var orderBc = new OrderBC(_context);

                        var rowId = new Guid(driverOrderRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var orderDm = orderBc.GetOrder(driverOrderRequest.Order_Id);
                            if (orderDm != null)
                            {
                                if (orderDm.Driver_Id == appUserDm.App_User_Id)
                                {
                                    if (orderDm.Status_Id == OrderStatus.OUT_FOR_DELIVERY)
                                    {
                                        byte[] bytes = Convert.FromBase64String(driverOrderRequest.Delivery_Image);
                                        var imagePathDir = "assets/images/delivery/";
                                        var fileName = StaticMethods.GetKuwaitTime().ToString("yyyyMMddHHmmssfff") +
                                                       ".png";
                                        var path = Path.Combine(_webHostEnvironment.WebRootPath, imagePathDir);
                                        var filePath = Path.Combine(path, fileName);
                                        System.IO.File.WriteAllBytes(filePath, bytes);

                                        orderDm.Delivery_Image = imagePathDir + fileName;
                                        orderDm.Status_Id = OrderStatus.ORDER_DELIVERED;
                                        orderBc.SaveOrder(orderDm);

                                        TXN_Order_Logs log = new TXN_Order_Logs();
                                        log.Order_Id = orderDm.Order_Id;
                                        log.Status_Id = orderDm.Status_Id;
                                        log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        log.Driver_Id = appUserDm.App_User_Id;
                                        log.Comments = "Order Delivered By Driver";
                                        orderBc.CreateOrderLog(log);

                                        CommonBC commonBc = new CommonBC(_context, _logPath);
                                        var pointsPerKd = commonBc.GetSettingValue<int>(SettingNames.Points_Per_KD);

                                        LP_POINTS_TRANSACTION txnDm = new LP_POINTS_TRANSACTION();
                                        txnDm.Type_Id = TXN_Point_Types.Add_Points_After_Payment;
                                        txnDm.TXN_Date = StaticMethods.GetKuwaitTime().Date;
                                        txnDm.App_User_Id = orderDm.App_User_Id;
                                        txnDm.Order_Id = orderDm.Order_Id;
                                        txnDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        txnDm.Points = orderDm.Net_Amount * pointsPerKd;

                                        string connectionString =
                                            _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                                        using (MySqlConnection con = new MySqlConnection(connectionString))
                                        {
                                            con.Open();
                                            var time = con.ConnectionTimeout;
                                            using (MySqlCommand cmd = new MySqlCommand("UpdateRedeemPoints", con))
                                            {
                                                using (var da = new MySqlDataAdapter(cmd))
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.Parameters.AddWithValue("@App_User_Id", orderDm.App_User_Id);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var orderBc = new OrderBC(_context);

                        var rowId = new Guid(driverOrderRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var orderDm = orderBc.GetOrder(driverOrderRequest.Order_Id);
                            if (orderDm != null)
                            {
                                if (orderDm.Driver_Id == appUserDm.App_User_Id)
                                {
                                    if (orderDm.Status_Id == OrderStatus.OUT_FOR_DELIVERY)
                                    {
                                        orderDm.Cancelled_Reason = driverOrderRequest.Comments;
                                        orderDm.Status_Id = OrderStatus.NOT_DELIVERED;
                                        orderBc.SaveOrder(orderDm);

                                        TXN_Order_Logs log = new TXN_Order_Logs();
                                        log.Order_Id = orderDm.Order_Id;
                                        log.Status_Id = orderDm.Status_Id;
                                        log.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        log.Driver_Id = appUserDm.App_User_Id;
                                        log.Comments = "Order Not Delivered (" + driverOrderRequest.Comments + ")";
                                        orderBc.CreateOrderLog(log);

                                        CommonBC commonBc = new CommonBC(_context, _logPath);
                                        var pointsPerKd = commonBc.GetSettingValue<int>(SettingNames.Points_Per_KD);

                                        LP_POINTS_TRANSACTION txnDm = new LP_POINTS_TRANSACTION();
                                        txnDm.Type_Id = TXN_Point_Types.Add_Points_After_Payment;
                                        txnDm.TXN_Date = StaticMethods.GetKuwaitTime().Date;
                                        txnDm.App_User_Id = orderDm.App_User_Id;
                                        txnDm.Order_Id = orderDm.Order_Id;
                                        txnDm.Created_Datetime = StaticMethods.GetKuwaitTime();
                                        txnDm.Points = orderDm.Net_Amount * pointsPerKd;

                                        string connectionString =
                                            _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                                        using (MySqlConnection con = new MySqlConnection(connectionString))
                                        {
                                            con.Open();
                                            var time = con.ConnectionTimeout;
                                            using (MySqlCommand cmd = new MySqlCommand("UpdateRedeemPoints", con))
                                            {
                                                using (var da = new MySqlDataAdapter(cmd))
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.Parameters.AddWithValue("@App_User_Id", orderDm.App_User_Id);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("DriverDeliveredOrders/{appUserId}")]
        [HttpGet]
        public DriverOrderResponse GetDriverDeliveredOrders(string appUserId)
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

                        if (string.IsNullOrEmpty(appUserId))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBc = new AppUserBC(_context);


                        var rowId = new Guid(appUserId);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var areaBc = new OrderBC(_context);
                            var currentevents = areaBc.GetDeliveredDriverOrders(lang, appUserDm.App_User_Id);

                            foreach (var currentevent in currentevents)
                            {
                                var eventsDto = new DriverOrderDTO();
                                eventsDto.Order_Id = currentevent.Order_Id;
                                eventsDto.Order_No = currentevent.Order_Serial;
                                eventsDto.Pickup_Address = currentevent.Branch_Address;
                                eventsDto.Delivery_Address = currentevent.Full_Address;
                                eventsDto.Order_Date = currentevent.Order_Datetime.ToString("dd-MM-yyyy hh:mm tt");
                                eventsDto.Order_Amount = currentevent.Net_Amount;
                                eventsDto.Order_Status = currentevent.Status_Name;
                                eventsDto.Payment_Type = currentevent.Payment_Type_Name;
                                response.Orders.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);


                        var rowId = new Guid(driverOrdersRequest.App_User_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var areaBc = new OrderBC(_context);
                            var fromDate = DateTime.ParseExact(driverOrdersRequest.From_Date, "dd-MMM-yyyy",
                                CultureInfo.InvariantCulture);
                            var toDate = DateTime.ParseExact(driverOrdersRequest.To_Date + " 23:59:59",
                                "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                            var currentevents =
                                areaBc.GetDeliveredDriverOrders(lang, appUserDm.App_User_Id, fromDate, toDate);

                            response.Total_Orders = currentevents.Count;
                            response.Cash_Collected = currentevents.Where(x => x.Payment_Type_Id == PaymentTypes.Cash)
                                .Sum(x => x.Net_Amount);
                            foreach (var currentevent in currentevents)
                            {
                                var eventsDto = new DriverOrderDTO();
                                eventsDto.Order_Id = currentevent.Order_Id;
                                eventsDto.Order_No = currentevent.Order_Serial;
                                eventsDto.Pickup_Address = currentevent.Branch_Address;
                                eventsDto.Delivery_Address = currentevent.Full_Address;
                                eventsDto.Order_Date = currentevent.Order_Datetime.ToString("dd-MM-yyyy hh:mm tt");
                                eventsDto.Order_Amount = currentevent.Net_Amount;
                                eventsDto.Order_Status = currentevent.Status_Name;
                                eventsDto.Payment_Type = currentevent.Payment_Type_Name;
                                response.Orders.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("DriverDeliveryOrder/{appUserId}")]
        [HttpGet]
        public OrderDetailResponse GetDriverDeliveryOrderDetail(string appUserId)
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

                        if (string.IsNullOrEmpty(appUserId))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBc = new AppUserBC(_context);


                        var rowId = new Guid(appUserId);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var orderBc = new OrderBC(_context);
                            var existingDeliveringOrder = orderBc.GetExistingDriverOrder(appUserDm.App_User_Id);


                            if (existingDeliveringOrder != null)
                            {
                                var orderDm = orderBc.GetOrder(existingDeliveringOrder.Order_Id);
                                response.Order_Id = orderDm.Order_Id;
                                response.Order_No = orderDm.Order_Serial;
                                response.Order_Date = orderDm.Order_Datetime.ToString("dd-MMM-yyyy hh:mm tt");
                                response.Cust_Name = orderDm.Cust_Name;
                                response.Mobile = orderDm.Mobile ?? "";
                                response.Email = orderDm.Email;

                                var pickAddressDm = new AddressCoordinatesDTO();
                                pickAddressDm.Address = orderDm.Branch_Address;
                                pickAddressDm.Latitude = orderDm.Branch_Latitude;
                                pickAddressDm.Longitude = orderDm.Branch_Longitude;
                                response.Pickup_Address = pickAddressDm;

                                var deliveryAddressDm = new AddressCoordinatesDTO();
                                deliveryAddressDm.Address = orderDm.Full_Address;
                                deliveryAddressDm.Latitude = orderDm.User_Address_Latitude;
                                deliveryAddressDm.Longitude = orderDm.User_Address_Longitude;
                                response.Delivery_Address = deliveryAddressDm;

                                response.Payment_Type = orderDm.Payment_Type_Name;
                                response.Order_Status_Id = orderDm.Status_Id;
                                response.Order_Status = orderDm.Status_Name;
                                response.Delivery_Option = orderDm.Delivery_Type_Name;
                                response.Sub_Total = orderDm.Gross_Amount;
                                response.Delivery_Charges = orderDm.Delivery_Charges;
                                response.Total = orderDm.Net_Amount;
                                response.Driver_Name = orderDm.Driver_Name;
                                response.Remarks = orderDm.Comments ?? "";
                                response.Restaurant_Name = orderDm.Restaurant_Name;
                                response.Restaurant_Mobile = orderDm.Mobile ?? "";
                                response.Current_Driver_Latitude = orderDm.Driver_Latitude;
                                response.Current_Driver_Longitude = orderDm.Driver_Longitude;

                                foreach (var detail in orderDm.TXN_Order_Details)
                                {
                                    var detailDm = new DriverOrderDetailDTO();
                                    detailDm.Prod_Name = detail.Full_Product_Name;
                                    detailDm.Qty = detail.Qty;
                                    detailDm.Rate = detail.Rate;
                                    detailDm.Gross_Amount = detail.Amount;
                                    detailDm.AddOn_Amount = detail.Gross_Amount - detail.Amount;
                                    detailDm.Net_Amount = detail.Gross_Amount;
                                    detailDm.Remarks = detail.Comments ?? "";
                                    response.OrderDetails.Add(detailDm);
                                }
                                //var addons = brandBC.GetProductAddOns(brandDM.Product_Id, lang);
                                // response.AddOns.AddRange(addons);


                                response.Message = ServiceResponse.Success;
                                response.Status = 0;
                            }
                            else
                            {
                                response.Message = lang == "A"
                                    ? "لم يتم العثور على طلب للتسليم"
                                    : "No Order found for delivery";
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        AppUserBC appUserBc = new AppUserBC(_context);
                        var orderBc = new OrderBC(_context);

                        var rowId = new Guid(trackingRequest.Driver_Id);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            TXN_Order_Tracking_Details trackingDm = new TXN_Order_Tracking_Details();
                            trackingDm.Order_Id = trackingRequest.Order_Id;
                            trackingDm.Status_Id = trackingRequest.Status_Id;
                            trackingDm.Track_Datetime = StaticMethods.GetKuwaitTime();
                            trackingDm.Driver_Id = appUserDm.App_User_Id;
                            trackingDm.Latitude = trackingRequest.Latitude;
                            trackingDm.Longitude = trackingRequest.Longitude;
                            orderBc.CreateOrderTrackingDetail(trackingDm);


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
                globalCls.WriteToFile(_logPath, ex.ToString());
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

                        var branchBc = new BranchBC(_context);
                        var currentevents = branchBc.GetAllBranches();
                        foreach (var currentevent in currentevents)
                        {
                            var eventsDto = new MapDTO();
                            eventsDto.Branch_Name = lang == "A"
                                ? currentevent.Branch_Name_A ?? currentevent.Branch_Name_E
                                : currentevent.Branch_Name_E;
                            eventsDto.Branch_Address = lang == "A"
                                ? currentevent.Address_A ?? currentevent.Address_E ?? ""
                                : currentevent.Address_E ?? "";
                            eventsDto.Restaurant_Name = currentevent.Restaurant_Name;
                            eventsDto.Latitude = currentevent.Latitude ?? 0;
                            eventsDto.Longitude = currentevent.Longitude ?? 0;
                            response.Branches.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("CustomerOrders/{appUserId}")]
        [HttpGet]
        public DriverOrderResponse GetCustomerOrders(string appUserId)
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

                        if (string.IsNullOrEmpty(appUserId))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBc = new AppUserBC(_context);


                        var rowId = new Guid(appUserId);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var areaBc = new OrderBC(_context);
                            var currentevents = areaBc.GetCustomerOrders(lang, appUserDm.App_User_Id);

                            foreach (var currentevent in currentevents)
                            {
                                var eventsDto = new DriverOrderDTO();
                                eventsDto.Order_Id = currentevent.Order_Id;
                                eventsDto.Order_No = currentevent.Order_Serial;
                                eventsDto.Pickup_Address = currentevent.Branch_Address;
                                eventsDto.Delivery_Address = currentevent.Full_Address;
                                eventsDto.Order_Date = currentevent.Order_Datetime.ToString("dd-MM-yyyy hh:mm tt");
                                eventsDto.Order_Amount = currentevent.Net_Amount;
                                eventsDto.Order_Status = currentevent.Status_Name;
                                response.Orders.Add(eventsDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                        var orderBc = new OrderBC(_context);

                        var orderDetailDm = orderBc.GetOrderDetail(ratingRequest.Order_Detail_Id);
                        if (orderDetailDm != null)
                        {
                            if (orderDetailDm.Rating == null)
                            {
                                var orderDm = orderBc.GetOrderByOrderDetail(ratingRequest.Order_Detail_Id);
                                if (orderDm != null)
                                {
                                    if (orderDm.Status_Id == OrderStatus.ORDER_DELIVERED)
                                    {
                                        orderDetailDm.Rating = ratingRequest.Rating;
                                        orderBc.SaveOrderDetail(orderDetailDm);

                                        string connectionString =
                                            _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                                        using (MySqlConnection con = new MySqlConnection(connectionString))
                                        {
                                            con.Open();
                                            var time = con.ConnectionTimeout;
                                            using (MySqlCommand cmd = new MySqlCommand("SetProductRating", con))
                                            {
                                                using (var da = new MySqlDataAdapter(cmd))
                                                {
                                                    cmd.CommandType = CommandType.StoredProcedure;
                                                    cmd.Parameters.AddWithValue("@Prod_Id", orderDetailDm.Product_Id);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }

        [Route("Notification/{appUserId}")]
        [HttpGet]
        public NotificationResponse GetNotifications(string appUserId)
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

                        if (string.IsNullOrEmpty(appUserId))
                        {
                            response.Status = 101;
                            response.Message = "App User Id cannot be empty";
                            return response;
                        }

                        AppUserBC appUserBc = new AppUserBC(_context);
                        NotificationBC notificationBc = new NotificationBC(_context, _logPath);


                        var rowId = new Guid(appUserId);
                        var appUserDm = appUserBc.GetAppUserByRowId(rowId);
                        if (appUserDm != null)
                        {
                            var notifications = notificationBc.GetNotifications(appUserDm.App_User_Id, lang);
                            foreach (var notification in notifications)
                            {
                                var notificationDto = new NotificationDTO();
                                notificationDto.Notification_Id = notification.Notification_Id;
                                notificationDto.Title = notification.Title;
                                notificationDto.Desc = notification.Desc;
                                notificationDto.Time = "";
                                TimeSpan span = StaticMethods.GetKuwaitTime().Subtract(notification.Created_Datetime);
                                if (span.TotalSeconds < 60)
                                {
                                    notificationDto.Time = "Just Now";
                                }
                                else if (span.TotalMinutes < 60)
                                {
                                    notificationDto.Time = span.Minutes + "m ago";
                                }
                                else if (span.TotalHours < 24)
                                {
                                    notificationDto.Time = span.Hours + "h ago";
                                }
                                else
                                {
                                    notificationDto.Time = span.Days + "d ago";
                                }

                                response.Notifications.Add(notificationDto);
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
                globalCls.WriteToFile(_logPath, ex.ToString());
            }

            return response;
        }


        #region Web Application API

        [Route("RestaurantAddOns/{restaurantId}")]
        [HttpGet]
        public List<SM_Restaurant_AddOns> GetRestaurantAddOns(long restaurantId)
        {
            var response = new List<SM_Restaurant_AddOns>();
            try
            {
                RestaurantBC restaurantBc = new RestaurantBC(_context);
                response = restaurantBc.GetAllRestaurantAddOns(restaurantId);
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(_logPath, ex.ToString(), true);
            }

            return response;
        }

        [Route("DashboardOrders/{dashboardOrderType}")]
        [HttpGet]
        public DriverOrderResponse GetDashboardOrders(int dashboardOrderType)
        {
            var response = new DriverOrderResponse();

            try
            {
                var areaBc = new OrderBC(_context);
                response.Orders = areaBc.GetDashboardOrders(dashboardOrderType);
                response.Status = 0;
                response.Message = ServiceResponse.Success;
            }

            catch (Exception ex)
            {
                response.Status = 1;
                response.Message = ServiceResponse.ServerError;
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                var eisDal = new DeviceBC(_context, _logPath);
                _securityDto = eisDal.GetDeviceByClientKey(Base64Decode(securityKey));
                // globalCls.WriteToFile(HttpContext.Current.Server.MapPath(Convert.ToString(ConfigurationManager.AppSettings["ErrorFilePath"])), "Client Key : " + securityDTO.Client_Key, true);

                if (_securityDto != null && !string.IsNullOrEmpty(_securityDto.Client_Key))
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
                globalCls.WriteToFile(_logPath, ex.ToString());
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
                var orderBc = new OrderBC(_context);
                RestaurantBC restaurantBc = new RestaurantBC(_context);
                decimal grossAmount = Decimal.Zero;

                var order = orderBc.GetOrder(Convert.ToInt32(orderId));
                var orderDetails = orderBc.GetOrderDetails(Convert.ToInt32(orderId));


                if (order != null)
                {
                    string bodyMessage = "";

                    var siteConfiguration = orderBc.GetSiteConfiguration(Email_Templates.COD_EMAIL_MESSAGE);

                    bodyMessage = siteConfiguration.Config_Value;
                    bodyMessage = bodyMessage.Replace("[CUST_NAME]", order.Cust_Name);
                    bodyMessage = bodyMessage.Replace("[ORDER_NO]", order.Order_Serial);
                    bodyMessage = bodyMessage.Replace("[ORDER_DATE]",
                        Convert.ToDateTime(order.Order_Datetime).ToString("dd/MM/yy hh:mm tt"));
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
                        string col1 =
                            " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" +
                            orderdetail.Full_Product_Name + "<o:p></o:p></span></p></td>";
                        string col2 =
                            " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" +
                            Convert.ToDecimal(orderdetail.Rate).ToString("N3") + "<o:p></o:p></span></p></td>";

                        string col5 =
                            " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" +
                            orderdetail.Qty.ToString() + "<o:p></o:p></span></p></td>";
                        string col6 =
                            " <td valign=top style='padding:5.0pt 0in 0in 0in'><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.5pt;color:#403F45'>" +
                            Convert.ToDecimal(orderdetail.Gross_Amount).ToString("N3") + "<o:p></o:p></span></p></td>";
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

                    var subject = siteConfiguration.Subject;
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


                    var restaurantDm = restaurantBc.GetRestaurant(order.Restaurant_Id);
                    if (restaurantDm != null && !string.IsNullOrEmpty(restaurantDm.Email))
                    {
                        siteConfiguration.BCC_Email =
                            siteConfiguration.BCC_Email.Replace("[RESTAURANT_EMAIL]", restaurantDm.Email);
                    }

                    bSuccess = GetSendHtmlMail(order.Email, subject, bodyMessage, siteConfiguration.CC_Email ?? "",
                        siteConfiguration.BCC_Email ?? "", siteConfiguration.From_Email ?? "",
                        siteConfiguration.Password ?? "");
                    var emailMsg = "";
                    if (bSuccess)
                    {
                        emailMsg = "Email sent successfully for Order Id:" + orderId;
                    }
                    else
                    {
                        emailMsg = "Email not sent successfully for Order Id:" + orderId;
                    }

                    globalCls.WriteToFile(_logPath, emailMsg);
                }
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(_logPath, ex.ToString());
            }


            return bSuccess;
        }

        [NonAction]
        public bool GetSendHtmlMail(string to, string subject, string body, string cc, string bcc, string fromEmail,
            string password, string receiverName = "")
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
                globalCls.WriteToFile(_logPath, ex.ToString());
                return false;
            }
        }

        #endregion
    }
}