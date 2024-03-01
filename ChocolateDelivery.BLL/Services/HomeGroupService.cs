using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL;

public class HomeGroupService
{
    private readonly AppDbContext _context;

    public HomeGroupService(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }
    public List<SM_Home_Groups> GetAllGroups()
    {
        var userMenu = new List<SM_Home_Groups>();
        try
        {
            userMenu = (from o in _context.sm_home_groups
                select o).ToList();


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }

    public List<SM_Home_Groups> GetGroups()
    {
        var userMenu = new List<SM_Home_Groups>();
        try
        {
            userMenu = (from o in _context.sm_home_groups
                where o.Show == true
                orderby o.Sequence
                select o).ToList();

        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }

    public SM_Home_Groups? GetGroup(int Group_id)
    {
        var userMenu = new SM_Home_Groups();
        try
        {
            userMenu = (from o in _context.sm_home_groups
                where o.Group_Id == Group_id
                select o).FirstOrDefault();
            if (userMenu != null) {
                userMenu.SM_Home_Group_Details = GetGroupDetails(userMenu.Group_Id);
            }

        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }

    public SM_Home_Groups? GetGroup(string Group_name)
    {
        var userMenu = new SM_Home_Groups();
        try
        {
            userMenu = (from o in _context.sm_home_groups
                where o.Group_Name_E == Group_name
                select o).FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }

    public SM_Home_Groups CreateGroup(SM_Home_Groups CustomerDM)
    {
        try
        {
            if (CustomerDM.Group_Id != 0)
            {
                var Customer = (from o in _context.sm_home_groups
                    where o.Group_Id == CustomerDM.Group_Id
                    select o).FirstOrDefault();

                if (Customer != null)
                {
                    Customer.Group_Name_A = CustomerDM.Group_Name_A;
                    Customer.Group_Name_E = CustomerDM.Group_Name_E;
                    Customer.Display_Type = CustomerDM.Display_Type;
                    Customer.Updated_By = CustomerDM.Updated_By;
                    Customer.Updated_Datetime = CustomerDM.Updated_Datetime;
                    Customer.Show = CustomerDM.Show;
                    Customer.Sequence = CustomerDM.Sequence;
                    _context.SaveChanges();
                }
            }
            else
            {
                _context.sm_home_groups.Add(CustomerDM);
                _context.SaveChanges();
            }
        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }


        return CustomerDM;
    }

    public long GetMaxGroupId()
    {
        long maxpromoterid = 1;
        var userMenu = new SM_Home_Groups();
        try
        {
            userMenu = (from o in _context.sm_home_groups
                orderby o.Group_Id descending
                select o).FirstOrDefault();
            if (userMenu != null)
                maxpromoterid = userMenu.Group_Id + 1;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return maxpromoterid;
    }

    public SM_Home_Group_Details CreateGroupDetail(SM_Home_Group_Details CustomerDM)
    {
        try
        {
            //if (CustomerDM.Size_Id != 0)
            {
                var Customer = (from o in _context.sm_home_group_details
                    where o.Group_Detail_Id == CustomerDM.Group_Detail_Id
                    select o).FirstOrDefault();

                if (Customer != null)
                {
                    Customer.Show = CustomerDM.Show;
                    Customer.Sequence = CustomerDM.Sequence;
                    _context.SaveChanges();

                }
                else
                {
                    _context.sm_home_group_details.Add(CustomerDM);
                    _context.SaveChanges();
                }
            }

        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }


        return CustomerDM;
    }

    public List<SM_Home_Group_Details> GetAllGroupDetails(long grp_id)
    {
        var userMenu = new List<SM_Home_Group_Details>();
        try
        {
            var groupDetails = (from o in _context.sm_home_group_details
                where o.Group_Id == grp_id
                orderby o.Sequence
                select o).ToList();


            foreach (var groupDetail in groupDetails)
            {
                if (groupDetail.Group_Type_Id == GroupType.CATEGORIES)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_categories
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Category_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Category_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(groupDetail.Group_Type_Id);
                        grpdetailDM.Image_Url = !string.IsNullOrEmpty(subquery.o.Image_Url) ? subquery.o.Image_Url : subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.SUB_CATEGORIES)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_sub_categories
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Sub_Category_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Sub_Category_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url = !string.IsNullOrEmpty(subquery.o.Image_Url) ? subquery.o.Image_Url : subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.CACAOO_CHEF || groupDetail.Group_Type_Id == GroupType.CACAOO_BOUTIQUE)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_chefs
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Chef_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Chef_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url = !string.IsNullOrEmpty(subquery.o.Image_Url) ? subquery.o.Image_Url : subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.PRODUCTS)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_products
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Product_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Product_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url = !string.IsNullOrEmpty(subquery.o.Image_Url) ? subquery.o.Image_Url : subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.RESTAURANTS)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_restaurants
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Restaurant_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Restaurant_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url = subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.OCCASIONS)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_occasions
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Occasion_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Occasion_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url = !string.IsNullOrEmpty(subquery.o.Image_Url) ? subquery.o.Image_Url : subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }


            }




        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }

