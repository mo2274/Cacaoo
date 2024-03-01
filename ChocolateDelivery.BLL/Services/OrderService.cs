using ChocolateDelivery.DAL;


namespace ChocolateDelivery.BLL;

public class OrderService
{
	private readonly AppDbContext _context;

	public OrderService(AppDbContext appDbContext)
	{
		_context = appDbContext;
	}
	public TXN_Orders SaveOrder(TXN_Orders CustomerDM)
	{
		try
		{
			if (CustomerDM.Order_Id != 0)
			{
				var Customer = (from o in _context.txn_orders
					where o.Order_Id == CustomerDM.Order_Id
					select o).FirstOrDefault();

				if (Customer != null)
				{
					Customer.Branch_Id = CustomerDM.Branch_Id;
					Customer.Status_Id = CustomerDM.Status_Id;
					if (CustomerDM.Driver_Id != null)
						Customer.Driver_Id = CustomerDM.Driver_Id;
					if (CustomerDM.Delivery_Image != null)
						Customer.Delivery_Image = CustomerDM.Delivery_Image;
					if (CustomerDM.Pickup_Image != null)
						Customer.Pickup_Image = CustomerDM.Pickup_Image;
					if (CustomerDM.Cancelled_Reason != null)
						Customer.Cancelled_Reason = CustomerDM.Cancelled_Reason;
					_context.SaveChanges();
				}
			}
			else
			{
				_context.txn_orders.Add(CustomerDM);
				_context.SaveChanges();
			}
		}

		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}


