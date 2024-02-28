using ChocolateDelivery.DAL;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace ChocolateDelivery.BLL
{
    public class ProductBC
    {
        private ChocolateDeliveryEntities context;

        public ProductBC(ChocolateDeliveryEntities benayaatEntities)
        {
            context = benayaatEntities;

        }

        public SM_Products CreateProduct(SM_Products productDM)
        {
            try
            {
                var query = (from o in context.sm_products
                             where o.Product_Id == productDM.Product_Id
                             select o).FirstOrDefault();

                if (query != null)
                {
                    query.Main_Category_Id = productDM.Main_Category_Id;
                    query.Product_Type_Id = productDM.Product_Type_Id;
                    query.Sub_Category_Id = productDM.Sub_Category_Id;
                    query.Product_Name_E = productDM.Product_Name_E;
                    query.Product_Name_A = productDM.Product_Name_A;
                    query.Product_Desc_E = productDM.Product_Desc_E;
                    query.Product_Desc_A = productDM.Product_Desc_A;
                    query.Price = productDM.Price;
                    query.Stock_In_Hand = productDM.Stock_In_Hand;
                    query.Show = productDM.Show;
                    query.Publish = productDM.Publish;
                    query.Sequence = productDM.Sequence;
                    query.Updated_By = productDM.Updated_By;
                    query.Updated_Datetime = productDM.Updated_Datetime;
                    if (!string.IsNullOrEmpty(productDM.Image_URL))
                    {
                        query.Image_URL = productDM.Image_URL;
                    }
                    query.Comments = productDM.Comments;
                    query.Brand_Id = productDM.Brand_Id;
                    query.Is_Exclusive = productDM.Is_Exclusive;
                }
                else
                {
                    context.sm_products.Add(productDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return productDM;
        }

        public SM_Products UpdateProductByAdmin(SM_Products productDM)
        {
            try
            {
                var query = (from o in context.sm_products
                             where o.Product_Id == productDM.Product_Id
                             select o).FirstOrDefault();

                if (query != null)
                {
                    query.Publish = productDM.Publish;
                    query.Admin_Comments = productDM.Admin_Comments;
                    query.Status_Id = productDM.Status_Id;
                    context.SaveChanges();
                }


            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return productDM;
        }

        public SM_Products? GetProduct(long product_id, long app_user_id = 0)
        {
            var area = new SM_Products();
            try
            {


                var query = (from o in context.sm_products
                             from sc in context.sm_sub_categories
                             from c in context.sm_categories
                             join f in context.txn_favorite on o.Product_Id equals f.Product_Id into favorite
                             from f in favorite.Where(x => x.App_User_Id == app_user_id).DefaultIfEmpty()
                             where o.Product_Id == product_id && o.Sub_Category_Id == sc.Sub_Category_Id && sc.Category_Id == c.Category_Id
                             select new { o, sc, f, c }).FirstOrDefault();

                if (query != null)
                {
                    area = query.o;
                    area.Category_Id = query.sc.Category_Id;
                    area.Is_Gift_Product = query.c.Is_Gift;
                    area.SM_Product_AddOns = GetAllProductAddOns(product_id);
                    area.SM_Product_Branches = GetAllProductBranches(product_id, query.o.Restaurant_Id);
                    area.SM_Product_Catering_Products = GetAllProductCateringProducts(product_id);
                    if (query.f != null)
                    {
                        area.Is_Favorite = true;
                    }
                    area.Occasion_Ids = GetAllProductOccasions(product_id).Select(x => x.Occasion_Id.ToString()).ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return area;
        }

        public AppProducts GetAppProducts(ProductRequest itemRequest)
        {

            var appPosts = new AppProducts();
            try
            {
                var current_kwt_time = StaticMethods.GetKuwaitTime();
                var query = (from o in context.sm_products
                             join b in context.sm_brands on o.Brand_Id equals b.Brand_Id into brand
                             from b in brand.DefaultIfEmpty()
                             where o.Show && o.Publish
                             && !o.Is_Catering_Menu_Product
                             orderby o.Sequence
                             select new { b, o }).ToList();

                if (itemRequest.Sub_Category_Id != 0)
                {
                    query = query.Where(x => x.o.Sub_Category_Id == itemRequest.Sub_Category_Id).ToList();
                }
                if (!string.IsNullOrEmpty(itemRequest.Like))
                {
                    var likes = itemRequest.Like.Split(' ');
                    if (likes.Length > 0)
                    {
                        query = query.Where(x => likes.All(y => x.o.Product_Name_E.ToLower().Contains(y.ToLower())
                     || (x.o.Product_Name_A != null && x.o.Product_Name_A.ToLower().Contains(y.ToLower()))
                     || (x.o.Product_Desc_E != null && x.o.Product_Desc_E.ToLower().Contains(y.ToLower()))
                     || (x.o.Product_Desc_A != null && x.o.Product_Desc_A.ToLower().Contains(y.ToLower()))
                    )).ToList();
                    }
                }
                if (itemRequest.Occasion_Id != 0)
                {
                    query = (from o in query
                             from oc in context.sm_product_occasions
                             where o.o.Product_Id == oc.Product_Id
                             select o).Distinct().ToList();
                }

                var total = query.Count;
                appPosts.Total_Items = total;
                if (itemRequest.Page_No != 0)
                {
                    var pageSize = 20;
                    var skip = pageSize * (itemRequest.Page_No - 1);

                    if (skip > total)
                    {
                        skip = (total / 20) * pageSize;
                    }

                    query = query
                     .Skip(skip)
                     .Take(pageSize)
                     .ToList();
                }
                foreach (var product in query)
                {
                    var prodDM = product.o;
                    prodDM.Brand_Name_E = product.b != null ? product.b.Brand_Name_E : "";
                    prodDM.Brand_Name_A = product.b != null ? product.b.Brand_Name_A : "";
                    appPosts.Items.Add(prodDM);
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return appPosts;
        }

        public SM_Product_AddOns CreateProductAddOn(SM_Product_AddOns invoiceDM)
        {
            try
            {
                var query = (from o in context.sm_product_addons
                             where o.Product_AddOnId == invoiceDM.Product_AddOnId
                             select o).FirstOrDefault();

                if (query != null)
                {
                    query.Line_AddOn_Name_E = invoiceDM.Line_AddOn_Name_E;
                    query.Line_AddOn_Name_A = invoiceDM.Line_AddOn_Name_A;
                    query.Price = invoiceDM.Price;
                    query.Publish = invoiceDM.Publish;
                }
                else
                {
                    context.sm_product_addons.Add(invoiceDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return invoiceDM;
        }
        public List<SM_Product_AddOns> GetAllProductAddOns(long product_id)
        {

            var customer = new List<SM_Product_AddOns>();
            try
            {
                var query = (from o in context.sm_product_addons
                             join u in context.sm_restaurant_addons on o.AddOn_Id equals u.AddOn_Id into AddOn
                             from u in AddOn.DefaultIfEmpty()
                             where o.Product_Id == product_id && !o.Is_Deleted
                             select new { o, u }).ToList();

                foreach (var detail in query)
                {
                    var detailDM = new SM_Product_AddOns();
                    detailDM = detail.o;
                    detailDM.AddOn_Name = detail.u != null ? detail.u.AddOn_Name_E : "";
                    detailDM.AddOn_Type = detail.u != null ? detail.u.AddOn_Type_Id == AddOn_Types.MANDATORY ? "Mandatory" : "Optional" : "";
                    customer.Add(detailDM);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }

        public List<AddOnDTO> GetProductAddOns(long product_id, string lang = "E")
        {
            AddOnBC addOnBC = new AddOnBC(context);
            var customer = new List<AddOnDTO>();
            try
            {
                var query = (from o in context.sm_product_addons
                             where o.Product_Id == product_id && !o.Is_Deleted && o.Publish
                             select o.AddOn_Id).Distinct().ToList();

                foreach (var addon in query)
                {
                    var addOnDTO = new AddOnDTO();
                    var addOnDM = addOnBC.GetAddOn(addon);
                    if (addOnDM != null)
                    {
                        addOnDTO.AddOn_Id = addOnDM.AddOn_Id;
                        addOnDTO.AddOn_Name = lang.ToUpper() == "E" ? addOnDM.AddOn_Name_E : addOnDM.AddOn_Name_A ?? "";
                        addOnDTO.AddOn_Desc = lang.ToUpper() == "E" ? addOnDM.AddOn_Desc_E ?? "" : addOnDM.AddOn_Desc_A ?? "";
                        addOnDTO.AddOn_Type = addOnDM.AddOn_Type_Id;
                        var options = (from o in context.sm_product_addons
                                       where o.Product_Id == product_id && !o.Is_Deleted && o.Publish
                                       && o.AddOn_Id == addOnDM.AddOn_Id
                                       select new AddOnOptionDTO
                                       {
                                           Option_Id = o.Product_AddOnId,
                                           Option_Name = lang.ToUpper() == "E" ? o.Line_AddOn_Name_E : o.Line_AddOn_Name_A ?? "",
                                           Price = o.Price
                                       }).ToList();
                        addOnDTO.Options.AddRange(options);
                        customer.Add(addOnDTO);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }

        public SM_Product_AddOns? GetProductAddOn(long product_addon_id)
        {
            var area = new SM_Product_AddOns();
            try
            {

                area = (from o in context.sm_product_addons
                        where o.Product_AddOnId == product_addon_id && !o.Is_Deleted
                        select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return area;
        }
        public bool DeleteProductAddOn(SM_Product_AddOns docDM)
        {
            try
            {
                var detail = (from o in context.sm_product_addons
                              where o.Product_AddOnId == docDM.Product_AddOnId
                              select o).FirstOrDefault();
                if (detail != null)
                {
                    detail.Is_Deleted = true;
                    detail.Deleted_By = docDM.Deleted_By;
                    detail.Deleted_Datetime = docDM.Deleted_Datetime;
                    context.SaveChanges();
                }

            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return true;
        }

        public ProductDetailResponse GetProductDetail(long product_id)
        {
            ProductDetailResponse response = new ProductDetailResponse();
            try
            {
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return response;
        }

        #region Occasions
        public SM_Product_Occasions CreateProductOccasion(SM_Product_Occasions invoiceDM)
        {
            try
            {
                var query = (from o in context.sm_product_occasions
                             where o.Product_Occasion_Id == invoiceDM.Product_Occasion_Id
                             select o).FirstOrDefault();

                if (query != null)
                {

                }
                else
                {
                    context.sm_product_occasions.Add(invoiceDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return invoiceDM;
        }

        public bool DeleteProductOccasions(long product_id)
        {
            try
            {
                var detail = (from o in context.sm_product_occasions
                              where o.Product_Id == product_id
                              select o).ToList();
                if (detail.Count > 0)
                {
                    context.sm_product_occasions.RemoveRange(detail);
                    context.SaveChanges();
                }

            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return true;
        }

        public List<SM_Product_Occasions> GetAllProductOccasions(long product_id)
        {

            var customer = new List<SM_Product_Occasions>();
            try
            {
                customer = (from o in context.sm_product_occasions
                            where o.Product_Id == product_id
                            select o).ToList();


            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }
        #endregion

        #region Branches

        public SM_Product_Branches CreateProductBranch(SM_Product_Branches invoiceDM)
        {
            try
            {
                var query = (from o in context.sm_product_branches
                             where o.Product_Branch_Id == invoiceDM.Product_Branch_Id
                             select o).FirstOrDefault();

                if (query != null)
                {
                    query.Is_Available = invoiceDM.Is_Available;
                }
                else
                {
                    context.sm_product_branches.Add(invoiceDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return invoiceDM;
        }

        public List<SM_Product_Branches> GetAllProductBranches(long product_id, long restaurant_id)
        {
            BranchBC branchBC = new BranchBC(context);

            var customer = new List<SM_Product_Branches>();
            try
            {
                var query = (from o in context.sm_product_branches
                             where o.Product_Id == product_id
                             select o).ToList();

                var branches = branchBC.GetRestaurantBranches(restaurant_id);
                foreach (var branchDM in branches)
                {
                    var productBranchDM = new SM_Product_Branches();
                    productBranchDM.Branch_Id = branchDM.Branch_Id;
                    productBranchDM.Branch_Name = branchDM.Branch_Name_E;
                    var isBranchExist = query.Where(x => x.Branch_Id == branchDM.Branch_Id).FirstOrDefault();
                    if (isBranchExist != null)
                    {
                        productBranchDM.Product_Branch_Id = isBranchExist.Product_Branch_Id;
                        productBranchDM.Is_Available = isBranchExist.Is_Available;
                    }
                    else
                    {
                        productBranchDM.Is_Available = true; // if not define explicitly we are assuming it is available
                    }
                    customer.Add(productBranchDM);
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }

        #endregion

        public SM_Product_Catering_Products CreateProductCategoryProduct(SM_Product_Catering_Products invoiceDM)
        {
            try
            {
                var query = (from o in context.sm_product_catering_products
                             where o.Catering_Product_Id == invoiceDM.Catering_Product_Id
                             select o).FirstOrDefault();

                if (query != null)
                {
                    query.Publish = invoiceDM.Publish;
                }
                else
                {
                    context.sm_product_catering_products.Add(invoiceDM);
                }
                context.SaveChanges();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return invoiceDM;
        }
        public List<CateringCategoryDTO> GetProductCateringProducts(long product_id, string lang = "E")
        {
            AddOnBC addOnBC = new AddOnBC(context);
            var customer = new List<CateringCategoryDTO>();
            try
            {
                var query = (from o in context.sm_product_catering_products
                             where o.Product_Id == product_id && !o.Is_Deleted && o.Publish
                             select o.Category_Id).Distinct().ToList();

                foreach (var category_id in query)
                {
                    var addOnDTO = new CateringCategoryDTO();
                    var addOnDM = GetCateringCategory(category_id);
                    if (addOnDM != null)
                    {
                        addOnDTO.Category_Id = addOnDM.Category_Id;
                        addOnDTO.Category_Name = lang.ToUpper() == "E" ? addOnDM.Category_Name_E : addOnDM.Category_Name_A ?? "";
                        addOnDTO.Qty_To_Choose = addOnDM.Qty;
                        var options = (from o in context.sm_product_catering_products
                                       from p in context.sm_products
                                       where o.Product_Id == product_id && !o.Is_Deleted && o.Publish
                                       && o.Category_Id == addOnDM.Category_Id && o.Child_Product_Id == p.Product_Id
                                       select new CateringOptionDTO
                                       {
                                           Cateroing_Product_Id = o.Catering_Product_Id,
                                           Option_Name = lang.ToUpper() == "E" ? p.Product_Name_E : p.Product_Name_A ?? "",

                                       }).ToList();
                        addOnDTO.Options.AddRange(options);
                        customer.Add(addOnDTO);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }

        public SM_Catering_Categories? GetCateringCategory(long category_id)
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

        public SM_Product_Catering_Products? GetProductCateringProduct(long catering_product_id)
        {
            var area = new SM_Product_Catering_Products();
            try
            {

                area = (from o in context.sm_product_catering_products
                        where o.Catering_Product_Id == catering_product_id && !o.Is_Deleted
                        select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return area;
        }

        public bool DeleteProductCateringProduct(SM_Product_Catering_Products docDM)
        {
            try
            {
                var detail = (from o in context.sm_product_catering_products
                              where o.Catering_Product_Id == docDM.Catering_Product_Id
                              select o).FirstOrDefault();
                if (detail != null)
                {
                    detail.Is_Deleted = true;
                    detail.Deleted_By = docDM.Deleted_By;
                    detail.Deleted_Datetime = docDM.Deleted_Datetime;
                    context.SaveChanges();
                }

            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return true;
        }

        public List<SM_Product_Catering_Products> GetAllProductCateringProducts(long product_id)
        {

            var customer = new List<SM_Product_Catering_Products>();
            try
            {
                var query = (from o in context.sm_product_catering_products
                             from c in context.sm_catering_categories
                             from p in context.sm_products
                             where o.Product_Id == product_id && !o.Is_Deleted
                             && o.Category_Id == c.Category_Id && o.Child_Product_Id == p.Product_Id
                             select new { o, c, p }).ToList();

                foreach (var detail in query)
                {
                    var detailDM = new SM_Product_Catering_Products();
                    detailDM = detail.o;
                    detailDM.Category_Name = detail.c.Category_Name_E + "(" + detail.c.Qty + ")";
                    detailDM.Product_Name = detail.p.Product_Name_E;
                    customer.Add(detailDM);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }
    }
}
