using ChocolateDelivery.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class SubCategoryBC
    {
        private ChocolateDeliveryEntities context;

        public SubCategoryBC(ChocolateDeliveryEntities benayaatEntities)
        {
            context = benayaatEntities;

        }
      
        public SM_Sub_Categories CreateSubCategory(SM_Sub_Categories categoryDM)
        {
            try
            {
                var query = (from o in context.sm_sub_categories
                             where o.Sub_Category_Id == categoryDM.Sub_Category_Id
                             select o).FirstOrDefault();

                if (query != null)
                {
                    query.Category_Id = categoryDM.Category_Id;
                    query.Sub_Category_Name_E = categoryDM.Sub_Category_Name_E;
                    query.Sub_Category_Name_A = categoryDM.Sub_Category_Name_A;
                    query.Sub_Category_Desc_E = categoryDM.Sub_Category_Desc_E;
                    query.Sub_Category_Desc_A = categoryDM.Sub_Category_Desc_A;
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
                    context.sm_sub_categories.Add(categoryDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return categoryDM;
        }

        public SM_Sub_Categories? GetSubCategory(int category_id)
        {
            var area = new SM_Sub_Categories();
            try
            {


                area = (from o in context.sm_sub_categories
                        where o.Sub_Category_Id == category_id
                        select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return area;
        }

        public List<SM_Sub_Categories> GetAllSubCategories()
        {

            var customer = new List<SM_Sub_Categories>();
            try
            {
                customer = (from o in context.sm_sub_categories                           
                            select o).ToList();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }

    }
}
