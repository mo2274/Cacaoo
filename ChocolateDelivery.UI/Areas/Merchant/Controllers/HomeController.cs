using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Areas.Merchant.Controllers;

[Area("Merchant")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}