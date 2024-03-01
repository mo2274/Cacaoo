using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL;

public class ChefService
{
    private readonly AppDbContext _context;

    public ChefService(AppDbContext benayaatEntities)
    {
        _context = benayaatEntities;

    }


    public SM_Chefs CreateChef(SM_Chefs categoryDM)
    {
        try
        {
            var query = (from o in _context.sm_chefs
                where o.Chef_Id == categoryDM.Chef_Id
                select o).FirstOrDefault();

            if (query != null)
            {
                query.Chef_Name_E = categoryDM.Chef_Name_E;
                query.Chef_Name_A = categoryDM.Chef_Name_A;
                query.Chef_Desc_E = categoryDM.Chef_Desc_E;
                query.Chef_Desc_A = categoryDM.Chef_Desc_A;
                query.Show = categoryDM.Show;
                query.Sequence = categoryDM.Sequence;
                query.Updated_By = categoryDM.Updated_By;
                query.Updated_Datetime = categoryDM.Updated_Datetime;
                if (!string.IsNullOrEmpty(categoryDM.Image_URL))
                {
                    query.Image_URL = categoryDM.Image_URL;
                }
            }
            else
            {
                _context.sm_chefs.Add(categoryDM);
            }
            _context.SaveChanges();
        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return categoryDM;
    }

    public SM_Chefs? GetChef(int chef_id)
    {
        var area = new SM_Chefs();
        try
        {
            area = (from o in _context.sm_chefs
                where o.Chef_Id == chef_id
                select o).FirstOrDefault();
            if (area != null) {
                area.SM_Chef_Products = GetAllChefProducts(chef_id);
            }

        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return area;
    }

    public SM_Chef_Products CreateChefProduct(SM_Chef_Products invoiceDM)
    {
        try
        {
            var query = (from o in _context.sm_chef_products
                where o.Id == invoiceDM.Id
                select o).FirstOrDefault();

            if (query != null)
            {
                query.Sequence = invoiceDM.Sequence;
                   
            }
            else
            {
                _context.sm_chef_products.Add(invoiceDM);
            }
            _context.SaveChanges();
        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return invoiceDM;
    }

    public List<SM_Chef_Products> GetAllChefProducts(long chef_id)
    {

        var appPosts = new List<SM_Chef_Products>();
        try
        {

            var query = (from c in _context.sm_chef_products
                from o in _context.sm_products                            
                where o.Product_Id == c.Product_Id && c.Chef_Id == chef_id
                orderby o.Sequence
                select new { c, o }).ToList();
             

            foreach (var product in query)
            {
                var prodDM = product.c;
                prodDM.Product_Name = product.o.Product_Name_E;                   
                appPosts.Add(prodDM);
            }


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return appPosts;
    }

    public AppProducts GetChefProducts(long chef_id)
    {

        var appPosts = new AppProducts();
        try
        {

            var query = (from c in _context.sm_chef_products
                from o in _context.sm_products
                join b in _context.sm_brands on o.Brand_Id equals b.Brand_Id into brand
                from b in brand.DefaultIfEmpty()
                where o.Show && o.Publish && o.Product_Id == c.Product_Id && c.Chef_Id == chef_id
                orderby o.Sequence
                select new { b, o }).ToList();


            var total = query.Count;
            appPosts.Total_Items = total;

            foreach (var product in query)
            {
                var prodDM = product.o;
                prodDM.Brand_Name_E = product.b != null ? product.b.Brand_Name_E : "";
                prodDM.Brand_Name_A = product.b != null ? product.b.Brand_Name_A : "";
                appPosts.Items.Add(prodDM);
            }


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return appPosts;
    }

    public bool DeleteChefProduct(long detail_id)
    {
        try
        {
            var detail = (from o in _context.sm_chef_products
                where o.Id == detail_id
                select o).FirstOrDefault();
            if (detail != null)
            {
                _context.sm_chef_products.Remove(detail);
                _context.SaveChanges();
            }
            else {
                return false;
            }

        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return true;
    }
}