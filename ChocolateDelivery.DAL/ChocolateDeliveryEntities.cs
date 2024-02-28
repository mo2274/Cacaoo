using Microsoft.EntityFrameworkCore;
using ChocolateDelivery.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChocolateDelivery.DAL
{
    public class ChocolateDeliveryEntities : DbContext
    {
        public DbSet<APP_PUSH_CAMPAIGN> app_push_campaign { get; set; }
        public DbSet<APP_SETTINGS> app_settings { get; set; }
        public DbSet<App_Users> app_users { get; set; }
        public DbSet<App_User_Address> app_user_address { get; set; }
        public DbSet<Device_Registration> device_registration { get; set; }
        public DbSet<LP_POINTS_TRANSACTION> lp_points_transaction { get; set; }
        public DbSet<NOTIFICATION_INBOX> notification_inbox { get; set; }
        public DbSet<PAYMENTS> payments { get; set; }
        public DbSet<Site_Configuration> site_configuration { get; set; }
        public DbSet<SM_Areas> sm_areas { get; set; }
        public DbSet<SM_Brands> sm_brands { get; set; }
        public DbSet<SM_Categories> sm_categories { get; set; }
        public DbSet<SM_Catering_Categories> sm_catering_categories { get; set; }
        public DbSet<SM_Carousels> sm_carousels { get; set; }
        public DbSet<SM_Chefs> sm_chefs { get; set; }
        public DbSet<SM_Chef_Products> sm_chef_products { get; set; }
        public DbSet<SM_Home_Groups> sm_home_groups { get; set; }
        public DbSet<SM_Home_Group_Details> sm_home_group_details { get; set; }
        public DbSet<SM_LABELS> sm_labels { get; set; }
        public DbSet<SM_LIST_FIELDS> sm_list_fields { get; set; }
        public DbSet<SM_LIST_PARAMETERS> sm_list_parameters { get; set; }
        public DbSet<SM_LISTS> sm_lists { get; set; }
        public DbSet<SM_Main_Categories> sm_main_categories { get; set; }
        public DbSet<SM_MENU> sm_menu { get; set; }
        public DbSet<SM_Occasions> sm_occasions { get; set; }
        public DbSet<SM_Order_Status> sm_order_status { get; set; }
        public DbSet<SM_Payment_Types> sm_payment_types { get; set; }
        public DbSet<SM_Products> sm_products { get; set; }
        public DbSet<SM_Product_AddOns> sm_product_addons { get; set; }
        public DbSet<SM_Product_Branches> sm_product_branches { get; set; }
        public DbSet<SM_Product_Catering_Products> sm_product_catering_products { get; set; }
        public DbSet<SM_Product_Occasions> sm_product_occasions { get; set; }
        public DbSet<SM_Product_Types> sm_product_types { get; set; }
        public DbSet<SM_Restaurants> sm_restaurants { get; set; }
        public DbSet<SM_Restaurant_AddOns> sm_restaurant_addons { get; set; }
        public DbSet<SM_Restaurant_Branches> sm_restaurant_branches { get; set; }
        public DbSet<SM_Sub_Categories> sm_sub_categories { get; set; }
        public DbSet<SM_USER_GROUP_RIGHTS> sm_user_group_rights { get; set; }
        public DbSet<SM_USER_GROUP_DASHBOARD> sm_user_group_dashboard { get; set; }
        public DbSet<SM_USER_GROUPS> sm_user_groups { get; set; }
        public DbSet<SM_USERS> sm_users { get; set; }
        public DbSet<TXN_Cart> txn_cart { get; set; }
        public DbSet<TXN_Cart_AddOns> txn_cart_addons { get; set; }
        public DbSet<TXN_Cart_Catering_Products> txn_cart_catering_products { get; set; }
        public DbSet<TXN_Favorite> txn_favorite { get; set; }
        public DbSet<TXN_Orders> txn_orders { get; set; }
        public DbSet<TXN_Order_Details> txn_order_details { get; set; }
        public DbSet<TXN_Order_Detail_AddOns> txn_order_detail_addons { get; set; }
        public DbSet<TXN_Order_Detail_Catering_Products> txn_order_detail_catering_products { get; set; }
        public DbSet<TXN_Order_Logs> txn_order_logs { get; set; }
        public DbSet<TXN_Order_Tracking_Details> txn_order_tracking_details { get; set; }
        public ChocolateDeliveryEntities(DbContextOptions<ChocolateDeliveryEntities> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

        }
    }
}
