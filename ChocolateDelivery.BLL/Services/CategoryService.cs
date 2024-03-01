using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL;

public class CategoryService
{
    private readonly AppDbContext _context;
      
    public CategoryService(AppDbContext benayaatEntities)
    {
        _context = benayaatEntities;           

    }

    public SM_Main_Categories CreateMainCategory(SM_Main_Categories categoryDM)
    {
        try
        {
            var query = (from o in _context.sm_main_categories
                where o.Category_Id == categoryDM.Category_Id
                select o).FirstOrDefault();

            if (query != null)
            {
                query.Category_Name_E = categoryDM.Category_Name_E;
                query.Category_Name_A = categoryDM.Category_Name_A;
                query.Category_Desc_E = categoryDM.Category_Desc_E;
                query.Category_Desc_A = categoryDM.Category_Desc_A;                   
                query.Show = categoryDM.Show;
                query.Sequence = categoryDM.Sequence;
                query.Updated_By = categoryDM.Updated_By;
                query.Updated_Datetime = categoryDM.Updated_Datetime;
            }
            else
            {
                _context.sm_main_categories.Add(categoryDM);
            }
            _context.SaveChanges();
        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return categoryDM;
    }

    public SM_Main_Categories? GetMainCategory(int category_id)
    {
        var area = new SM_Main_Categories();
        try
        {


            area = (from o in _context.sm_main_categories
                where o.Category_Id == category_id
                select o).FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return area;
    }

    public SM_Categories CreateCategory(SM_Categories categoryDM)
    {
        try
        {
            var query = (from o in _context.sm_categories
                where o.Category_Id == categoryDM.Category_Id
                select o).FirstOrDefault();

            if (query != null)
            {
                query.Category_Name_E = categoryDM.Category_Name_E;
                query.Category_Name_A = categoryDM.Category_Name_A;
                query.Category_Desc_E = categoryDM.Category_Desc_E;
                query.Category_Desc_A = categoryDM.Category_Desc_A;
                query.Show = categoryDM.Show;
                query.Sequence = categoryDM.Sequence;
                query.Updated_By = categoryDM.Updated_By;
                query.Updated_Datetime = categoryDM.Updated_Datetime;
                if (!string.IsNullOrEmpty(categoryDM.Image_URL)) {
                    query.Image_URL = categoryDM.Image_URL;
                }
                query.Background_Color = categoryDM.Background_Color;
                query.Is_Gift = categoryDM.Is_Gift;
            }
            else
            {
                _context.sm_categories.Add(categoryDM);
            }
            _context.SaveChanges();
        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return categoryDM;
    }

    public SM_Categories? GetCategory(int category_id)
    {
        var area = new SM_Categories();
        try
        {


            area = (from o in _context.sm_categories
                where o.Category_Id == category_id
                select o).FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return area;
    }

    public List<SM_Categories> GetCategories()
    {

        var categories = new List<SM_Categories>();
        try
        {
            categories = (from o in _context.sm_categories                            
                where o.Show
                orderby o.Sequence
                select o).ToList();

              
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return categories;
    }

    public List<SM_Sub_Categories> GetSubCategories(long cat_id)
    {

        var categories = new List<SM_Sub_Categories>();
        try
        {
            categories = (from o in _context.sm_sub_categories
                where o.Show && o.Category_Id == cat_id
                orderby o.Sequence
                select o).ToList();


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return categories;
    }
}