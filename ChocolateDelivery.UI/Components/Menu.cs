using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Components;

public class Menu : ViewComponent
{
    private AppDbContext context;
    private readonly IConfiguration _config;
    private string logPath = "";
    public Menu(AppDbContext cc, IConfiguration config)
    {
        context = cc;
        _config = config;
        logPath = _config.GetValue<string>("ErrorFilePath"); // "Information"
    }
    public IViewComponentResult Invoke()
    {
        var userBC = new UserService(context,logPath);
        var user_cd =  HttpContext.Session.GetInt32("UserCd");
        var menuList = new List<SM_MENU>();
        if (user_cd != null) {
            menuList = userBC.GetUserMenu(Convert.ToInt32(user_cd));
        }
        return View(menuList);
    }
}