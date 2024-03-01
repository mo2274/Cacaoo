using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace ChocolateDelivery.UI.CustomFilters
{
    public class CheckSession : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                var controllerType = controllerActionDescriptor.ControllerTypeInfo;
                controllerType.GetCustomAttribute<AreaAttribute>();
                //if (areaAttribute != null && areaAttribute.RouteValue == "Admin")
                {
                    if (context.Controller is Controller)
                    {
                        var controller = (Controller)context.Controller;
                        var controllerName = controller.ControllerContext.ActionDescriptor.ControllerName;
                        var currentArea = context.ActionDescriptor.RouteValues["area"];

                        if (currentArea == "Admin")
                        {
                            var user_cd = context.HttpContext.Session.GetInt32("UserCd");
                            var excludeControllers = new List<string>(new string[] { "Login", "Knet", "KnetResponse", "KnetError", "InvoicePrint" });
                            if (user_cd == null && !excludeControllers.Any(x => x == controllerName))
                            {
                                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                {
                                    area = "Admin",
                                    controller = "Login",
                                    action = "Index",
                                    //returnurl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetEncodedUrl(context.HttpContext.Request)
                                }));
                                /*context.HttpContext.Session.SetString("UserName", "Admin");
                                context.HttpContext.Session.SetInt32("UserCd", 1);
                                context.HttpContext.Session.SetString("UserId", "Admin");
                                context.HttpContext.Session.SetString("Culture", "en-US");
                                context.HttpContext.Session.SetInt32("GroupCd", 1);
                                context.HttpContext.Session.SetString("IsSuperAdmin", "true");
                                context.HttpContext.Session.SetInt32("Entity_Id", 1);*/
                            }
                        }
                        else if(currentArea == "Merchant") {
                            var vendor_id = context.HttpContext.Session.GetInt32("VendorId");
                            var excludeControllers = new List<string>(new string[] { "Login" });
                            if (vendor_id == null && !excludeControllers.Any(x => x == controllerName))
                            {
                                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                                {
                                    area = "Merchant",
                                    controller = "Login",
                                    action = "Index",
                                    //returnurl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetEncodedUrl(context.HttpContext.Request)
                                }));
                            }
                        }
                        
                    }
                }
            }

           

        }
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