		return CustomerDM;
	}

	public TXN_Order_Details SaveOrderDetail(TXN_Order_Details CustomerDM)
	{
		try
		{
			if (CustomerDM.Order_Detail_Id != 0)
			{
				var Customer = (from o in _context.txn_order_details
					where o.Order_Detail_Id == CustomerDM.Order_Detail_Id
					select o).FirstOrDefault();

				if (Customer != null)
				{
					Customer.Rating = CustomerDM.Rating;
					_context.SaveChanges();
				}
			}
			else
			{
				_context.txn_order_details.Add(CustomerDM);
				_context.SaveChanges();
			}
		}

		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}


		return CustomerDM;
	}

	public TXN_Order_Detail_AddOns SaveOrderDetailAddon(TXN_Order_Detail_AddOns CustomerDM)
	{
		try
		{
			if (CustomerDM.Detail_AddOnId != 0)
			{
				var Customer = (from o in _context.txn_order_detail_addons
					where o.Detail_AddOnId == CustomerDM.Detail_AddOnId
					select o).FirstOrDefault();

				if (Customer != null)
				{

					_context.SaveChanges();
				}
			}
			else
			{
				_context.txn_order_detail_addons.Add(CustomerDM);
				_context.SaveChanges();
			}
		}

		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}


		return CustomerDM;
	}

	public TXN_Order_Detail_Catering_Products SaveOrderDetailCateringProduct(TXN_Order_Detail_Catering_Products CustomerDM)
	{
		try
		{
			if (CustomerDM.Detail_Categoring_Product_Id != 0)
			{
				var Customer = (from o in _context.txn_order_detail_catering_products
					where o.Detail_Categoring_Product_Id == CustomerDM.Detail_Categoring_Product_Id
					select o).FirstOrDefault();

				if (Customer != null)
				{

					_context.SaveChanges();
				}
			}
			else
			{
				_context.txn_order_detail_catering_products.Add(CustomerDM);
				_context.SaveChanges();
			}
		}

		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}


		return CustomerDM;
	}

	public TXN_Orders? GetOrder(long order_id, string lang = "E")
	{
		var appUserService = new AppUserService(_context);
		var area = new TXN_Orders();
		try
		{


			var query = (from o in _context.txn_orders
				from s in _context.sm_order_status
				from p in _context.sm_payment_types
				join u in _context.sm_restaurant_branches on o.Branch_Id equals u.Branch_Id into branch
				from u in branch.DefaultIfEmpty()
				join d in _context.app_users on o.Driver_Id equals d.App_User_Id into driver
				from d in driver.DefaultIfEmpty()
				join r in _context.sm_restaurants on o.Restaurant_Id equals r.Restaurant_Id into restaurant
				from r in restaurant.DefaultIfEmpty()
				where o.Order_Id == order_id && o.Status_Id == s.Status_Id
				                             && o.Payment_Type_Id == p.Payment_Type_Id
				select new { o, s, p, u, d, r }).FirstOrDefault();

			if (query != null)
			{
				area = query.o;
				area.Status_Name = query.s.Status_Name_E;
				area.Payment_Type_Name = query.p.Payment_Type_Name_E;
				area.Delivery_Type_Name = query.o.Delivery_Type == Delivery_Types.PICKUP ? "Pickup" : "Delivery";
				area.Order_Type_Name = query.o.Order_Type == OrderTypes.NORMAL ? "Normal" : "Gift";
				if (query.u != null)
				{
					area.Branch_Address = lang == "A" ? query.u.Address_A ?? query.u.Address_E : query.u.Address_E ?? "";
					area.Branch_Latitude = query.u.Latitude ?? 0;
					area.Branch_Longitude = query.u.Longitude ?? 0;
				}

				if (query.d != null)
					area.Driver_Name = query.d.Name;
				var address = "";
				if (area.Address_Id != null)
				{
					var useraddressDM = appUserService.GetUserAddress((long)area.Address_Id);
					if (useraddressDM != null)
					{
						address = "Area:" + useraddressDM.Area_Name + " ,Block:" + useraddressDM.Block + " ,Street:" + useraddressDM.Street;
						if (!string.IsNullOrEmpty(useraddressDM.House_No))
							address += " ,House No.:" + useraddressDM.House_No;
						if (!string.IsNullOrEmpty(useraddressDM.Building))
							address += " ,Building:" + useraddressDM.Building;
						if (!string.IsNullOrEmpty(useraddressDM.Floor))
							address += " ,Floor:" + useraddressDM.Floor;
						if (!string.IsNullOrEmpty(useraddressDM.Apartment))
							address += " ,Apartment:" + useraddressDM.Apartment;
						if (!string.IsNullOrEmpty(useraddressDM.Avenue))
							address += " ,Avenue:" + useraddressDM.Avenue;
						if (!string.IsNullOrEmpty(useraddressDM.Extra_Direction))
							address += " ,Extra Direction:" + useraddressDM.Extra_Direction;
						area.User_Address_Latitude = useraddressDM.Latitude;
						area.User_Address_Longitude = useraddressDM.Longitude;
					}
				}
				if (query.r != null)
				{

					area.Restaurant_Name = lang == "A" ? query.r.Restaurant_Name_A ?? query.r.Restaurant_Name_E : query.r.Restaurant_Name_E;
					area.Restaurant_Mobile = query.r.Mobile;
				}

				area.Full_Address = address;
				area.TXN_Order_Details = GetOrderDetails(query.o.Order_Id);
			}
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return area;
	}

	public TXN_Order_Details? GetOrderDetail(long order_detail_id)
	{
		var area = new TXN_Order_Details();
		try
		{


			area = (from o in _context.txn_order_details
				where o.Order_Detail_Id == order_detail_id
				select o).FirstOrDefault();
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return area;
	}

	public TXN_Orders? GetOrderByOrderDetail(long order_detail_id)
	{
		var area = new TXN_Orders();
		try
		{
			area = (from od in _context.txn_order_details
				from o in _context.txn_orders
				where od.Order_Detail_Id == order_detail_id && od.Order_Id == o.Order_Id
				select o).FirstOrDefault();
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return area;
	}

	public List<TXN_Order_Details> GetOrderDetails(long order_id)
	{

		var query = new List<TXN_Order_Details>();
		try
		{
			query = (from o in _context.txn_order_details
				where o.Order_Id == order_id
				select o).ToList();
			foreach (var detail in query)
			{
				detail.AddOn_Amount = detail.Gross_Amount - detail.Amount;
				detail.Full_Product_Name = detail.Product_Name;
				detail.TXN_Order_Detail_AddOns = GetOrderDetailAddOns(detail.Order_Detail_Id);
				if (detail.TXN_Order_Detail_AddOns.Count > 0)
				{
					var addons = string.Join(',', detail.TXN_Order_Detail_AddOns.Select(x => x.AddOn_Name));
					detail.Full_Product_Name += "\r\n( AddOns : " + addons + " )";
				}
			}


		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}

	public List<TXN_Order_Detail_AddOns> GetOrderDetailAddOns(long order_detail_id)
	{

		var query = new List<TXN_Order_Detail_AddOns>();
		try
		{
			var addons = (from o in _context.txn_order_detail_addons
				from p in _context.sm_product_addons
				where o.Order_Detail_Id == order_detail_id && o.Product_AddOnId == p.Product_AddOnId
				select new { o, p }).ToList();
			foreach (var addon in addons)
			{
				var detailAddons = new TXN_Order_Detail_AddOns();
				detailAddons = addon.o;
				detailAddons.AddOn_Name = addon.p.Line_AddOn_Name_E;
				query.Add(detailAddons);
			}


		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}

	public List<TXN_Order_Detail_Catering_Products> GetOrderDetailCategoryProducts(long order_detail_id,string lang ="E")
	{

		var query = new List<TXN_Order_Detail_Catering_Products>();
		try
		{
			var addons = (from o in _context.txn_order_detail_catering_products
				from p in _context.sm_product_catering_products
				from c in _context.sm_products
				where o.Detail_Id == order_detail_id && o.Category_Product_Id == p.Catering_Product_Id
				                                     && p.Child_Product_Id == c.Product_Id
				select new { o, c }).ToList();
			foreach (var addon in addons)
			{
				var detailAddons = new TXN_Order_Detail_Catering_Products();
				detailAddons = addon.o;
				detailAddons.Product_Name = lang == "A" ? addon.c.Product_Name_A ?? addon.c.Product_Name_E : addon.c.Product_Name_E;
				query.Add(detailAddons);
			}


		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}

	public long GetNextOrderNo()
	{
		long invoice_no = 1;
		try
		{
			var max_no = (from o in _context.txn_orders
				orderby o.Order_No descending
				select o).FirstOrDefault();
			if (max_no != null)
			{
				invoice_no = max_no.Order_No + 1;
			}
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return invoice_no;
	}

	public TXN_Order_Logs CreateOrderLog(TXN_Order_Logs CustomerDM)
	{
		try
		{
			if (CustomerDM.Log_Id != 0)
			{
				var Customer = (from o in _context.txn_order_logs
					where o.Log_Id == CustomerDM.Log_Id
					select o).FirstOrDefault();

				if (Customer != null)
				{
					_context.SaveChanges();
				}
			}
			else
			{
				_context.txn_order_logs.Add(CustomerDM);
				_context.SaveChanges();
			}
		}

		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}


		return CustomerDM;
	}

	public Site_Configuration? GetSiteConfiguration(string config_name)
	{
		var site_config = new Site_Configuration();
		try
		{
			site_config = (from o in _context.site_configuration
				where o.Config_Name == config_name
				select o).FirstOrDefault();
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return site_config;
	}

	public List<SM_Payment_Types> GetPaymentTypes()
	{
		var userMenu = new List<SM_Payment_Types>();
		try
		{
			userMenu = (from o in _context.sm_payment_types
				where o.Show
				select o).ToList();

		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return userMenu;
	}


	#region Tap payments
	public PAYMENTS CreatePayment(PAYMENTS paymentDM)
	{
		try
		{
			if (paymentDM.Id != 0)
			{
				var Promoter = (from o in _context.payments
					where o.Id == paymentDM.Id
					select o).FirstOrDefault();

				if (Promoter != null)
				{
					Promoter.Payment_Id = paymentDM.Payment_Id;
					Promoter.Trans_Id = paymentDM.Trans_Id;
					Promoter.Auth = paymentDM.Auth;
					Promoter.Result = paymentDM.Result;
					Promoter.Reference_No = paymentDM.Reference_No;
					Promoter.Payment_Mode = paymentDM.Payment_Mode;
					Promoter.Updated_Datetime = paymentDM.Updated_Datetime;

					_context.SaveChanges();
				}
			}
			else
			{
				_context.payments.Add(paymentDM);
				_context.SaveChanges();
			}
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}


		return paymentDM;
	}

	public PAYMENTS? GetPaymentByTrackId(string track_id)
	{
		var post = new PAYMENTS();
		try
		{

			post = (from o in _context.payments
				where o.Track_Id == track_id
				orderby o.Created_Datetime descending
				select o).FirstOrDefault();

		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return post;
	}

	public PAYMENTS? GetPaymentByOrderId(long order_id)
	{
		var post = new PAYMENTS();
		try
		{

			post = (from o in _context.payments
				where o.Order_Id == order_id
				orderby o.Created_Datetime descending
				select o).FirstOrDefault();

		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return post;
	}
	#endregion

	#region Driver

	public List<TXN_Orders> GetPendingDriverOrders(string lang, long driver_id)
	{
		var appUserService = new AppUserService(_context);
		var query = new List<TXN_Orders>();
		try
		{
			var orders = (from o in _context.txn_orders
				join u in _context.sm_restaurant_branches on o.Branch_Id equals u.Branch_Id into branch
				from u in branch.DefaultIfEmpty()
				join l in _context.txn_order_logs on o.Order_Id equals l.Order_Id into log
				from l in log.Where(f => f.Driver_Id == driver_id).DefaultIfEmpty()
				where (o.Status_Id == OrderStatus.ORDER_PREPARING || o.Status_Id == OrderStatus.ORDER_PAID)
				      && o.Delivery_Type == Delivery_Types.DELIVERY
				select new { o, u, l }).ToList();
			foreach (var order in orders)
			{
				if (order.l == null)
				{
					var orderDM = order.o;
					if (order.u != null)
						orderDM.Branch_Address = lang == "A" ? order.u.Address_A ?? order.u.Address_E ?? "" : order.u.Address_E ?? "";
					var address = "";
					if (orderDM.Address_Id != null)
					{
						var useraddressDM = appUserService.GetUserAddress((long)orderDM.Address_Id);
						if (useraddressDM != null)
						{
							address = "Area:" + useraddressDM.Area_Name + " ,Block:" + useraddressDM.Block + " ,Street:" + useraddressDM.Street;
							if (!string.IsNullOrEmpty(useraddressDM.House_No))
								address += " ,House No.:" + useraddressDM.House_No;
							if (!string.IsNullOrEmpty(useraddressDM.Building))
								address += " ,Building:" + useraddressDM.Building;
							if (!string.IsNullOrEmpty(useraddressDM.Floor))
								address += " ,Floor:" + useraddressDM.Floor;
							if (!string.IsNullOrEmpty(useraddressDM.Apartment))
								address += " ,Apartment:" + useraddressDM.Apartment;
							if (!string.IsNullOrEmpty(useraddressDM.Avenue))
								address += " ,Avenue:" + useraddressDM.Avenue;
							if (!string.IsNullOrEmpty(useraddressDM.Extra_Direction))
								address += " ,Extra Direction:" + useraddressDM.Extra_Direction;

						}
					}
					orderDM.Full_Address = address;
					query.Add(orderDM);
				}

			}


		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}

	public TXN_Orders? GetExistingDriverOrder(long driver_id)
	{
		var area = new TXN_Orders();
		try
		{


			area = (from o in _context.txn_orders
				where o.Driver_Id == driver_id
				      && (o.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER || o.Status_Id == OrderStatus.OUT_FOR_DELIVERY)
				select o).FirstOrDefault();
			if (area != null)
			{
				var trackingDM = (from o in _context.txn_order_tracking_details
					orderby o.Track_Datetime descending
					where o.Order_Id == area.Order_Id
					select o).FirstOrDefault();
				if (trackingDM != null)
				{
					area.Driver_Latitude = trackingDM.Latitude;
					area.Driver_Longitude = trackingDM.Longitude;
				}
			}
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return area;
	}

	public List<TXN_Orders> GetDeliveredDriverOrders(string lang, long driver_id)
	{
		var appUserService = new AppUserService(_context);
		var query = new List<TXN_Orders>();
		try
		{
			var orders = (from o in _context.txn_orders
				join u in _context.sm_restaurant_branches on o.Branch_Id equals u.Branch_Id into branch
				from u in branch.DefaultIfEmpty()
				from p in _context.sm_payment_types
				where o.Status_Id == OrderStatus.ORDER_DELIVERED
				      && o.Delivery_Type == Delivery_Types.DELIVERY && o.Driver_Id == driver_id && o.Payment_Type_Id == p.Payment_Type_Id
				orderby o.Order_Datetime descending
				select new { o, u, p }).ToList();
			foreach (var order in orders)
			{
				var orderDM = order.o;
				orderDM.Branch_Address = lang == "A" ? order.u.Address_A ?? order.u.Address_E ?? "" : order.u.Address_E ?? "";
				var address = "";
				if (orderDM.Address_Id != null)
				{
					var useraddressDM = appUserService.GetUserAddress((long)orderDM.Address_Id);
					if (useraddressDM != null)
					{
						address = "Area:" + useraddressDM.Area_Name + " ,Block:" + useraddressDM.Block + " ,Street:" + useraddressDM.Street;
						if (!string.IsNullOrEmpty(useraddressDM.House_No))
							address += " ,House No.:" + useraddressDM.House_No;
						if (!string.IsNullOrEmpty(useraddressDM.Building))
							address += " ,Building:" + useraddressDM.Building;
						if (!string.IsNullOrEmpty(useraddressDM.Floor))
							address += " ,Floor:" + useraddressDM.Floor;
						if (!string.IsNullOrEmpty(useraddressDM.Apartment))
							address += " ,Apartment:" + useraddressDM.Apartment;
						if (!string.IsNullOrEmpty(useraddressDM.Avenue))
							address += " ,Avenue:" + useraddressDM.Avenue;
						if (!string.IsNullOrEmpty(useraddressDM.Extra_Direction))
							address += " ,Extra Direction:" + useraddressDM.Extra_Direction;

					}
				}
				orderDM.Full_Address = address;
				orderDM.Payment_Type_Name = lang == "A" ? order.p.Payment_Type_Name_A ?? order.p.Payment_Type_Name_E ?? "" : order.p.Payment_Type_Name_E ?? "";
				query.Add(orderDM);
			}
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}

	public List<TXN_Orders> GetDeliveredDriverOrders(string lang, long driver_id, DateTime from_date, DateTime to_date)
	{
		var appUserService = new AppUserService(_context);
		var query = new List<TXN_Orders>();
		try
		{
			var orders = (from o in _context.txn_orders
				join u in _context.sm_restaurant_branches on o.Branch_Id equals u.Branch_Id into branch
				from u in branch.DefaultIfEmpty()
				from p in _context.sm_payment_types
				where o.Status_Id == OrderStatus.ORDER_DELIVERED
				      && o.Delivery_Type == Delivery_Types.DELIVERY && o.Driver_Id == driver_id && o.Payment_Type_Id == p.Payment_Type_Id
				      && o.Order_Datetime >= from_date && o.Order_Datetime <= to_date
				orderby o.Order_Datetime descending
				select new { o, u, p }).ToList();
			foreach (var order in orders)
			{
				var orderDM = order.o;
				orderDM.Branch_Address = lang == "A" ? order.u.Address_A ?? order.u.Address_E ?? "" : order.u.Address_E ?? "";
				var address = "";
				if (orderDM.Address_Id != null)
				{
					var useraddressDM = appUserService.GetUserAddress((long)orderDM.Address_Id);
					if (useraddressDM != null)
					{
						address = "Area:" + useraddressDM.Area_Name + " ,Block:" + useraddressDM.Block + " ,Street:" + useraddressDM.Street;
						if (!string.IsNullOrEmpty(useraddressDM.House_No))
							address += " ,House No.:" + useraddressDM.House_No;
						if (!string.IsNullOrEmpty(useraddressDM.Building))
							address += " ,Building:" + useraddressDM.Building;
						if (!string.IsNullOrEmpty(useraddressDM.Floor))
							address += " ,Floor:" + useraddressDM.Floor;
						if (!string.IsNullOrEmpty(useraddressDM.Apartment))
							address += " ,Apartment:" + useraddressDM.Apartment;
						if (!string.IsNullOrEmpty(useraddressDM.Avenue))
							address += " ,Avenue:" + useraddressDM.Avenue;
						if (!string.IsNullOrEmpty(useraddressDM.Extra_Direction))
							address += " ,Extra Direction:" + useraddressDM.Extra_Direction;

					}
				}
				orderDM.Full_Address = address;
				orderDM.Payment_Type_Name = lang == "A" ? order.p.Payment_Type_Name_A ?? order.p.Payment_Type_Name_E ?? "" : order.p.Payment_Type_Name_E ?? "";
				query.Add(orderDM);
			}
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}

	public List<TXN_Orders> GetCustomerOrders(string lang, long app_user_id)
	{
		var appUserService = new AppUserService(_context);
		var query = new List<TXN_Orders>();
		try
		{
			var orders = (from o in _context.txn_orders
				from s in _context.sm_order_status
				join u in _context.sm_restaurant_branches on o.Branch_Id equals u.Branch_Id into branch
				from u in branch.DefaultIfEmpty()
				where (o.Payment_Type_Id == PaymentTypes.Cash || (o.Payment_Type_Id == PaymentTypes.KNET && o.Status_Id != OrderStatus.ORDER_FAILED && o.Status_Id != OrderStatus.ORDER_PROCESSING_PAYMENT))
				      && o.App_User_Id == app_user_id && o.Status_Id == s.Status_Id
				orderby o.Order_Datetime descending
				select new { o, u, s }).ToList();
			foreach (var order in orders)
			{
				var orderDM = order.o;
				if (order.u != null)
					orderDM.Branch_Address = lang == "A" ? order.u.Address_A ?? order.u.Address_E ?? "" : order.u.Address_E ?? "";
				var address = "";
				if (orderDM.Address_Id != null)
				{
					var useraddressDM = appUserService.GetUserAddress((long)orderDM.Address_Id);
					if (useraddressDM != null)
					{
						address = "Area:" + useraddressDM.Area_Name + " ,Block:" + useraddressDM.Block + " ,Street:" + useraddressDM.Street;
						if (!string.IsNullOrEmpty(useraddressDM.House_No))
							address += " ,House No.:" + useraddressDM.House_No;
						if (!string.IsNullOrEmpty(useraddressDM.Building))
							address += " ,Building:" + useraddressDM.Building;
						if (!string.IsNullOrEmpty(useraddressDM.Floor))
							address += " ,Floor:" + useraddressDM.Floor;
						if (!string.IsNullOrEmpty(useraddressDM.Apartment))
							address += " ,Apartment:" + useraddressDM.Apartment;
						if (!string.IsNullOrEmpty(useraddressDM.Avenue))
							address += " ,Avenue:" + useraddressDM.Avenue;
						if (!string.IsNullOrEmpty(useraddressDM.Extra_Direction))
							address += " ,Extra Direction:" + useraddressDM.Extra_Direction;

					}
				}
				orderDM.Full_Address = address;
				orderDM.Status_Name = order.s.Status_Name_E;
				query.Add(orderDM);

			}


		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}

	public List<TXN_Orders> GetReportOrders(DateTime FromDate, DateTime ToDate, string lang)
	{
		var appUserService = new AppUserService(_context);
		var query = new List<TXN_Orders>();
		try
		{
			var orders = (from o in _context.txn_orders
				from s in _context.sm_order_status
				join u in _context.sm_restaurant_branches on o.Branch_Id equals u.Branch_Id into branch
				from u in branch.DefaultIfEmpty()
				where (o.Payment_Type_Id == PaymentTypes.Cash || (o.Payment_Type_Id == PaymentTypes.KNET && o.Status_Id != OrderStatus.ORDER_FAILED && o.Status_Id != OrderStatus.ORDER_PROCESSING_PAYMENT))
				      && o.Status_Id == s.Status_Id && o.Order_Datetime >= FromDate && o.Order_Datetime <= ToDate
				orderby o.Order_Datetime descending
				select new { o, u, s }).ToList();
			foreach (var order in orders)
			{
				var orderDM = order.o;
				if (order.u != null)
					orderDM.Branch_Address = lang == "A" ? order.u.Address_A ?? order.u.Address_E ?? "" : order.u.Address_E ?? "";
				var address = "";
				if (orderDM.Address_Id != null)
				{
					var useraddressDM = appUserService.GetUserAddress((long)orderDM.Address_Id);
					if (useraddressDM != null)
					{
						address = "Area:" + useraddressDM.Area_Name + " ,Block:" + useraddressDM.Block + " ,Street:" + useraddressDM.Street;
						if (!string.IsNullOrEmpty(useraddressDM.House_No))
							address += " ,House No.:" + useraddressDM.House_No;
						if (!string.IsNullOrEmpty(useraddressDM.Building))
							address += " ,Building:" + useraddressDM.Building;
						if (!string.IsNullOrEmpty(useraddressDM.Floor))
							address += " ,Floor:" + useraddressDM.Floor;
						if (!string.IsNullOrEmpty(useraddressDM.Apartment))
							address += " ,Apartment:" + useraddressDM.Apartment;
						if (!string.IsNullOrEmpty(useraddressDM.Avenue))
							address += " ,Avenue:" + useraddressDM.Avenue;
						if (!string.IsNullOrEmpty(useraddressDM.Extra_Direction))
							address += " ,Extra Direction:" + useraddressDM.Extra_Direction;

					}
				}
				orderDM.Full_Address = address;
				orderDM.Status_Name = order.s.Status_Name_E;
				query.Add(orderDM);

			}


		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}

	public List<TXN_Order_Details> GetReportOrderDetails(DateTime FromDate, DateTime ToDate, string lang)
	{
		var query = new List<TXN_Order_Details>();
		try
		{
			query = (from o in _context.txn_orders
				from od in _context.txn_order_details
				from s in _context.sm_order_status
				join u in _context.sm_restaurant_branches on o.Branch_Id equals u.Branch_Id into branch
				from u in branch.DefaultIfEmpty()
				where (o.Payment_Type_Id == PaymentTypes.Cash || (o.Payment_Type_Id == PaymentTypes.KNET && o.Status_Id != OrderStatus.ORDER_FAILED && o.Status_Id != OrderStatus.ORDER_PROCESSING_PAYMENT))
				      && o.Status_Id == s.Status_Id && o.Order_Id == od.Order_Id
				      && o.Order_Datetime >= FromDate && o.Order_Datetime <= ToDate
				orderby o.Order_Datetime descending
				select od).ToList();



		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}

	public List<DriverOrderDTO> GetDashboardOrders(int dashboard_order_type)
	{
		var currentKwtTime = StaticMethods.GetKuwaitTime();
		var kwtDate = currentKwtTime.Date;

		var query = new List<DriverOrderDTO>();
		try
		{
			var orders = (from o in _context.txn_orders
				from s in _context.sm_order_status
				from r in _context.sm_restaurants
				join u in _context.sm_restaurant_branches on o.Branch_Id equals u.Branch_Id into branch
				from u in branch.DefaultIfEmpty()
				join d in _context.app_users on o.Driver_Id equals d.App_User_Id into driver
				from d in driver.DefaultIfEmpty()
				where (o.Payment_Type_Id == PaymentTypes.Cash || (o.Payment_Type_Id == PaymentTypes.KNET && o.Status_Id != OrderStatus.ORDER_FAILED && o.Status_Id != OrderStatus.ORDER_PROCESSING_PAYMENT))
				      && o.Status_Id == s.Status_Id && o.Restaurant_Id == r.Restaurant_Id
				      && (o.Order_Datetime.Date == kwtDate /*|| (o.Pickup_Datetime != null &&  Convert.ToDateTime(o.Pickup_Datetime).Date == kwtDate)*/)
				orderby o.Order_Datetime descending
				select new { o, u, s, r, d }).ToList();

			if (dashboard_order_type == DashboardOrderTypes.PENDING)
			{
				orders = orders.Where(x => x.o.Status_Id == OrderStatus.ORDER_RECEIVED).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.PREPARING)
			{
				orders = orders.Where(x => x.o.Status_Id == OrderStatus.ORDER_PREPARING).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.OUT_FOR_DELIVERY)
			{
				orders = orders.Where(x => x.o.Status_Id == OrderStatus.OUT_FOR_DELIVERY).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.DELIVERED)
			{
				orders = orders.Where(x => x.o.Status_Id == OrderStatus.ORDER_DELIVERED).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.SCHEDULED)
			{
				orders = orders.Where(x => x.o.Pickup_Datetime != null && Convert.ToDateTime(x.o.Pickup_Datetime).Date == kwtDate).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.CANCELLED)
			{
				orders = orders.Where(x => x.o.Status_Id == OrderStatus.ORDER_CANCELLED_BY_USER).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.NOT_PICKED_30)
			{
				orders = orders.Where(x => x.o.Status_Id == OrderStatus.ACCEPTED_BY_DRIVER && x.o.Order_Datetime.AddMinutes(30) < currentKwtTime).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.OFD_30)
			{
				orders = (from io in orders
					from l in _context.txn_order_logs
					where io.o.Order_Id == l.Order_Id && l.Created_Datetime.AddMinutes(30) < currentKwtTime
					                                  && io.o.Status_Id == OrderStatus.OUT_FOR_DELIVERY && l.Status_Id == OrderStatus.OUT_FOR_DELIVERY
					select io).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.OFD_60)
			{
				orders = (from io in orders
					from l in _context.txn_order_logs
					where io.o.Order_Id == l.Order_Id && l.Created_Datetime.AddMinutes(60) < currentKwtTime
					                                  && io.o.Status_Id == OrderStatus.OUT_FOR_DELIVERY && l.Status_Id == OrderStatus.OUT_FOR_DELIVERY
					select io).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.NO_DRIVERS_10)
			{
				orders = orders.Where(x => x.o.Status_Id == OrderStatus.ORDER_RECEIVED && x.o.Order_Datetime.AddMinutes(10) < currentKwtTime).ToList();
			}
			if (dashboard_order_type == DashboardOrderTypes.DUPLICATE)
			{
				var innerQuery = (from io in orders
					from l in _context.txn_order_details
					where io.o.Order_Id == l.Order_Id

					select new { io, l }).ToList();
				var duplicates = innerQuery.GroupBy(x => new { x.io.o.App_User_Id, x.io.o.Net_Amount, x.l.Qty, x.l.Rate, Line_Amount = x.l.Net_Amount }).Select(x => x.FirstOrDefault().io.o.Order_Id).Distinct().ToList();
				orders = orders.Where(x => duplicates.Contains(x.o.Order_Id)).ToList();

			}
			if (dashboard_order_type == DashboardOrderTypes.EXPENSIVE)
			{
				orders = orders.Where(x => x.o.Gross_Amount + x.o.Delivery_Charges >= 25).ToList();
			}
			foreach (var order in orders)
			{
				var orderDM = new DriverOrderDTO
				{
					Order_Id = order.o.Order_Id,
					Order_No = order.o.Order_Serial,
					Order_Time = order.o.Order_Datetime.ToString("hh:mm tt"),
					Restaurant_Name = order.r.Restaurant_Name_E
				};
				if (order.u != null)
					orderDM.Restaurant_Branch = order.u.Branch_Name_E;
				orderDM.Order_Status = order.s.Status_Name_E;
				if (order.d != null)
					orderDM.Driver_Name = order.d.Name;
				orderDM.Order_Amount = order.o.Gross_Amount + order.o.Delivery_Charges;
				orderDM.Customer_Mobile = order.o.Mobile ?? "";
				orderDM.Customer_Note = order.o.Comments ?? "";
				orderDM.Cancelled_Reason = order.o.Cancelled_Reason;
				orderDM.Rejected_Reason = order.o.Rejected_Reason;
				orderDM.Staff_Note = order.o.Staff_Note;
				query.Add(orderDM);

			}


		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return query;
	}


	#endregion

	public TXN_Order_Tracking_Details CreateOrderTrackingDetail(TXN_Order_Tracking_Details CustomerDM)
	{
		try
		{
			if (CustomerDM.Tracking_Id != 0)
			{
				var Customer = (from o in _context.txn_order_tracking_details
					where o.Tracking_Id == CustomerDM.Tracking_Id
					select o).FirstOrDefault();

				if (Customer != null)
				{
					_context.SaveChanges();
				}
			}
			else
			{
				_context.txn_order_tracking_details.Add(CustomerDM);
				_context.SaveChanges();
			}
		}

		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}


		return CustomerDM;
	}

	public TXN_Order_Tracking_Details? GetLatestTrackingDetail(long order_id)
	{
		var site_config = new TXN_Order_Tracking_Details();
		try
		{
			site_config = (from o in _context.txn_order_tracking_details
				where o.Order_Id == order_id
				orderby o.Track_Datetime descending
				select o).FirstOrDefault();
		}
		catch (Exception ex)
		{
			throw new Exception(ex.ToString());
		}
		return site_config;
	}
}