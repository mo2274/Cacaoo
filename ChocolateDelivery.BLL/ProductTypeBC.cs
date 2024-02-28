using ChocolateDelivery.DAL;

namespace ChocolateDelivery.BLL
{
    public class ProductTypeBC
    {
        private ChocolateDeliveryEntities context;

        public ProductTypeBC(ChocolateDeliveryEntities benayaatEntities)
        {
            context = benayaatEntities;

        }

        public SM_Product_Types CreateType(SM_Product_Types typeDM)
        {
            try
            {
                var query = (from o in context.sm_product_types
                             where o.Type_Id == typeDM.Type_Id
                             select o).FirstOrDefault();

                if (query != null)
                {
                    query.Type_Name_E = typeDM.Type_Name_E;
                    query.Type_Name_A = typeDM.Type_Name_A;
                    query.Type_Desc_E = typeDM.Type_Desc_E;
                    query.Type_Desc_A = typeDM.Type_Desc_A;
                    query.Image_URL = typeDM.Image_URL;
                    query.Show = typeDM.Show;
                    query.Sequence = typeDM.Sequence;
                    query.Updated_By = typeDM.Updated_By;
                    query.Updated_Datetime = typeDM.Updated_Datetime;
                }
                else
                {
                    context.sm_product_types.Add(typeDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return typeDM;
        }

        public SM_Product_Types? GetType(int type_id)
        {
            var area = new SM_Product_Types();
            try
            {


                area = (from o in context.sm_product_types
                        where o.Type_Id == type_id
                        select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return area;
        }
    }
}