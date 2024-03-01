using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL;

public class AreaService
{
    private readonly AppDbContext _context;

    public AreaService(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }
    public List<SM_Areas> GetAllAreas()
    {
        var userMenu = new List<SM_Areas>();
        try
        {
            userMenu = (from o in _context.sm_areas                            
                select  o).ToList();
               
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }

    public List<SM_Areas> GetAreas()
    {
        var userMenu = new List<SM_Areas>();
        try
        {
            userMenu = (from o in _context.sm_areas
                where o.Show
                select o).ToList();

        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return userMenu;
    }
}