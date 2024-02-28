using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Data;
using System.Globalization;

namespace ChocolateDelivery.UI.Areas.Merchant.Controllers
{
    [Area("Merchant")]
    public class ListController : Controller
    {
        private ChocolateDeliveryEntities context;
        private readonly IConfiguration _config;
        private IWebHostEnvironment iwebHostEnvironment;
        private string logPath = "";
        ListBC listBC;
        CommonBC commonBC;

        public ListController(ChocolateDeliveryEntities cc, IConfiguration config, IWebHostEnvironment iwebHostEnvironment)
        {
            context = cc;
            _config = config;
            this.iwebHostEnvironment = iwebHostEnvironment;
            logPath = Path.Combine(this.iwebHostEnvironment.WebRootPath, _config.GetValue<string>("ErrorFilePath")); // "Information"
            listBC = new ListBC(context, logPath);
            commonBC = new CommonBC(context, logPath);
        }

        public IActionResult Index(long Id)
        {
            DataTable dt = new DataTable();
            try
            {
                ViewBag.Id = Id;
                ViewBag.Type = Request.Query["Type"];
                var findDTO = listBC.GetList(Id);
                var findParameters = listBC.GetListParameters(Id);
                ViewData["findParameters"] = findParameters;
                string[] parameterValues = Array.Empty<string>();
                ViewData["parameterValues"] = parameterValues;
                var findDetails = listBC.GetListFields(Id);
                if (findDTO != null)
                {
                    var restaurant_id = HttpContext.Session.GetInt32("VendorId");
                    if (restaurant_id == null) {
                        globalCls.WriteToFile(logPath, "Getting vendor id null for list :"+findDTO.List_Name_E, true);
                    }
                    ViewBag.Show_Add_New_URL = findDTO.Show_Add_New_URL;
                    ViewBag.Add_New_URL = findDTO.Add_New_URL + "?List_Id=" + Id + "&Type=" + Request.Query["Type"];
                    ViewBag.List_Name = findDTO.List_Name_E;
                    ViewBag.Update_URL = findDTO.Update_URL;
                    ViewData["ListDetails"] = findDetails;
                    string connectionString = _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                    var sqlstmt = "";
                    var parameters = new List<string>();
                    DateTime now = StaticMethods.GetKuwaitTime();
                    foreach (var par in findParameters)
                    {
                        string value = "";

                        if (par.Parameter_Type == Parameter_Types.TextBox)
                        {
                            if (par.Default_Value == Parameter_Default_Values.FirstOfMonth)
                            {
                                var fromdate = new DateTime(now.Year, now.Month, 1);
                                // DateTime fromdate = DateTime.ParseExact(value, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                                value = fromdate.ToString(Date_Formats.MySqlDatetimeFormat);
                            }
                            if (par.Default_Value == Parameter_Default_Values.LastOfMonth)
                            {
                                DateTime todate = now.Date;
                                //DateTime todate = DateTime.ParseExact(value, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                                TimeSpan time = new TimeSpan(23, 59, 00);
                                todate = ((DateTime)todate).Add(time);

                                value = todate.ToString(Date_Formats.MySqlDatetimeFormat);
                            }
                            parameters.Add(value);
                        }
                        else if (par.Parameter_Type == Parameter_Types.DropDownList || par.Parameter_Type == Parameter_Types.ListBox)
                        {
                            parameters.Add(value);
                        }
                        else if (par.Parameter_Type == Parameter_Types.Session)
                        {
                            if (par.Value_Type == List_Fields_Types.ZString)
                            {
                                value = HttpContext.Session.GetString(par.Control_Name);
                                parameters.Add(HttpContext.Session.GetString(par.Control_Name));
                            }
                            else if (par.Value_Type == List_Fields_Types.ZInt)
                            {
                                value = HttpContext.Session.GetInt32(par.Control_Name).ToString();
                                parameters.Add(HttpContext.Session.GetInt32(par.Control_Name).ToString());
                            }
                            else
                            {
                                parameters.Add(value);
                            }

                        }

                        if (par.Value_Type == Value_Types.String)
                        {

                            sqlstmt += " set " + par.Parameter_Name + "='" + value + "';";

                        }
                        else if (par.Value_Type == Value_Types.DateTime)
                        {

                            sqlstmt += " set " + par.Parameter_Name + "='" + value + "';";
                        }
                        else if (par.Value_Type == Value_Types.Number)
                        {

                            sqlstmt += " set " + par.Parameter_Name + "=" + value + ";";
                        }

                    }
                    var selectstmt = sqlstmt + "\n select ";
                    var isGroupBy = findDTO.Contain_Group_By_Clause ?? false;
                    var groupbyclause = new List<string>();
                    int i = 0;
                    foreach (var findDetail in findDetails)
                    {
                        var field_name = (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name;
                        if (i == findDetails.Count - 1)
                        {
                            if (!isGroupBy || (findDetail.Field_Group_By_Type == null || findDetail.Field_Group_By_Type == List_Fields_Group_Types.Normal_Field))
                            {
                                //selectstmt += (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + " as " + "'" + findDetail.Field_Name_E + "'";
                                if (findDetail.Field_Type == List_Fields_Types.ZDatetime && !string.IsNullOrEmpty(findDetail.Format_String))
                                {
                                    selectstmt += "DATE_FORMAT(" + field_name + ",'" + findDetail.Format_String + "')" + " as " + "'" + findDetail.Field_Name_E + "'";
                                }
                                else
                                {
                                    selectstmt += field_name + " as " + "'" + findDetail.Field_Name_E + "'";
                                }
                                if (!string.IsNullOrEmpty(findDetail.Group_By_Field_Name))
                                    groupbyclause.Add((string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Group_By_Field_Name);
                            }
                            else if (findDetail.Field_Group_By_Type == List_Fields_Group_Types.Sum_Field)
                            {
                                selectstmt += "Sum(" + (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + ")" + " as " + "'" + findDetail.Field_Name_E + "'"; ;
                            }
                            else if (findDetail.Field_Group_By_Type == List_Fields_Group_Types.Max_Field)
                            {
                                selectstmt += "Max(" + (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + ")" + " as " + "'" + findDetail.Field_Name_E + "'"; ;
                            }

                        }
                        else
                        {

                            if (!isGroupBy || (findDetail.Field_Group_By_Type == null || findDetail.Field_Group_By_Type == List_Fields_Group_Types.Normal_Field))
                            {
                                //selectstmt += (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + " as " + "'" + findDetail.Field_Name_E + "'" + ",";
                                if (findDetail.Field_Type == List_Fields_Types.ZDatetime && !string.IsNullOrEmpty(findDetail.Format_String))
                                {
                                    selectstmt += "DATE_FORMAT(" + field_name + ",'" + findDetail.Format_String + "')" + " as " + "'" + findDetail.Field_Name_E + "'" + ",";
                                }
                                else
                                {
                                    selectstmt += field_name + " as " + "'" + findDetail.Field_Name_E + "'" + ",";
                                }
                                if (!string.IsNullOrEmpty(findDetail.Group_By_Field_Name))
                                    groupbyclause.Add(findDetail.Table_Id + "." + findDetail.Group_By_Field_Name);
                            }
                            else if (findDetail.Field_Group_By_Type == List_Fields_Group_Types.Sum_Field)
                            {
                                selectstmt += "Sum(" + (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + ")" + ",";
                            }
                            else if (findDetail.Field_Group_By_Type == List_Fields_Group_Types.Max_Field)
                            {
                                selectstmt += "Max(" + (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + ")" + ",";
                            }

                        }

                        //if (dt.Columns.Contains(listBC.getDataField(findDetail, findDTO.Is_StoredProcedure)))
                        //{
                        //    if (findDetail.Field_Type == List_Fields_Types.ZDecimal)
                        //    {
                        //        dt.Columns.Add(listBC.getDataField(findDetail, findDTO.Is_StoredProcedure) + "  ", typeof(System.Decimal));
                        //    }
                        //    else
                        //    {
                        //        dt.Columns.Add(listBC.getDataField(findDetail, findDTO.Is_StoredProcedure) + "  ");
                        //    }

                        //}
                        //else
                        //{
                        //    if (findDetail.Field_Type == List_Fields_Types.ZDecimal)
                        //    {
                        //        dt.Columns.Add(listBC.getDataField(findDetail, findDTO.Is_StoredProcedure), typeof(System.Decimal));
                        //    }
                        //    else
                        //    {
                        //        dt.Columns.Add(listBC.getDataField(findDetail, findDTO.Is_StoredProcedure));
                        //    }

                        //}

                        i++;
                    }
                    selectstmt += " from " + findDTO.From_Clause;
                    if (!string.IsNullOrEmpty(findDTO.Where_Clause))
                    {
                        selectstmt += " where " + findDTO.Where_Clause;

                        if (findDTO.Contain_Entity_Id)
                        {
                            if (string.IsNullOrEmpty(findDTO.Entity_Id_Alias))
                                selectstmt += " and restaurant_id = " + restaurant_id;
                            else
                                selectstmt += " and " + findDTO.Entity_Id_Alias + ".restaurant_id = " + restaurant_id;
                        }
                    }
                    else
                    {
                        if (findDTO.Contain_Entity_Id)
                        {
                            if (string.IsNullOrEmpty(findDTO.Entity_Id_Alias))
                                selectstmt += " where restaurant_id = " + restaurant_id;
                            else
                                selectstmt += " where " + findDTO.Entity_Id_Alias + ".restaurant_id = " + restaurant_id;
                        }
                    }
                    if (isGroupBy)
                    {
                        selectstmt += " group by " + string.Join(",", groupbyclause);
                    }
                    if (!string.IsNullOrEmpty(findDTO.Order_By_Clause))
                    {
                        selectstmt += " order by " + findDTO.Order_By_Clause;
                    }

                    if (findDTO.Write_SQL_Log == true)
                    {
                        globalCls.WriteToFile(logPath, selectstmt, true);

                    }
                    if (findDTO.Is_StoredProcedure == true)
                    {

                        using (MySqlConnection con = new MySqlConnection(connectionString))
                        {
                            var time = con.ConnectionTimeout;
                            using (MySqlCommand cmd = new MySqlCommand(findDTO.StoredProcedure_Name, con))
                            {
                                if (findDTO.Command_Timeout != null)
                                {
                                    cmd.CommandTimeout = (int)findDTO.Command_Timeout;
                                }
                                using (var da = new MySqlDataAdapter(cmd))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;

                                    //var findParameters = findDTO.X_SM_REPORT_PARAMETERS.ToList();
                                    //for (int j = 0; j < findParameters.Count; j++)
                                    //{
                                    //    cmd.Parameters.Add(findParameters[j].Parameter_Name, GetSqlDbType(findParameters[j].Parameter_DB_Type)).Value = paramterValues[j];
                                    //}
                                    da.Fill(dt);
                                    //da.Fill(dt);
                                }


                                //con.Open();
                                //cmd.ExecuteNonQuery();
                            }
                        }
                        //return tempDatatable;

                    }
                    else
                    {
                        using (MySqlConnection con = new MySqlConnection(connectionString))
                        {
                            using (MySqlCommand cmd = new MySqlCommand(selectstmt))
                            {
                                cmd.Connection = con;
                                using (MySqlDataAdapter sda = new MySqlDataAdapter(cmd))
                                {
                                    sda.Fill(dt);
                                }
                            }
                        }
                        /*MySqlConnection connection = new MySqlConnection(connectionString);
                        MySqlCommand command1;
                        MySqlDataReader dataReader1;
                        connection.Open();
                        command1 = new MySqlCommand(selectstmt, connection);
                        if (findDTO.Command_Timeout != null)
                        {
                            command1.CommandTimeout = (int)findDTO.Command_Timeout;
                        }


                        dataReader1 = command1.ExecuteReader();

                        if (dataReader1.HasRows)
                        {
                            while (dataReader1.Read())
                            {
                                var row = dt.NewRow();
                                int j = 0;
                                foreach (var column in findDetails)
                                {
                                    var returnvalue = dataReader1.IsDBNull(j);
                                    //row[column.Table_Id + "_" + column.Field_Id] = column.Field_Type == "N" ?dataReader1.GetInt64(j).ToString():dataReader1.GetString(j);
                                    if (!returnvalue)
                                        row[listBC.getDataField(column, findDTO.Is_StoredProcedure)] = dataReader1.GetValue(j).ToString();
                                    else
                                    {
                                        if (column.Field_Type == List_Fields_Types.ZDecimal)
                                        {
                                            row[listBC.getDataField(column, findDTO.Is_StoredProcedure)] = 0.000;
                                        }
                                        else if (column.Field_Type == List_Fields_Types.ZInt)
                                        {
                                            row[listBC.getDataField(column, findDTO.Is_StoredProcedure)] = 0;
                                        }
                                        else
                                        {
                                            row[listBC.getDataField(column, findDTO.Is_StoredProcedure)] = "";
                                        }
                                    }
                                    j++;
                                }
                                dt.Rows.Add(row);
                            }
                        }
                        dataReader1.Close();
                        command1.Dispose();
                        connection.Close();*/
                    }

                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("name", "Due to some technical error, data cannot be fetched");
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }
            var rowsReturned = dt.Rows.Count;
            return View(dt);
        }

        // HTTP POST VERSION  
        [HttpPost]
        public IActionResult Index(string[] txtParameter, long Id)
        {
            var parameterValues = new List<string>();

            DataTable dt = new DataTable();
            try
            {
                var findid = Id;
                //DataTable dt = new DataTable();
                var sqlstmt = "";
                var findDTO = listBC.GetList(findid);
                var findDetails = listBC.GetListFields(findid);
                findDetails = findDetails.OrderBy(x => x.Sequence).ToList();
                ViewData["ListDetails"] = findDetails;
                //ViewBag.List_Name = findDTO.List_Name_E;
                if (findDTO != null)
                {
                    ViewBag.Show_Add_New_URL = findDTO.Show_Add_New_URL;
                    ViewBag.Add_New_URL = findDTO.Add_New_URL + "?List_Id=" + Id + "&Type=" + Request.Query["Type"];
                    ViewBag.List_Name = findDTO.List_Name_E;
                    ViewBag.Update_URL = findDTO.Update_URL;
                }

                var findParameters = listBC.GetListParameters(findid);
                ViewData["findParameters"] = findParameters;
                if (findParameters == null || findParameters.Count == 0)
                {

                }
                else
                {
                    var p = 0;
                    foreach (var par in findParameters)
                    {
                        string value = txtParameter[p];

                        if (par.Parameter_Type == Parameter_Types.TextBox)
                        {
                            if (par.Default_Value == Parameter_Default_Values.FirstOfMonth)
                            {
                                DateTime fromdate = DateTime.ParseExact(value, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                                value = fromdate.ToString(Date_Formats.MySqlDatetimeFormat);
                            }
                            if (par.Default_Value == Parameter_Default_Values.LastOfMonth)
                            {
                                DateTime todate = DateTime.ParseExact(value, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                                TimeSpan time = new TimeSpan(23, 59, 00);
                                todate = ((DateTime)todate).Add(time);

                                value = todate.ToString(Date_Formats.MySqlDatetimeFormat);
                            }
                            parameterValues.Add(value);
                        }
                        else if (par.Parameter_Type == Parameter_Types.DropDownList || par.Parameter_Type == Parameter_Types.ListBox)
                        {
                            parameterValues.Add(value);
                        }
                        else if (par.Parameter_Type == Parameter_Types.Session)
                        {
                            if (par.Value_Type == List_Fields_Types.ZString)
                            {
                                parameterValues.Add(HttpContext.Session.GetString(par.Control_Name));
                                value = HttpContext.Session.GetString(par.Control_Name);
                            }
                            else if (par.Value_Type == List_Fields_Types.ZInt)
                            {
                                parameterValues.Add(HttpContext.Session.GetInt32(par.Control_Name).ToString());
                                value = HttpContext.Session.GetInt32(par.Control_Name).ToString();
                            }
                            else
                            {
                                parameterValues.Add(value);
                            }
                        }

                        if (par.Value_Type == Value_Types.String)
                        {

                            sqlstmt += " set " + par.Parameter_Name + "='" + value + "';";

                        }
                        else if (par.Value_Type == Value_Types.DateTime)
                        {

                            sqlstmt += " set " + par.Parameter_Name + "='" + value + "';";
                        }
                        else if (par.Value_Type == Value_Types.Number)
                        {

                            sqlstmt += " set " + par.Parameter_Name + "=" + value + ";";
                        }
                        p++;
                    }
                }

                string connectionString = _config.GetValue<string>("ConnectionStrings:DefaultConnection");
                dt = commonBC.GetDataTable(findid, sqlstmt, parameterValues, connectionString);
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }
            ViewData["parameterValues"] = txtParameter;
            var rowsReturned = dt.Rows.Count;
            var columnsReturned = dt.Columns.Count;
            return View(dt);
        }
    }
}
