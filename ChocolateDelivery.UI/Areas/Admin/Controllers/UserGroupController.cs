using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserGroupController : Controller
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        UserBC userBC;


        public UserGroupController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            userBC = new UserBC(context, logPath);
        }
        public IActionResult Create()
        {
            var list_id = Request.Query["List_Id"];
            ViewBag.List_Id = list_id;
            var userGroup = new SM_USER_GROUPS();
            var userGroupRights = new List<SM_USER_GROUP_RIGHTS>();
            var menus = userBC.GetAllMenus();

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
                        group = userBC.CreateUserGroup(group);
                        if (group.Group_Cd != 0)
                        {
                            foreach (var right in group.SM_USER_GROUP_RIGHTS)
                            {
                                right.Group_Cd = group.Group_Cd;
                                right.User_Date_Time = StaticMethods.GetKuwaitTime();
                                userBC.CreateUserGroupRight(right);
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
                globalCls.WriteToFile(logPath, ex.ToString(), true);

            }
            return View("Create", group);
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
                var areaexist = userBC.GetUserGroup(decryptedId);
                if (areaexist != null && areaexist.Group_Cd != 0)
                {
                    var userRights = userBC.GetGroupRights(decryptedId);
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
                globalCls.WriteToFile(logPath, ex.ToString(), true);

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
                    var areaDM = userBC.GetUserGroup(decryptedId);
                    if (areaDM != null && areaDM.Group_Cd != 0)
                    {
                        group.User_Date_Time = StaticMethods.GetKuwaitTime();
                        group.Group_Cd = decryptedId;
                        userBC.CreateUserGroup(group);
                        if (group.Group_Cd != 0)
                        {
                            foreach (var right in group.SM_USER_GROUP_RIGHTS)
                            {
                                right.Group_Cd = group.Group_Cd;
                                right.User_Date_Time = StaticMethods.GetKuwaitTime();
                                userBC.CreateUserGroupRight(right);
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
                globalCls.WriteToFile(logPath, ex.ToString(), true);

            }
            return View("Create", group);
        }
    }
}
