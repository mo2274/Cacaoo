using ChocolateDelivery.DAL;


namespace ChocolateDelivery.BLL
{
    public class CarouselBC
    {
        private ChocolateDeliveryEntities context;

        public CarouselBC(ChocolateDeliveryEntities chocolateDeliveryEntities)
        {
            context = chocolateDeliveryEntities;

        }
        public List<SM_Carousels> GetAllCarousels()
        {
            var userMenu = new List<SM_Carousels>();
            try
            {
                userMenu = (from o in context.sm_carousels
                            where o.Is_Deleted == false
                            select o).ToList();


            }
            catch (Exception ex)
            {
                 throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public List<SM_Carousels> GetCarousels()
        {
            var userMenu = new List<SM_Carousels>();
            try
            {
                userMenu = (from o in context.sm_carousels
                            where o.Show == true && o.Is_Deleted == false
                            orderby o.Sequence
                            select o).ToList();
            }
            catch (Exception ex)
            {
                 throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public SM_Carousels? GetCarousel(int Carousel_Id)
        {
            var userMenu = new SM_Carousels();
            try
            {
                userMenu = (from o in context.sm_carousels
                            where o.Carousel_Id == Carousel_Id && o.Is_Deleted == false
                            select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                 throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public SM_Carousels CreateCarousel(SM_Carousels CustomerDM)
        {
            try
            {
                if (CustomerDM.Carousel_Id != 0)
                {
                    var Customer = (from o in context.sm_carousels
                                    where o.Carousel_Id == CustomerDM.Carousel_Id
                                    select o).FirstOrDefault();

                    if (Customer != null)
                    {
                        Customer.Title_E = CustomerDM.Title_E;
                        Customer.Title_A = CustomerDM.Title_A;
                        Customer.Sub_Title_E = CustomerDM.Sub_Title_E;
                        Customer.Sub_Title_A = CustomerDM.Sub_Title_A;
                        if (!String.IsNullOrEmpty(CustomerDM.Media_URL))
                        {
                            Customer.Media_URL = CustomerDM.Media_URL;
                        }
                        if (!String.IsNullOrEmpty(CustomerDM.Thumbnail_URL))
                        {
                            Customer.Thumbnail_URL = CustomerDM.Thumbnail_URL;
                        }
                        Customer.Media_Type = CustomerDM.Media_Type;
                        Customer.Media_From_Type = CustomerDM.Media_From_Type;
                        Customer.Redirect_Id = CustomerDM.Redirect_Id;
                        Customer.Sequence = CustomerDM.Sequence;                       
                        Customer.Show = CustomerDM.Show;                       
                        Customer.Updated_By = CustomerDM.Updated_By;
                        Customer.Updated_Datetime = CustomerDM.Updated_Datetime;
                        context.SaveChanges();
                    }
                }
                else
                {
                    context.sm_carousels.Add(CustomerDM);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                 throw new Exception(ex.ToString());
            }


            return CustomerDM;
        }

        public bool RemoveCarousel(SM_Carousels CustomerDM)
        {
            var isremoved = false;
            try
            {

                var Customer = (from o in context.sm_carousels
                                where o.Carousel_Id == CustomerDM.Carousel_Id
                                select o).FirstOrDefault();

                if (Customer != null)
                {
                    Customer.Is_Deleted = CustomerDM.Is_Deleted;
                    Customer.Deleted_By = CustomerDM.Deleted_By;
                    Customer.Deleted_Datetime = CustomerDM.Deleted_Datetime;
                    context.SaveChanges();

                    isremoved = true;
                }
                else
                {
                    isremoved = false;
                }
            }


            catch (Exception ex)
            {
                isremoved = false;
                 throw new Exception(ex.ToString());
            }


            return isremoved;
        }

        public long GetMaxCarouselId()
        {
            long maxpromoterid = 1;
            var userMenu = new SM_Carousels();
            try
            {
                userMenu = (from o in context.sm_carousels
                            orderby o.Carousel_Id descending
                            select o).FirstOrDefault();
                if (userMenu != null)
                    maxpromoterid = userMenu.Carousel_Id + 1;
            }
            catch (Exception ex)
            {
                 throw new Exception(ex.ToString());
            }
            return maxpromoterid;
        }

        public SM_Carousels? UpdateSequence(long carousel_id, int sequence)
        {
            var category = new SM_Carousels();
            try
            {
                category = (from o in context.sm_carousels
                            where o.Carousel_Id == carousel_id
                            select o).FirstOrDefault();

                if (category != null)
                {
                    category.Sequence = sequence;
                    context.SaveChanges();
                }


            }
            catch (Exception ex)
            {
                 throw new Exception(ex.ToString());
            }


            return category;
        }
    }
}
