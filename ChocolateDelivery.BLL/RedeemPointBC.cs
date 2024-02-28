using ChocolateDelivery.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class RedeemPointBC
    {
        private ChocolateDeliveryEntities context;

        public RedeemPointBC(ChocolateDeliveryEntities benayaatEntities)
        {
            context = benayaatEntities;

        }
        public LP_POINTS_TRANSACTION CreatePointTransaction(LP_POINTS_TRANSACTION pointDM)
        {
            try
            {
                var query = (from o in context.lp_points_transaction
                             where o.TXN_Id == pointDM.TXN_Id
                             select o).FirstOrDefault();

                if (query != null)
                {
                    
                }
                else
                {
                    context.lp_points_transaction.Add(pointDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return pointDM;
        }

    }
}
