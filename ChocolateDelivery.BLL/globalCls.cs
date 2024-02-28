using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.BLL
{
    public class globalCls
    {

        public static string LoginPage = "Login.aspx";
        public static string HomePage = "Dashboard.aspx";
        public static string DeniedPage = "AccessDenied.aspx";

        public static void WriteToFile(string p_str_path, string p_str_data, bool p_bln_append = true)
        {
            string str = p_str_path;
            if (!File.Exists(p_str_path))
            {
                DirectoryInfo dinfo = Directory.CreateDirectory(p_str_path);
                dinfo.Create();
            }
            if (!File.Exists(p_str_path + FormatDate(DateTime.Today.Date) + ".txt"))
            {
                FileInfo fi = new FileInfo(p_str_path + FormatDate(DateTime.Today.Date) + ".txt");
                FileStream fstr = fi.Create();
                fstr.Close();
            }
            StreamWriter obj = new StreamWriter(p_str_path + FormatDate(DateTime.Today.Date) + ".txt", p_bln_append);
            obj.WriteLine("=========================================================================");
            obj.WriteLine("Date Time : " + DateTime.Now.ToString());
            obj.WriteLine("-------------------------------------------------------------------------");
            obj.Write(p_str_data + "\n");
            obj.WriteLine("=========================================================================");
            obj.Close();
        }

        internal static void WriteToFile(object p, string v1, bool v2)
        {
            throw new NotImplementedException();
        }

        public static string FormatDate(DateTime Date)
        {
            try
            {//declare as constant.
                string format = "dd-MMM-yyyy";
                return Convert.ToDateTime(Date).ToString(format);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        public string returnSerializeStr<T>(T item)
        {
            var returnString = "";
            try
            {

                // Just grabbing this to get hold of the type name:
                var type = item.GetType();

                // Get the PropertyInfo object:
                var properties = type.GetProperties();
                Console.WriteLine("Finding PK for {0}", type.Name);
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(string) || property.PropertyType == typeof(int) ||
                        property.PropertyType == typeof(Int16) || property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Int64) ||
                        property.PropertyType == typeof(DateTime))
                    {
                        var value = item.GetType().GetProperty(property.Name).GetValue(item, null);

                        returnString += property.Name + ":" + value + "\n";
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return returnString;

        }

    }
}
