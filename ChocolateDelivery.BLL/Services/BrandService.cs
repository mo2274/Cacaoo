using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL;

public class BrandService
{
    private readonly AppDbContext _context;

    public BrandService(AppDbContext benayaatEntities)
    {
        _context = benayaatEntities;

    }


    public SM_Brands CreateBrand(SM_Brands brandDM)
    {
        try
        {
            var query = (from o in _context.sm_brands
                where o.Brand_Id == brandDM.Brand_Id
                select o).FirstOrDefault();

            if (query != null)
            {
                query.Brand_Name_E = brandDM.Brand_Name_E;
                query.Brand_Name_A = brandDM.Brand_Name_A;
                query.Brand_Desc_E = brandDM.Brand_Desc_E;
                query.Brand_Desc_A = brandDM.Brand_Desc_A;
                query.Show = brandDM.Show;
                query.Sequence = brandDM.Sequence;
                query.Updated_By = brandDM.Updated_By;
                query.Updated_Datetime = brandDM.Updated_Datetime;
                if (!string.IsNullOrEmpty(brandDM.Image_URL))
                {
                    query.Image_URL = brandDM.Image_URL;
                }
                query.Background_Color = brandDM.Background_Color;
            }
            else
            {
                _context.sm_brands.Add(brandDM);
            }
            _context.SaveChanges();
        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return brandDM;
    }

    public SM_Brands? GetBrand(int brand_id)
    {
        var area = new SM_Brands();
        try
        {
            var query = (from o in _context.sm_brands
                from r in _context.sm_restaurants
                where o.Brand_Id == brand_id && o.Restaurant_Id == r.Restaurant_Id
                select new { o, r }).FirstOrDefault();
            if (query != null) {
                area = query.o;
                area.Delivery_Charge = query.r.Delivery_Charge;
                area.Delivery_Time = query.r.Delivery_Time ?? "";
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return area;
    }

    public List<SM_Restaurants> GetBrands(ProductRequest itemRequest, string lang = "E")
    {

        var appPosts = new List<SM_Restaurants>();
        try
        {
            StaticMethods.GetKuwaitTime();
            var query = (from o in _context.sm_products
                from b in _context.sm_restaurants
                where o.Show && o.Publish && o.Restaurant_Id == b.Restaurant_Id
                orderby o.Sequence
                select new { b, o }).ToList();

            if (itemRequest.Sub_Category_Id != 0)
            {
                query = query.Where(x => x.o.Sub_Category_Id == itemRequest.Sub_Category_Id).ToList();
            }
            if (!string.IsNullOrEmpty(itemRequest.Like))
            {
                var likes = itemRequest.Like.Split(' ');
                if (likes.Length > 0)
                {
                    query = query.Where(x => likes.All(y => x.b.Restaurant_Name_E.ToLower().Contains(y.ToLower())
                                                            || (x.b.Restaurant_Name_A != null && x.b.Restaurant_Name_A.ToLower().Contains(y.ToLower()))
                                                            || (x.b.Restaurant_Desc_E != null && x.b.Restaurant_Desc_E.ToLower().Contains(y.ToLower()))
                                                            || (x.b.Restaurant_Desc_A != null && x.b.Restaurant_Desc_A.ToLower().Contains(y.ToLower()))
                    )).ToList();
                }
            }
            /*var brandIds = query.Select(x => x.o.Brand_Id).Distinct().ToList();
            var brandQuery = (from o in context.sm_brands
                              from r in context.sm_restaurants
                              where brandIds.Contains(o.Brand_Id) && o.Restaurant_Id == r.Restaurant_Id
                              select new { o, r }).ToList();*/
            foreach (var brand in query)
            {
                var brandDM = brand.b;                  
                var brand_categories = GetBrandCategories(brand.b.Restaurant_Id);
                if (lang == "A")
                {
                    brandDM.Categories = string.Join(",", brand_categories.Select(x => x.Sub_Category_Name_E).ToList());
                }
                else
                {
                    brandDM.Categories = string.Join(",", brand_categories.Select(x => x.Sub_Category_Name_A).ToList());
                }

                appPosts.Add(brandDM);
            }


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return appPosts.Distinct().ToList();
    }

    public List<SM_Sub_Categories> GetBrandCategories(long brand_id)
    {
        var categoryList = new List<SM_Sub_Categories>();
        try
        {

            categoryList = (from o in _context.sm_restaurants
                from p in _context.sm_products
                from s in _context.sm_sub_categories
                where o.Restaurant_Id == brand_id && o.Restaurant_Id == p.Restaurant_Id && p.Sub_Category_Id == s.Sub_Category_Id
                select s).Distinct().ToList();

            /* if (lang == "A")
             {
                 categories = string.Join(",", area.Select(x => x.Sub_Category_Name_E).ToList()) ;
             }
             else
             {
                 categories = string.Join(",", area.Select(x => x.Sub_Category_Name_A).ToList());
             }*/
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return categoryList;
    }

    public List<SM_Products> GetBrandCategoryProducts(long brand_id, long cat_id)
    {
        var productList = new List<SM_Products>();
        try
        {

            productList = (from o in _context.sm_restaurants
                from p in _context.sm_products
                from s in _context.sm_sub_categories
                where o.Restaurant_Id == brand_id && o.Restaurant_Id == p.Restaurant_Id
                                                  && p.Sub_Category_Id == s.Sub_Category_Id
                                                  && p.Sub_Category_Id == cat_id
                select p).Distinct().ToList();


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return productList;
    }

     
}