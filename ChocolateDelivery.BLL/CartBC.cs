using ChocolateDelivery.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class CartBC
    {
        private ChocolateDeliveryEntities context;

        public CartBC(ChocolateDeliveryEntities chocolateDeliveryEntities)
        {
            context = chocolateDeliveryEntities;
        }
        public TXN_Cart AddtoCart(TXN_Cart CustomerDM)
        {
            try
            {
                if (CustomerDM.Cart_Id != 0)
                {
                    var Customer = (from o in context.txn_cart
                                    where o.Cart_Id == CustomerDM.Cart_Id
                                    select o).FirstOrDefault();

                    if (Customer != null)
                    {
                        Customer.Qty = CustomerDM.Qty;
                        context.SaveChanges();
                    }
                }
                else
                {
                    context.txn_cart.Add(CustomerDM);
                    context.SaveChanges();
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }


            return CustomerDM;
        }

        public bool RemoveCartItem(long cart_id)
        {
            var isremoved = true;
            try
            {

                var Customer = (from o in context.txn_cart
                                where o.Cart_Id == cart_id
                                select o).FirstOrDefault();

                if (Customer != null)
                {
                    var addons = (from o in context.txn_cart_addons
                                  where o.Cart_Id == cart_id
                                  select o).ToList();

                    var cateringProducts = (from o in context.txn_cart_catering_products
                                            where o.Cart_Id == cart_id
                                            select o).ToList();

                    context.txn_cart_catering_products.RemoveRange(cateringProducts);
                    context.txn_cart_addons.RemoveRange(addons);
                    context.txn_cart.Remove(Customer);
                    context.SaveChanges();
                }
                else
                {
                    isremoved = false;
                }
            }


            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }


            return isremoved;
        }

        public CartResponse GetCartItems(long app_user_id, string lang)
        {
            RestaurantBC restaurantBC = new RestaurantBC(context);
            BranchBC branchBC = new BranchBC(context);
            var itemBC = new ProductBC(context);
            var response = new CartResponse();
            try
            {
                var query = (from o in context.txn_cart
                             from i in context.sm_products
                             from s in context.sm_sub_categories
                             from c in context.sm_categories
                             where o.App_User_Id == app_user_id && o.Product_Id == i.Product_Id
                             && i.Sub_Category_Id == s.Sub_Category_Id && s.Category_Id == c.Category_Id
                             select new { o, i, c.Is_Gift }).ToList();

                foreach (var item in query)
                {
                    var cartDTO = new CartDTO();
                    cartDTO.Cart_Id = item.o.Cart_Id;
                    cartDTO.Product_Id = item.o.Product_Id;
                    cartDTO.Product_Name = lang.ToUpper() == "E" ? item.i.Product_Name_E : item.i.Product_Name_A ?? "";
                    cartDTO.Image_Url = item.i.Image_URL ?? "";
                    cartDTO.Qty = item.o.Qty;
                    cartDTO.Rate = item.i.Price;
                    cartDTO.Amount = item.o.Qty * item.i.Price;
                    cartDTO.Comments = item.o.Comments ?? "";
                    cartDTO.Is_Currently_Available = item.i.Show && item.i.Publish;



                    cartDTO.Product_AddOns = (from o in context.txn_cart_addons
                                              from ad in context.sm_product_addons
                                              where o.Cart_Id == item.o.Cart_Id && o.Product_AddOnId == ad.Product_AddOnId
                                              select new ProductAddOnDTO
                                              {
                                                  Product_AddOn_Id = o.Product_AddOnId,
                                                  Product_AddOn_Name = lang.ToUpper() == "E" ? ad.Line_AddOn_Name_E : ad.Line_AddOn_Name_A ?? ad.Line_AddOn_Name_E,
                                                  Product_AddOn_Price = ad.Price
                                              }).ToList();

                    cartDTO.Catering_Products = (from o in context.txn_cart_catering_products
                                                 from ad in context.sm_product_catering_products
                                                 from cp in context.sm_products
                                                 where o.Cart_Id == item.o.Cart_Id && o.Catering_Product_Id == ad.Catering_Product_Id && ad.Child_Product_Id == cp.Product_Id
                                                 select new CartCateringProductsDTO
                                                 {
                                                     Catering_Product_Id = o.Catering_Product_Id,
                                                     Catering_Product = lang.ToUpper() == "E" ? cp.Product_Name_E :cp.Product_Name_A ?? cp.Product_Name_E,
                                                     Qty = o.Qty
                                                 }).ToList();

                    response.CartItems.Add(cartDTO);
                    response.Is_Gift_Items = item.Is_Gift;
                }

                if (query.Count > 0)
                {
                    var restaurantDM = restaurantBC.GetRestaurant(query.FirstOrDefault().i.Restaurant_Id);
                    if (restaurantDM != null)
                    {
                        RestaurantDTO restaurantDTO = new RestaurantDTO();
                        restaurantDTO.Restaurant_Id = restaurantDM.Restaurant_Id;
                        restaurantDTO.Delivery_Charges = restaurantDM.Delivery_Charge;
                        restaurantDTO.Restaurant_Name = lang.ToUpper() == "E" ? restaurantDM.Restaurant_Name_E : restaurantDM.Restaurant_Name_A ?? "";
                        var branches = branchBC.GetRestaurantBranches(restaurantDM.Restaurant_Id);
                        foreach (var branch in branches)
                        {
                            var branchDTO = new BranchDTO();
                            branchDTO.Branch_Id = branch.Branch_Id;
                            branchDTO.Branch_Name = lang.ToUpper() == "E" ? branch.Branch_Name_E : branch.Branch_Name_A ?? "";
                            branchDTO.Branch_Address = lang.ToUpper() == "E" ? branch.Address_E ?? "" : branch.Address_A ?? "";
                            branchDTO.Latitude = branch.Latitude ?? 0;
                            branchDTO.Longitude = branch.Longitude ?? 0;
                            if (branch.Opening_Time != null)
                            {
                                branchDTO.Opening_Time = DateTime.Today.Add((TimeSpan)branch.Opening_Time).ToString("hh:mm tt");
                            }
                            if (branch.Closing_Time != null)
                            {
                                branchDTO.Closing_Time = DateTime.Today.Add((TimeSpan)branch.Closing_Time).ToString("hh:mm tt");
                            }
                            restaurantDTO.Branches.Add(branchDTO);
                        }
                        response.Restaurant_Details = restaurantDTO;
                    }
                }
                response.Status = 0;
                response.Message = ServiceResponse.Success;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return response;
        }

        public TXN_Cart_AddOns CreateCartAddOn(TXN_Cart_AddOns invoiceDM)
        {
            try
            {
                var query = (from o in context.txn_cart_addons
                             where o.Cart_AddOnId == invoiceDM.Cart_AddOnId
                             select o).FirstOrDefault();

                if (query != null)
                {

                }
                else
                {
                    context.txn_cart_addons.Add(invoiceDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return invoiceDM;
        }

        public TXN_Cart? GetCart(long cart_id)
        {

            var userMenu = new TXN_Cart();
            try
            {
                userMenu = (from o in context.txn_cart
                            where o.Cart_Id == cart_id
                            select o).FirstOrDefault();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public TXN_Cart? GetCart(long app_user_id, long product_id)
        {

            var userMenu = new TXN_Cart();
            try
            {
                userMenu = (from o in context.txn_cart
                            where o.App_User_Id == app_user_id && o.Product_Id == product_id
                            select o).FirstOrDefault();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public bool IsValidVendor(long app_user_id, long product_id)
        {
            bool isValid = true;
            try
            {
                var query = (from o in context.txn_cart
                             from i in context.sm_products
                             where o.App_User_Id == app_user_id && o.Product_Id == i.Product_Id
                             select i).FirstOrDefault();
                if (query != null)
                {
                    var addedProduct = (from o in context.sm_products
                                        where o.Product_Id == product_id
                                        select o).FirstOrDefault();

                    if (addedProduct != null && addedProduct.Restaurant_Id != query.Restaurant_Id)
                    {
                        isValid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return isValid;
        }

        public bool IsValidCategory(long app_user_id, long product_id)
        {
            bool isValid = true;
            try
            {
                var query = (from o in context.txn_cart
                             from i in context.sm_products
                             from sb in context.sm_sub_categories
                             from c in context.sm_categories
                             where o.App_User_Id == app_user_id && o.Product_Id == i.Product_Id
                             && i.Sub_Category_Id == sb.Sub_Category_Id && sb.Category_Id == c.Category_Id
                           
                             select new { i, c }).FirstOrDefault();
                if (query != null)
                {
                    var addedProduct = (from o in context.sm_products
                                        from sb in context.sm_sub_categories
                                        from c in context.sm_categories
                                        where o.Product_Id == product_id && o.Sub_Category_Id == sb.Sub_Category_Id && sb.Category_Id == c.Category_Id
                                        select new { o, c }).FirstOrDefault();

                    if (addedProduct != null && addedProduct.c.Is_Gift != query.c.Is_Gift)
                    {
                        isValid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return isValid;
        }

        public bool RemoveCart(long app_user_id)
        {
            var isremoved = true;
            try
            {

                var cart = (from o in context.txn_cart
                            where o.App_User_Id == app_user_id
                            select o).ToList();

                foreach (var item in cart)
                {
                    var cartAddOns = (from o in context.txn_cart_addons
                                      where o.Cart_Id == item.Cart_Id
                                      select o).ToList();
                    foreach (var addon in cartAddOns)
                    {
                        context.txn_cart_addons.Remove(addon);
                    }
                    context.txn_cart.Remove(item);
                }
                context.SaveChanges();

            }


            catch (Exception ex)
            {
                isremoved = false;
                throw new Exception(ex.ToString());
            }


            return isremoved;
        }

        public TXN_Cart_Catering_Products CreateCartCateringProduct(TXN_Cart_Catering_Products invoiceDM)
        {
            try
            {
                var query = (from o in context.txn_cart_catering_products
                             where o.Cart_Catering_Product_Id == invoiceDM.Cart_Catering_Product_Id
                             select o).FirstOrDefault();

                if (query != null)
                {

                }
                else
                {
                    context.txn_cart_catering_products.Add(invoiceDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return invoiceDM;
        }
    }
}
