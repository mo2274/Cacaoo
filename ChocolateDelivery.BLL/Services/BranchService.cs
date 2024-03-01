using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL;

public class BranchService
{
    private readonly AppDbContext _context;

    public BranchService(AppDbContext appDbContext)
    {
        _context = appDbContext;

    }


    public SM_Restaurant_Branches CreateBranch(SM_Restaurant_Branches brandDM)
    {
        try
        {
            var query = (from o in _context.sm_restaurant_branches
                where o.Branch_Id == brandDM.Branch_Id
                select o).FirstOrDefault();

            if (query != null)
            {
                query.Branch_Name_E = brandDM.Branch_Name_E;
                query.Branch_Name_A = brandDM.Branch_Name_A;
                query.Address_E = brandDM.Address_E;
                query.Address_A = brandDM.Address_A;
                query.Show = brandDM.Show;
                query.Sequence = brandDM.Sequence;
                query.Updated_By = brandDM.Updated_By;
                query.Updated_Datetime = brandDM.Updated_Datetime;
                query.Latitude = brandDM.Latitude;
                query.Longitude = brandDM.Longitude;
                query.Username = brandDM.Username;
                query.Password = brandDM.Password;
                query.Opening_Time = brandDM.Opening_Time;
                query.Closing_Time = brandDM.Closing_Time;
            }
            else
            {
                _context.sm_restaurant_branches.Add(brandDM);
            }
            _context.SaveChanges();
        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return brandDM;
    }

    public SM_Restaurant_Branches? GetBranch(int brand_id)
    {
        var area = new SM_Restaurant_Branches();
        try
        {


            area = (from o in _context.sm_restaurant_branches
                where o.Branch_Id == brand_id
                select o).FirstOrDefault();
            if (area != null)
            {
                if (area.Opening_Time != null)
                {
                    var time = StaticMethods.GetKuwaitTime().Date.Add((TimeSpan)area.Opening_Time);
                    area.Opening_Time_String = time.ToString("hh:mm tt");
                }
                if (area.Closing_Time != null)
                {
                    var time = StaticMethods.GetKuwaitTime().Date.Add((TimeSpan)area.Closing_Time);
                    area.Closing_Time_String = time.ToString("hh:mm tt");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return area;
    }

    public List<SM_Restaurant_Branches> GetAllBranches(string lang = "E")
    {

        var customer = new List<SM_Restaurant_Branches>();
        try
        {
            var query = (from o in _context.sm_restaurant_branches
                from r in _context.sm_restaurants
                where o.Restaurant_Id == r.Restaurant_Id
                select new { o, r }).ToList();

            foreach (var detail in query)
            {
                var detailDM = new SM_Restaurant_Branches();
                detailDM = detail.o;
                detailDM.Restaurant_Name = lang == "A" ? detail.r.Restaurant_Name_A ?? detail.r.Restaurant_Name_E : detail.r.Restaurant_Name_E;
                customer.Add(detailDM);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return customer;
    }

    public List<SM_Restaurant_Branches> GetRestaurantBranches(long restaurant_id)
    {

        var customer = new List<SM_Restaurant_Branches>();
        try
        {
            customer = (from o in _context.sm_restaurant_branches
                where o.Restaurant_Id == restaurant_id
                select o).ToList();


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return customer;
    }

}