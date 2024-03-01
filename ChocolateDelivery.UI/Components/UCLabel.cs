using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Components;

public class UCLabel : ViewComponent
{
    private AppDbContext context;
    private readonly IConfiguration _config;
    private string logPath = "";
    public UCLabel(AppDbContext cc, IConfiguration config)
    {
        context = cc;
        _config = config;
        logPath = _config.GetValue<string>("ErrorFilePath"); // "Information"
    }
    public IViewComponentResult Invoke(int labelId,string cssClass = "",string userControlType = "",string value ="",string clickFunction = "",string style = "",string id ="")
    {
        ViewBag.CssClass = cssClass;
        ViewBag.UserControlType = "Label";
        ViewBag.LabelName = "";
        ViewBag.Value = "";
        ViewBag.ClickFunction = "";
        ViewBag.Style = "";
        ViewBag.Id = "btnSubmit";
        if (!string.IsNullOrEmpty(userControlType)) {
            ViewBag.UserControlType = userControlType;
        }
        if (!string.IsNullOrEmpty(value))
        {
            ViewBag.Value = value;
        }
        if (!string.IsNullOrEmpty(clickFunction))
        {
            ViewBag.ClickFunction = clickFunction;
        }
        if (!string.IsNullOrEmpty(style))
        {
            ViewBag.Style = style;
        }
        if (!string.IsNullOrEmpty(id))
        {
            ViewBag.Id = id;
        }
        var commonBC = new CommonService(context, logPath);
        var lang = HttpContext.Session.GetString("Culture") ?? Language.English;
        if (lang == Language.Arabic) {
            //adding ar-labels class defined in site.css for arabic labels to customise fonts
            ViewBag.CssClass = cssClass + " ar-labels";
        }
        var appLabels = SessionHelper.GetObjectFromJson<List<SM_LABELS>>(HttpContext.Session, "AppLabels");
        if (appLabels != null)
        {
            var labelDM = appLabels.Where(x => x.Label_Id == labelId).FirstOrDefault();
            if (labelDM != null)
            {
                ViewBag.LabelName = lang == Language.Arabic ? labelDM.A_Label_Name : labelDM.L_Label_Name;
                  
            }
            else {
                labelDM = commonBC.GetLabel(labelId);
                if (labelDM != null)
                {
                    ViewBag.LabelName = lang == Language.Arabic ? labelDM.A_Label_Name : labelDM.L_Label_Name;
                       
                }
            }
        }
        else
        {
            var labels = commonBC.GetAllLabels();
            SessionHelper.SetObjectAsJson(HttpContext.Session, "AppLabels", labels);
            var labelDM = labels.Where(x => x.Label_Id == labelId).FirstOrDefault();
            if (labelDM != null)
            {
                ViewBag.LabelName = lang == Language.Arabic ? labelDM.A_Label_Name : labelDM.L_Label_Name;
                  
            }
        }
        return View();
    }
}