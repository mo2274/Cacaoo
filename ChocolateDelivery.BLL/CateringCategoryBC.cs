using ChocolateDelivery.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class CateringCategoryBC
    {
        private ChocolateDeliveryEntities context;

        public CateringCategoryBC(ChocolateDeliveryEntities benayaatEntities)
        {
            context = benayaatEntities;

        }
       
        public SM_Catering_Categories CreateCategory(SM_Catering_Categories categoryDM)
        {
            try
            {
                var query = (from o in context.sm_catering_categories
                             where o.Category_Id == categoryDM.Category_Id
                             select o).FirstOrDefault();

                if (query != null)
                {
                    query.Category_Name_E = categoryDM.Category_Name_E;
                    query.Category_Name_A = categoryDM.Category_Name_A;
                    query.Qty = categoryDM.Qty;                   
                    query.Show = categoryDM.Show;
                    query.Sequence = categoryDM.Sequence;
                    query.Updated_By = categoryDM.Updated_By;
                    query.Updated_Datetime = categoryDM.Updated_Datetime;
                  
                }
                else
                {
                    context.sm_catering_categories.Add(categoryDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return categoryDM;
        }

        public SM_Catering_Categories? GetCategory(int category_id)
        {
            var area = new SM_Catering_Categories();
            try
            {


                area = (from o in context.sm_catering_categories
                        where o.Category_Id == category_id
                        select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return area;
        }

        public List<SM_Catering_Categories> GetCategories()
        {

            var categories = new List<SM_Catering_Categories>();
            try
            {
                categories = (from o in context.sm_catering_categories
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

      
    }
}
