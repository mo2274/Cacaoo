using ChocolateDelivery.DAL;


namespace ChocolateDelivery.BLL;

public class SettingService
{
    private readonly AppDbContext _context;
    private string _logPath;

    public SettingService(AppDbContext benayaatEntities, string errorLogPath = "")
    {
        _context = benayaatEntities;
        _logPath = errorLogPath;
    }

    public List<APP_SETTINGS> GetSettings()
    {
        var customerVisit = new List<APP_SETTINGS>();
        try
        {
            customerVisit = (from o in _context.app_settings
                select o).ToList();


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return customerVisit;
    }

    public APP_SETTINGS? GetSetting(string Setting_Name)
    {
        var customerVisit = new APP_SETTINGS();
        try
        {
            customerVisit = (from o in _context.app_settings
                where o.SETTING_NAME == Setting_Name
                select o).FirstOrDefault();


        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return customerVisit;
    }

    public APP_SETTINGS SaveSetting(APP_SETTINGS userDM)
    {
        try
        {
            var user = (from o in _context.app_settings
                where o.SETTING_ID == userDM.SETTING_ID
                select o).FirstOrDefault();

            if (user != null)
            {
                user.SETTING_VALUE = userDM.SETTING_VALUE;
                _context.SaveChanges();
            }
            else
            {
                _context.app_settings.Add(userDM);
                _context.SaveChanges();

            }
            return userDM;

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    public T GetSettingValue<T>(string settingName)
    {
        var preferenceValue = "";

        try
        {
            var com_pref = (from o in _context.app_settings
                where o.SETTING_NAME == settingName
                select o.SETTING_VALUE).FirstOrDefault();
            if (!string.IsNullOrEmpty(com_pref))
            {
                preferenceValue = com_pref;
            }

        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
        return (T)Convert.ChangeType(preferenceValue, typeof(T));
    }
       
}