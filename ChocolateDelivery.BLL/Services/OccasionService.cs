using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL;

public class OccasionService
{
    private readonly AppDbContext _context;

    public OccasionService(AppDbContext benayaatEntities)
    {
        _context = benayaatEntities;

    }      

    public SM_Occasions CreateOccasion(SM_Occasions categoryDM)
    {
        try
        {
            var query = (from o in _context.sm_occasions
                where o.Occasion_Id == categoryDM.Occasion_Id
                select o).FirstOrDefault();

            if (query != null)
            {
                query.Occasion_Name_E = categoryDM.Occasion_Name_E;
                query.Occasion_Name_A = categoryDM.Occasion_Name_A;
                query.Occasion_Desc_E = categoryDM.Occasion_Desc_E;
                query.Occasion_Desc_A = categoryDM.Occasion_Desc_A;
                query.Show = categoryDM.Show;
                query.Sequence = categoryDM.Sequence;
                query.Updated_By = categoryDM.Updated_By;
                query.Updated_Datetime = categoryDM.Updated_Datetime;
                if (!string.IsNullOrEmpty(categoryDM.Image_URL))
                {
                    query.Image_URL = categoryDM.Image_URL;
                }
                query.Background_Color = categoryDM.Background_Color;
            }
            else
            {
                _context.sm_occasions.Add(categoryDM);
            }
            _context.SaveChanges();
        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return categoryDM;
    }

    public SM_Occasions? GetOccasion(int category_id)
    {
        var area = new SM_Occasions();
        try
        {


            area = (from o in _context.sm_occasions
                where o.Occasion_Id == category_id
                select o).FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return area;
    }

    public List<SM_Occasions> GetOccasions()
    {

        var occasions = new List<SM_Occasions>();
        try
        {
            occasions = (from o in _context.sm_occasions
                where o.Show
                orderby o.Sequence
                select o).ToList();


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return occasions;
    }

       
}