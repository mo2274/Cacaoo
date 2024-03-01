namespace ChocolateDelivery.BLL;

public static class Helpers
{
    public static void WriteToFile(string p_str_path, string p_str_data, bool p_bln_append = true)
    {
        if (!File.Exists(p_str_path))
        {
            var dinfo = Directory.CreateDirectory(p_str_path);
            dinfo.Create();
        }
        if (!File.Exists(p_str_path + FormatDate(DateTime.Today.Date) + ".txt"))
        {
            var fi = new FileInfo(p_str_path + FormatDate(DateTime.Today.Date) + ".txt");
            var fstr = fi.Create();
            fstr.Close();
        }
        var obj = new StreamWriter(p_str_path + FormatDate(DateTime.Today.Date) + ".txt", p_bln_append);
        obj.WriteLine("=========================================================================");
        obj.WriteLine("Date Time : " + DateTime.Now.ToString());
        obj.WriteLine("-------------------------------------------------------------------------");
        obj.Write(p_str_data + "\n");
        obj.WriteLine("=========================================================================");
        obj.Close();
    }

    private static string FormatDate(DateTime Date)
    {
        //declare as constant.
        var format = "dd-MMM-yyyy";
        return Convert.ToDateTime(Date).ToString(format);
    }
}