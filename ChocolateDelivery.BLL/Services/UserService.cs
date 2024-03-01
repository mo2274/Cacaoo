using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL;

public class UserService
{
    private readonly AppDbContext context;
    private readonly string logPath;

    public UserService(AppDbContext appDbContext, string errorLogPath)
    {
        context = appDbContext;
        logPath = errorLogPath;
        // context.UpdateEFCon();

    }
    public SM_USERS? ValidateUser(string userName, string password)
    {
        var userId = new SM_USERS();
        try
        {
            userId = (from o in context.sm_users
                where o.User_Id == userName && o.Password == password
                select o).FirstOrDefault();


        }

        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userId;
    }

    public SM_USERS? isUserExist(string userName)
    {
        var username = new SM_USERS();
        try
        {

            username = (from o in context.sm_users
                where o.User_Id.ToUpper() == userName.ToUpper()
                select o).FirstOrDefault();

        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return username;
    }
    //public List<SP_GetUserList_Result> GetUserList()
    //{
    //    var userList = new List<SP_GetUserList_Result>();
    //    try
    //    {
    //        var query = (from o in context.sm_users
    //                     select o).ToList();

    //        foreach (var smUser in query)
    //        {
    //            userList.Add(new SP_GetUserList_Result
    //            {
    //                User_cd = smUser.User_Cd,
    //                User_Name = smUser.User_Name,
    //                IsOnline = 1
    //            });
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Helpers.WriteToFile(logPath ex.ToString(), true);
    //    }
    //    return userList;
    //}

    public List<SM_USERS> GetUsers()
    {
        var userList = new List<SM_USERS>();
        try
        {
            var query = (from o in context.sm_users
                from g in context.sm_user_groups
                where o.Group_Cd == g.Group_Cd
                select new
                {
                    o,
                    g
                }).ToList();

            foreach (var VARIABLE in query)
            {
                var user = VARIABLE.o;
                user.Group_Name_En = VARIABLE.g.Group_Name_En;
                user.Group_Name_Ar = VARIABLE.g.Group_Name_Ar;

                userList.Add(user);
            }

        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userList;
    }
    public List<SM_USERS> GetProductionUsers()
    {
        var userList = new List<SM_USERS>();
        try
        {
            var query = (from o in context.sm_users
                from g in context.sm_user_groups
                where o.Group_Cd == g.Group_Cd && o.Group_Cd == 3
                select new
                {
                    o,
                    g
                }).ToList();

            foreach (var VARIABLE in query)
            {
                var user = VARIABLE.o;
                user.Group_Name_En = VARIABLE.g.Group_Name_En;
                user.Group_Name_Ar = VARIABLE.g.Group_Name_Ar;

                userList.Add(user);
            }

        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userList;
    }
    public SM_USERS? GetUser(Int16 userCd)
    {
        var userList = new SM_USERS();
        try
        {
            userList = (from o in context.sm_users
                where o.User_Cd == userCd
                select o).FirstOrDefault();

        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userList;
    }

    public List<SM_MENU> GetUserMenu(int userId)
    {
        var userMenu = new List<SM_MENU>();
        try
        {
            userMenu = (from o in context.sm_menu
                from u in context.sm_users
                from ug in context.sm_user_groups
                from ugr in context.sm_user_group_rights
                where o.MenuId == ugr.Menu_Id && ugr.Group_Cd == ug.Group_Cd
                                              && ug.Group_Cd == u.Group_Cd && u.User_Cd == userId
                                              && ugr.AllowView && o.Show
                select o).ToList();
        }
        catch (Exception ex)
        {

            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userMenu;
    }

    public List<SM_MENU> GetUserMenuEditPerm(int userId)
    {
        var userMenu = new List<SM_MENU>();
        try
        {
            userMenu = (from o in context.sm_menu
                from u in context.sm_users
                from ug in context.sm_user_groups
                from ugr in context.sm_user_group_rights
                where o.MenuId == ugr.Menu_Id && ugr.Group_Cd == ug.Group_Cd
                                              && ug.Group_Cd == u.Group_Cd && u.User_Cd == userId
                                              && ugr.AllowEdit && o.Show
                select o).ToList();
        }
        catch (Exception ex)
        {

            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userMenu;
    }

    public List<SM_MENU> GetUserMenuAddPerm(int userId)
    {
        var userMenu = new List<SM_MENU>();
        try
        {
            userMenu = (from o in context.sm_menu
                from u in context.sm_users
                from ug in context.sm_user_groups
                from ugr in context.sm_user_group_rights
                where o.MenuId == ugr.Menu_Id && ugr.Group_Cd == ug.Group_Cd
                                              && ug.Group_Cd == u.Group_Cd && u.User_Cd == userId
                                              && ugr.AllowAdd && o.Show
                select o).ToList();
        }
        catch (Exception ex)
        {

            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userMenu;
    }

    public List<SM_MENU> GetUserMenuDeletePerm(int userId)
    {
        var userMenu = new List<SM_MENU>();
        try
        {
            userMenu = (from o in context.sm_menu
                from u in context.sm_users
                from ug in context.sm_user_groups
                from ugr in context.sm_user_group_rights
                where o.MenuId == ugr.Menu_Id && ugr.Group_Cd == ug.Group_Cd
                                              && ug.Group_Cd == u.Group_Cd && u.User_Cd == userId
                                              && ugr.AllowDelete && o.Show
                select o).ToList();
        }
        catch (Exception ex)
        {

            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userMenu;
    }

    public List<SM_MENU> GetAllMenus()
    {
        var userMenu = new List<SM_MENU>();
        try
        {
            userMenu = (from o in context.sm_menu
                where o.Show
                select o).ToList();
        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userMenu;
    }
    public SM_USERS CreateUser(SM_USERS userDM)
    {
        try
        {
            if (userDM.User_Cd != 0)
            {
                var category = (from o in context.sm_users
                    where o.User_Cd == userDM.User_Cd
                    select o).FirstOrDefault();

                if (category != null)
                {
                    category.User_Id = userDM.User_Id;
                    category.Group_Cd = userDM.Group_Cd;
                    category.User_Name = userDM.User_Name;
                    category.Password = userDM.Password;
                    category.Comments = userDM.Comments;
                    category.User_Cd = userDM.User_Cd;

                    context.SaveChanges();
                }
            }
            else
            {

                context.sm_users.Add(userDM);
                context.SaveChanges();
            }

        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }


        return userDM;
    }

    public List<SM_USER_GROUPS> GetUserGroupList()
    {
        var userList = new List<SM_USER_GROUPS>();
        try
        {
            userList = (from o in context.sm_user_groups
                select o).ToList();


        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userList;
    }

    public SM_USER_GROUPS GetUserGroup(int groupCd)
    {
        var userList = new SM_USER_GROUPS();
        try
        {
            userList = (from o in context.sm_user_groups
                where o.Group_Cd == groupCd
                select o).FirstOrDefault();

        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }
        return userList;
    }
    //public List<SM_USER_GROUP_RIGHTS> GetUserGroupRights(int groupId)
    //{
    //    var userMenu = new List<SM_USER_GROUP_RIGHTS>();
    //    try
    //    {
    //        var query = (from o in context.sm_user_group_rights
    //                     from m in context.sm_menu
    //                     where o.Group_Cd == groupId && o.Menu_Id == m.MenuId
    //                     select new
    //                     {
    //                         o,
    //                         m
    //                     }).ToList();

    //        foreach (var variable in query)
    //        {
    //            var smUserGroupRight = variable.o;
    //            smUserGroupRight.MenuName_En = variable.m.MenuName_En;
    //            userMenu.Add(smUserGroupRight);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Helpers.WriteToFile(logPath ex.ToString(), true);
    //    }
    //    return userMenu;
    //}

    public List<GroupsRightsDTO> GetGroupRights(int gruopId)
    {
        var grouprights = new List<GroupsRightsDTO>();
        try
        {

            grouprights = (from o in context.sm_menu
                where o.Show
                select new GroupsRightsDTO
                {
                    Menu_Id = o.MenuId,
                    MenuName_En = o.MenuName_En
                }).ToList();

            var groupMenuRights = (from o in context.sm_user_group_rights
                where o.Group_Cd == gruopId
                select o).ToList();

            foreach (var location in grouprights)
            {
                var locRight = (from o in groupMenuRights
                    from g in context.sm_user_groups
                    where o.Group_Cd == g.Group_Cd && o.Menu_Id == location.Menu_Id
                    select new
                    {
                        o,
                        g
                    }).FirstOrDefault();

                if (locRight != null)
                {
                    location.Right_Id = locRight.o.Right_Id;
                    location.Group_Cd = locRight.o.Group_Cd;
                    location.Group_Name_En = locRight.g.Group_Name_En;
                    location.AllowView = locRight.o.AllowView;
                    location.AllowEdit = locRight.o.AllowEdit;
                    location.AllowDelete = locRight.o.AllowDelete;
                    location.AllowAdd = locRight.o.AllowAdd;
                }
            }


        }
        catch (Exception ex)
        {
            throw ex;
        }
        return grouprights;
    }

    public SM_USER_GROUPS CreateUserGroup(SM_USER_GROUPS userDM)
    {
        try
        {
            if (userDM.Group_Cd != 0)
            {
                var category = (from o in context.sm_user_groups
                    where o.Group_Cd == userDM.Group_Cd
                    select o).FirstOrDefault();

                if (category != null)
                {
                    category.Group_Name_En = userDM.Group_Name_En;
                    category.Group_Name_Ar = userDM.Group_Name_Ar;
                    category.Show = userDM.Show;
                    category.User_Date_Time = userDM.User_Date_Time;

                    context.SaveChanges();
                }
            }
            else
            {

                context.sm_user_groups.Add(userDM);
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }


        return userDM;
    }

    public SM_USER_GROUP_RIGHTS CreateUserGroupRight(SM_USER_GROUP_RIGHTS userDM)
    {
        try
        {
            if (userDM.Right_Id != 0)
            {
                var category = (from o in context.sm_user_group_rights
                    where o.Right_Id == userDM.Right_Id
                    select o).FirstOrDefault();

                if (category != null)
                {
                    category.AllowView = userDM.AllowView;
                    category.AllowAdd = userDM.AllowAdd;
                    category.AllowEdit = userDM.AllowEdit;
                    category.AllowDelete = userDM.AllowDelete;
                    category.User_Date_Time = userDM.User_Date_Time;

                    context.SaveChanges();
                }
            }
            else
            {

                context.sm_user_group_rights.Add(userDM);
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Helpers.WriteToFile(logPath, ex.ToString(), true);
        }


        return userDM;
    }
    public bool DeleteUserGroupRights(long groupCd)
    {
        try
        {
            var query = (from o in context.sm_user_group_rights
                where o.Group_Cd == groupCd
                select o).ToList();

            foreach (var userGroupRight in query)
            {
                context.sm_user_group_rights.Remove(userGroupRight);
            }
            context.SaveChanges();
            return true;
        }
        catch (Exception)
        {
            return false;
        }

    }


}