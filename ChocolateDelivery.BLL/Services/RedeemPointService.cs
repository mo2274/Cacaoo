using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL;

public class RedeemPointService
{
    private readonly AppDbContext _context;

    public RedeemPointService(AppDbContext benayaatEntities)
    {
        _context = benayaatEntities;

    }
    public LP_POINTS_TRANSACTION CreatePointTransaction(LP_POINTS_TRANSACTION pointDM)
    {
        try
        {
            var query = (from o in _context.lp_points_transaction
                where o.TXN_Id == pointDM.TXN_Id
                select o).FirstOrDefault();

            if (query != null)
            {
                    
            }
            else
            {
                _context.lp_points_transaction.Add(pointDM);
            }
            _context.SaveChanges();
        }

        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return pointDM;
    }

}