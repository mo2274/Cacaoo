using ChocolateDelivery.DAL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class AreaBC
    {
        private ChocolateDeliveryEntities context;

        public AreaBC(ChocolateDeliveryEntities chocolateDeliveryEntities)
        {
            context = chocolateDeliveryEntities;
        }
        public List<SM_Areas> GetAllAreas()
        {
            var userMenu = new List<SM_Areas>();
            try
            {
                userMenu = (from o in context.sm_areas                            
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
                userMenu = (from o in context.sm_areas
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
}