    public List<SM_Home_Group_Details> GetGroupDetails(long grp_id)
    {
        var userMenu = new List<SM_Home_Group_Details>();
        try
        {
            var groupDetails = (from o in _context.sm_home_group_details
                from g in _context.sm_home_groups
                where o.Group_Id == grp_id && o.Show == true
                                           && g.Show == true && o.Group_Id == g.Group_Id
                orderby o.Sequence
                select o).ToList();


            foreach (var groupDetail in groupDetails)
            {
                if (groupDetail.Group_Type_Id == GroupType.CATEGORIES)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_categories
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Category_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Category_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(groupDetail.Group_Type_Id);
                        grpdetailDM.Image_Url =  subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.SUB_CATEGORIES)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_sub_categories
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Sub_Category_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Sub_Category_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url =  subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.CACAOO_CHEF || groupDetail.Group_Type_Id == GroupType.CACAOO_BOUTIQUE)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_chefs
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Chef_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Chef_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url = subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.PRODUCTS)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_products
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Product_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Product_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url =subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.RESTAURANTS)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_restaurants
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Restaurant_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Restaurant_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url =  subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }
                else if (groupDetail.Group_Type_Id == GroupType.OCCASIONS)
                {
                    var subquery = (from o in _context.sm_home_group_details
                        from i in _context.sm_occasions
                        where o.Group_Detail_Id == groupDetail.Group_Detail_Id && o.Id == i.Occasion_Id
                        orderby o.Sequence
                        select new { o, Item_Name = i.Occasion_Name_E, Image_Url = i.Image_URL }).FirstOrDefault();

                    if (subquery != null)
                    {
                        var grpdetailDM = new SM_Home_Group_Details();
                        grpdetailDM = subquery.o;
                        grpdetailDM.Item_Name = subquery.Item_Name;
                        grpdetailDM.Group_Type_Name = GetGroupType(subquery.o.Group_Type_Id);
                        grpdetailDM.Image_Url = subquery.Image_Url;
                        userMenu.Add(grpdetailDM);
                    }
                }



            }


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }

    public SM_Home_Group_Details? GetGroupDetail(long grp_detail_id)
    {
        var userMenu = new SM_Home_Group_Details();
        try
        {
            userMenu = (from o in _context.sm_home_group_details
                where o.Group_Detail_Id == grp_detail_id
                select o).FirstOrDefault();
              

        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }

    public bool RemoveGroupdetail(long grp_detail_id)
    {
        var isremoved = true;
        try
        {

            var Customer = (from o in _context.sm_home_group_details
                where o.Group_Detail_Id == grp_detail_id
                select o).FirstOrDefault();

            if (Customer != null)
            {

                _context.sm_home_group_details.Remove(Customer);
                _context.SaveChanges();
            }
            else
            {
                isremoved = false;
            }
        }


        catch (Exception ex)
        {
            isremoved = false;
            throw new Exception(ex.ToString());
        }


        return isremoved;
    }

    public bool RemoveGroup(long grp_id)
    {
        var isremoved = false;
        try
        {

            var groupList = (from o in _context.sm_home_group_details
                where o.Group_Id == grp_id
                select o).ToList();

            foreach (var grp in groupList)
            {
                _context.sm_home_group_details.Remove(grp);
            }

            var Customer = (from o in _context.sm_home_groups
                where o.Group_Id == grp_id
                select o).FirstOrDefault();
            if (Customer != null)
            {

                _context.sm_home_groups.Remove(Customer);
                _context.SaveChanges();
                isremoved = true;
            }
            else
            {
                isremoved = false;
            }
        }


        catch (Exception ex)
        {
            isremoved = false;
            throw new Exception(ex.ToString());
        }


        return isremoved;
    }

    public SM_Home_Groups? UpdateSequence(int grp_id, int sequence)
    {
        var Customer = new SM_Home_Groups();
        try
        {
            Customer = (from o in _context.sm_home_groups
                where o.Group_Id == grp_id
                select o).FirstOrDefault();

            if (Customer != null)
            {
                Customer.Sequence = sequence;
                _context.SaveChanges();
            }


        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }


        return Customer;
    }

    public string GetGroupType(short group_type_id)
    {
        var group_type = "";
        try
        {
            if (group_type_id == GroupType.CATEGORIES)
            {
                group_type = "Categories";
            }
            else if (group_type_id == GroupType.SUB_CATEGORIES)
            {
                group_type = "Sub Categories";
            }
            else if (group_type_id == GroupType.CACAOO_CHEF)
            {
                group_type = "Cacaoo Chef";
            }
            else if (group_type_id == GroupType.CACAOO_BOUTIQUE)
            {
                group_type = "Cacaoo Boutique";
            }
            else if (group_type_id == GroupType.PRODUCTS)
            {
                group_type = "Products";
            }
            else if (group_type_id == GroupType.RESTAURANTS)
            {
                group_type = "Restaurant";
            }
            else if (group_type_id == GroupType.OCCASIONS)
            {
                group_type = "Occasions";
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return group_type;
    }

    public List<Select2DTO> GetGroupItems(short group_type_id)
    {
        var userMenu = new List<Select2DTO>();
        try
        {
            if (group_type_id == GroupType.CATEGORIES)
            {
                userMenu = (from o in _context.sm_categories
                    select new Select2DTO
                    {
                        id = o.Category_Id.ToString(),
                        text = o.Category_Name_E
                    }).ToList();
            } else if (group_type_id == GroupType.SUB_CATEGORIES)
            {
                userMenu = (from o in _context.sm_sub_categories
                    select new Select2DTO
                    {
                        id = o.Sub_Category_Id.ToString(),
                        text = o.Sub_Category_Name_E
                    }).ToList();
            }
            else if (group_type_id == GroupType.CACAOO_CHEF)
            {
                userMenu = (from o in _context.sm_chefs
                    where o.Type_Id == Chef_Types.CHEF
                    select new Select2DTO
                    {
                        id = o.Chef_Id.ToString(),
                        text = o.Chef_Name_E
                    }).ToList();
            }
            else if (group_type_id == GroupType.CACAOO_BOUTIQUE)
            {
                userMenu = (from o in _context.sm_chefs
                    where o.Type_Id == Chef_Types.BOUTIQUE
                    select new Select2DTO
                    {
                        id = o.Chef_Id.ToString(),
                        text = o.Chef_Name_E
                    }).ToList();
            }
            else if (group_type_id == GroupType.PRODUCTS)
            {
                userMenu = (from o in _context.sm_products                              
                    select new Select2DTO
                    {
                        id = o.Product_Id.ToString(),
                        text = o.Product_Name_E
                    }).ToList();
            }
            else if (group_type_id == GroupType.RESTAURANTS)
            {
                userMenu = (from o in _context.sm_restaurants
                    select new Select2DTO
                    {
                        id = o.Restaurant_Id.ToString(),
                        text = o.Restaurant_Name_E
                    }).ToList();
            }
            else if (group_type_id == GroupType.OCCASIONS)
            {
                userMenu = (from o in _context.sm_occasions
                    select new Select2DTO
                    {
                        id = o.Occasion_Id.ToString(),
                        text = o.Occasion_Name_E
                    }).ToList();
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }

    public void UpdateGroupDetails(int line_no, long grp_id)
    {
        try
        {
            var query = (from o in _context.sm_home_group_details
                where o.Line_No > line_no
                      && o.Group_Id == grp_id
                select o).ToList();
            foreach (var detail in query)
            {
                detail.Line_No = detail.Line_No - 1;
            }
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
}