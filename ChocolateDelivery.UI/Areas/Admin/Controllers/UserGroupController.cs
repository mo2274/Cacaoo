using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserGroupController : Controller
    {
        private AppDbContext context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        UserService _userService;


        public UserGroupController(AppDbContext cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            _userService = new UserService(context, logPath);
        }
        public IActionResult Create()
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            var userGroup = new SM_USER_GROUPS();
            var userGroupRights = new List<SM_USER_GROUP_RIGHTS>();
            var menus = _userService.GetAllMenus();

            foreach (var smMenu in menus)
            {
                userGroupRights.Add(new SM_USER_GROUP_RIGHTS
                {
                    Menu_Id = smMenu.MenuId,
                    MenuName_En = smMenu.MenuName_En,
                    AllowView = false,
                    AllowAdd = false,
                    AllowEdit = false,
                    AllowDelete = false
                });
            }
            userGroup.SM_USER_GROUP_RIGHTS = userGroupRights;

            return View(userGroup);
        }

        // HTTP POST VERSION  
        [HttpPost]
        public IActionResult Create(SM_USER_GROUPS group)
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
                        group.Admin = false;
                        group.User_Date_Time = StaticMethods.GetKuwaitTime();
                        group = _userService.CreateUserGroup(group);
                        if (group.Group_Cd != 0)
                        {
                            foreach (var right in group.SM_USER_GROUP_RIGHTS)
                            {
                                right.Group_Cd = group.Group_Cd;
                                right.User_Date_Time = StaticMethods.GetKuwaitTime();
                                _userService.CreateUserGroupRight(right);
                            }
                        }
                        return Redirect("/List/" + list_id);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Login");
                    }



                }
                else
                    return View("Create", group);

            }
            catch (Exception ex)
            {
                /* lblError.Visible = true;
                 lblError.Text = "Invalid username or password";*/
                ModelState.AddModelError("name", "Due to some technical error, data not saved");
                Helpers.WriteToFile(logPath, ex.ToString(), true);

            }
            return View("Create", group);
            /*if (ModelState.IsValid)
            {

                var userDM = _userService.isUserExist(user.User_Id.Trim());
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
                var areaexist = _userService.GetUserGroup(decryptedId);
                if (areaexist != null && areaexist.Group_Cd != 0)
                {
                    var userRights = _userService.GetGroupRights(decryptedId);
                    var userGroupRights = new List<SM_USER_GROUP_RIGHTS>();
                    foreach (var smMenu in userRights)
                    {
                        userGroupRights.Add(new SM_USER_GROUP_RIGHTS
                        {
                            Right_Id = smMenu.Right_Id,
                            Menu_Id = smMenu.Menu_Id,
                            MenuName_En = smMenu.MenuName_En,
                            AllowView = smMenu.AllowView,
                            AllowAdd = smMenu.AllowAdd,
                            AllowEdit = smMenu.AllowEdit,
                            AllowDelete = smMenu.AllowDelete,
                        });
                    }
                    areaexist.SM_USER_GROUP_RIGHTS = userGroupRights;
                    return View("Create", areaexist);
                }
                else
                {
                    ModelState.AddModelError("name", "Group not exist");
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
        public IActionResult Update(SM_USER_GROUPS group, string Id)
        {
            try
            {
                var list_id = Request.Query["List_Id"];
                ViewBag.List_Id = list_id;
                if (ModelState.IsValid)
                {
                    var decryptedId = Convert.ToInt16(StaticMethods.GetDecrptedString(Id));
                    var areaDM = _userService.GetUserGroup(decryptedId);
                    if (areaDM != null && areaDM.Group_Cd != 0)
                    {
                        group.User_Date_Time = StaticMethods.GetKuwaitTime();
                        group.Group_Cd = decryptedId;
                        _userService.CreateUserGroup(group);
                        if (group.Group_Cd != 0)
                        {
                            foreach (var right in group.SM_USER_GROUP_RIGHTS)
                            {
                                right.Group_Cd = group.Group_Cd;
                                right.User_Date_Time = StaticMethods.GetKuwaitTime();
                                _userService.CreateUserGroupRight(right);
                            }
                        }
                        return Redirect("/List/" + list_id);

                    }
                    else
                    {
                        ModelState.AddModelError("", "Group not exist");
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
            return View("Create", group);
        }
    }
}
