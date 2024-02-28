using ChocolateDelivery.DAL;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class CommonBC
    {
        private ChocolateDeliveryEntities context;
        private string logPath;

        public CommonBC(ChocolateDeliveryEntities benayaatEntities, string errorLogPath)
        {
            context = benayaatEntities;
            logPath = errorLogPath;

        }

        public List<SM_LABELS> GetAllLabels()
        {

            var customer = new List<SM_LABELS>();
            try
            {
                customer = (from o in context.sm_labels
                            select o).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }
        public List<SM_LABELS> GetAppLabels()
        {

            var customer = new List<SM_LABELS>();
            try
            {
                customer = (from o in context.sm_labels
                            where o.Is_App
                            select o).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return customer;
        }
        public SM_LABELS? GetLabel(int label_id)
        {
            var label = new SM_LABELS();
            try
            {
                label = (from o in context.sm_labels
                         where o.Label_Id == label_id
                         select o).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return label;
        }

        //public APP_CMS? GetCMSContent(string pageName)
        //{

        //    var cmsPage = new APP_CMS();
        //    try
        //    {
        //        cmsPage = (from o in context.APP_CMS
        //                   where o.PAGE_NAME.ToLower() == pageName.ToLower()
        //                   select o).FirstOrDefault();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //    return cmsPage;
        //}

        public DataTable GetDataTable(long report_id, string stmt, List<string> paramterValues, string connectionString)
        {
            var listBC = new ListBC(context, logPath);
            DataTable dt = new DataTable();
            try
            {

                var sqlstmt = stmt;
                var findDTO = listBC.GetList(report_id);
                var findDetails = listBC.GetListFields(report_id);
                if (findDetails != null)
                    findDetails = findDetails.OrderBy(x => x.Sequence).ToList();


                //string connectionString = _config.GetValue<string>("ConnectionStrings:DefaultConnection");

                if (findDTO != null && findDetails != null)
                {
                    var isGroupBy = findDTO.Contain_Group_By_Clause ?? false;
                    var groupbyclause = new List<string>();
                    int i = 0;
                    sqlstmt += " Select ";
                    foreach (var findDetail in findDetails)
                    {
                        var field_name = (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name;
                        if (i == findDetails.Count - 1)
                        {
                            if (!isGroupBy || (findDetail.Field_Group_By_Type == null || findDetail.Field_Group_By_Type == List_Fields_Group_Types.Normal_Field))
                            {
                                if (findDetail.Field_Type == List_Fields_Types.ZDatetime && !string.IsNullOrEmpty(findDetail.Format_String))
                                {
                                    sqlstmt += "DATE_FORMAT(" + field_name + ",'" + findDetail.Format_String + "')" + " as " + "'" + findDetail.Field_Name_E + "'";
                                }
                                else
                                {
                                    sqlstmt += field_name + " as " + "'" + findDetail.Field_Name_E + "'";
                                }


                                if (!string.IsNullOrEmpty(findDetail.Group_By_Field_Name))
                                    groupbyclause.Add((string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Group_By_Field_Name);
                            }
                            else if (findDetail.Field_Group_By_Type == List_Fields_Group_Types.Sum_Field)
                            {
                                sqlstmt += "Sum(" + (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + ")" + " as " + "'" + findDetail.Field_Name_E + "'";
                            }
                            else if (findDetail.Field_Group_By_Type == List_Fields_Group_Types.Max_Field)
                            {
                                sqlstmt += "Max(" + (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + ")" + " as " + "'" + findDetail.Field_Name_E + "'";
                            }

                        }
                        else
                        {

                            if (!isGroupBy || (findDetail.Field_Group_By_Type == null || findDetail.Field_Group_By_Type == List_Fields_Group_Types.Normal_Field))
                            {
                                if (findDetail.Field_Type == List_Fields_Types.ZDatetime && !string.IsNullOrEmpty(findDetail.Format_String))
                                {
                                    sqlstmt += "DATE_FORMAT(" + field_name + ",'" + findDetail.Format_String + "')" + " as " + "'" + findDetail.Field_Name_E + "'" + ",";
                                }
                                else
                                {
                                    sqlstmt += field_name + " as " + "'" + findDetail.Field_Name_E + "'" + ",";
                                }

                                if (!string.IsNullOrEmpty(findDetail.Group_By_Field_Name))
                                    groupbyclause.Add(findDetail.Table_Id + "." + findDetail.Group_By_Field_Name);
                            }
                            else if (findDetail.Field_Group_By_Type == List_Fields_Group_Types.Sum_Field)
                            {
                                sqlstmt += "Sum(" + (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + ")" + " as " + "'" + findDetail.Field_Name_E + "'" + ",";
                            }
                            else if (findDetail.Field_Group_By_Type == List_Fields_Group_Types.Max_Field)
                            {
                                sqlstmt += "Max(" + (string.IsNullOrEmpty(findDetail.Table_Id) ? "" : findDetail.Table_Id + ".") + findDetail.Table_Field_Name + ")" + " as " + "'" + findDetail.Field_Name_E + "'" + ",";
                            }

                        }

                        i++;
                    }


                    sqlstmt += " from " + findDTO.From_Clause;
                    if (!string.IsNullOrEmpty(findDTO.Where_Clause))
                    {
                        sqlstmt += " where " + findDTO.Where_Clause;
                    }
                    if (isGroupBy)
                    {
                        sqlstmt += " group by " + string.Join(",", groupbyclause);
                    }
                    if (!string.IsNullOrEmpty(findDTO.Order_By_Clause))
                    {
                        sqlstmt += " order by " + findDTO.Order_By_Clause;
                    }

                    if (findDTO.Write_SQL_Log == true)
                    {
                        globalCls.WriteToFile(logPath, sqlstmt, true);

                    }
                    if (findDTO.Is_StoredProcedure == true)
                    {
                        DataTable tempDatatable = new DataTable();
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

                                    var findParameters = listBC.GetListParameters(report_id);
                                    for (int j = 0; j < findParameters.Count; j++)
                                    {
                                        cmd.Parameters.Add(findParameters[j].Parameter_Name, GetSqlDbType(findParameters[j].Parameter_DB_Type)).Value = paramterValues[j];
                                    }
                                    da.Fill(tempDatatable);
                                    //da.Fill(dt);
                                }


                                //con.Open();
                                //cmd.ExecuteNonQuery();
                            }
                        }
                        return tempDatatable;

                    }
                    else
                    {
                        using (MySqlConnection con = new MySqlConnection(connectionString))
                        {
                            using (MySqlCommand cmd = new MySqlCommand(sqlstmt))
                            {
                                cmd.Connection = con;
                                using (MySqlDataAdapter sda = new MySqlDataAdapter(cmd))
                                {
                                    sda.Fill(dt);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }
            return dt;
        }

        public MySqlDbType GetSqlDbType(string par_type)
        {
            var sqldbType = MySqlDbType.VarChar;
            try
            {
                if (par_type.ToLower().Equals(SP_DB_Types.Int))
                {
                    sqldbType = MySqlDbType.Int32;
                }
                else if (par_type.ToLower().Equals(SP_DB_Types.SmallInt))
                {
                    sqldbType = MySqlDbType.Int16;
                }
                else if (par_type.ToLower().Equals(SP_DB_Types.BigInt))
                {
                    sqldbType = MySqlDbType.Int64;
                }
                else if (par_type.ToLower().Equals(SP_DB_Types.Decimal))
                {
                    sqldbType = MySqlDbType.Decimal;
                }
                else if (par_type.ToLower().Equals(SP_DB_Types.Datetime))
                {
                    sqldbType = MySqlDbType.DateTime;
                }
                else if (par_type.ToLower().Equals(SP_DB_Types.Varchar))
                {
                    sqldbType = MySqlDbType.VarChar;
                }
            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }

            return sqldbType;
        }

        //public bool SendHttpFCCSMS(string mobileNo, string msgContent, string lang, string smsUrl,string ccNumbers = "",bool sendSMSToExtraNumbers = true)
        //{
        //    var smssent = false;
        //    try
        //    {                            
        //        mobileNo = mobileNo.Replace("+", "");
        //        if (mobileNo.Length == 8)
        //        {
        //            mobileNo = "965" + mobileNo;
        //        }
               
                
        //        msgContent = msgContent.Replace("&", "%26");
        //        string url = string.Format(smsUrl, mobileNo, msgContent, lang);
        //        //url = Uri.EscapeUriString(url);

        //        //System.Net.WebRequest myRequest = System.Net.WebRequest.Create(url) ;
        //        //System.Net.WebResponse myResponse = myRequest.GetResponse();
        //        //var sentResult = myResponse.ToString();
        //        //globalCls.WriteToFile(logPath, "SMS Response for " + mobileNo + " : " + sentResult, true);
        //        //myResponse.Close();
        //        //myRequest.Abort();

        //        var myClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
        //        var response = myClient.GetAsync(url);
        //        var streamResponse = response.Result.Content.ToString();
        //        globalCls.WriteToFile(logPath, "SMS Response for " + mobileNo + " : " + streamResponse, true);

        //        try
        //        {
        //            if (sendSMSToExtraNumbers) {
        //                SettingBC settingBC = new SettingBC(context, logPath);
        //                var extra_mobile_numbers = settingBC.GetSettingValue<string>(SettingNames.SMS_Mobile_Numbers).Split(',').ToList();
        //                if (!string.IsNullOrEmpty(ccNumbers))
        //                {
        //                    extra_mobile_numbers.AddRange(ccNumbers.Split(','));
        //                }
        //                foreach (var mob in extra_mobile_numbers)
        //                {
        //                    url = string.Format(smsUrl, mob, msgContent, lang);
        //                    response = myClient.GetAsync(url);
        //                    streamResponse = response.Result.Content.ToString();
        //                    globalCls.WriteToFile(logPath, "SMS Response for " + mob + " : " + streamResponse, true);
        //                }
        //            }
                   
        //        }
        //        catch (Exception ex)
        //        {
        //            globalCls.WriteToFile(logPath, ex.ToString(), true);
        //        }

        //        smssent = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        smssent = false;
        //        globalCls.WriteToFile(logPath, "SMS Sending Error for " + mobileNo, true);
        //        globalCls.WriteToFile(logPath, ex.ToString(), true);

        //    }
        //    return smssent;
        //}     

        public bool SendMail(string to, string subject, string body, string cc, string bcc, string fromEmail, string password)
        {
            var emailSent = true;
            try
            {

                SmtpClient mSmtpClient = new SmtpClient();

               var message = new MailMessage();

                /*if (string.IsNullOrEmpty(fromEmail))
                    message.From = new MailAddress(_config.GetValue<string>("EmailFrom"));
                else
                    message.From = new MailAddress(fromEmail);*/
                message.From = new MailAddress(fromEmail);

                //message.From = new MailAddress(setting.GetValue("FromEmail", typeof(string)).ToString(), "GulfGroupCo");
                //message.To.Add(new MailAddress(to));
                if (to != "")
                {
                    string[] str = to.Split(',');

                    foreach (var toEmail in str)
                    {
                        message.To.Add(new MailAddress(toEmail));
                    }
                }

                if (cc != "")
                {
                    string[] str = cc.Split(',');

                    foreach (var ccEmail in str)
                    {
                        message.CC.Add(new MailAddress(ccEmail));
                    }
                }
                if (bcc != "")
                {
                    string[] str = bcc.Split(',');

                    foreach (var bccEmail in str)
                    {
                        message.Bcc.Add(new MailAddress(bccEmail));
                    }

                }

                message.Body = body;
                message.IsBodyHtml = true;
                message.Subject = subject;

                mSmtpClient.UseDefaultCredentials = false;
                mSmtpClient.Host = "mail.ChocolateDelivery-kw.com";
                mSmtpClient.Port = 26;
                mSmtpClient.EnableSsl = false;
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential();
                if (!string.IsNullOrEmpty(fromEmail))
                    credentials.UserName = fromEmail;
                else
                    credentials.UserName = "admin@innovasolution.net";

                if (!string.IsNullOrEmpty(password))
                    credentials.Password = password;
                else
                    credentials.Password = "Innova@2022";

                mSmtpClient.Credentials = credentials;


                mSmtpClient.Send(message);
                message.Dispose();
            }
            catch (Exception ex)
            {
                emailSent = false;
                //LogErrorToText("Sending Email", "SendMailMessage", ex.ToString());
                globalCls.WriteToFile(logPath, ex.ToString(), true);
                //throw ex;
            }
            return emailSent;
        }

        #region Favorite Module 
        public TXN_Favorite AddFavorite(TXN_Favorite CustomerDM)
        {
            try
            {
                if (CustomerDM.Favorite_Id != 0)
                {
                    var Customer = (from o in context.txn_favorite
                                    where o.Favorite_Id == CustomerDM.Favorite_Id
                                    select o).FirstOrDefault();

                    if (Customer != null)
                    {
                    
                        context.SaveChanges();
                    }
                }
                else
                {
                    context.txn_favorite.Add(CustomerDM);
                    context.SaveChanges();
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }


            return CustomerDM;
        }

        public TXN_Favorite? GetFavorite(long app_user_id,long prod_id)
        {

            var userMenu = new TXN_Favorite();
            try
            {
                userMenu = (from o in context.txn_favorite
                            where o.App_User_Id == app_user_id && o.Product_Id == prod_id
                            select o).FirstOrDefault();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return userMenu;
        }

        public bool RemoveFavorite(long fav_id)
        {
            var isremoved = true;
            try
            {

                var Customer = (from o in context.txn_favorite
                                where o.Favorite_Id == fav_id
                                select o).FirstOrDefault();

                if (Customer != null)
                {
                    
                    context.txn_favorite.Remove(Customer);
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

        public AppProducts GetFavoriteProducts(long app_user_id)
        {

            var appPosts = new AppProducts();
            try
            {
                
                var query = (from o in context.sm_products
                             from f in context.txn_favorite
                             join b in context.sm_brands on o.Brand_Id equals b.Brand_Id into brand
                             from b in brand.DefaultIfEmpty()
                             where o.Show && o.Publish && o.Product_Id == f.Product_Id && f.App_User_Id == app_user_id
                             orderby o.Sequence
                             select new { b, o }).ToList();

              
                var total = query.Count;
                appPosts.Total_Items = total;
                
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
        #endregion

        public T GetSettingValue<T>(string settingName)
        {
            string preferenceValue = "";

            try
            {
                var com_pref = (from o in context.app_settings
                                where o.SETTING_NAME == settingName
                                select o.SETTING_VALUE).FirstOrDefault();
                if (!string.IsNullOrEmpty(com_pref))
                {
                    preferenceValue = com_pref;
                }

            }
            catch (Exception ex)
            {
                globalCls.WriteToFile(logPath, ex.ToString(), true);
            }
            return (T)Convert.ChangeType(preferenceValue, typeof(T));
        }

        public List<APP_SETTINGS> GetSettings()
        {
            var customerVisit = new List<APP_SETTINGS>();
            try
            {
                customerVisit = (from o in context.app_settings
                                 select o).ToList();


            }
            catch (Exception)
            {
                return customerVisit;
            }
            return customerVisit;
        }
    }
}
