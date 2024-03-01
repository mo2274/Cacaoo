using ChocolateDelivery.BLL;
using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChocolateDelivery.UI.Components
{
    public class UCDatePicker : ViewComponent
    {
        private AppDbContext context;
        private readonly IConfiguration _config;
        private string logPath = "";
        public UCDatePicker(AppDbContext cc, IConfiguration config)
        {
            context = cc;
            _config = config;
            logPath = _config.GetValue<string>("ErrorFilePath"); // "Information"
        }
        public IViewComponentResult Invoke(UCProperties properties)
        {
            try
            {
                ViewBag.ErrorMessage = "This field is Required";

                if (properties.Is_Required && properties.Error_Label_Id != null && properties.Error_Label_Id != 0)
                {
                    var commonBC = new CommonService(context, logPath);
                    var lang = HttpContext.Session.GetString("Culture") ?? Language.English;
                    if (lang == Language.Arabic)
                    {
                        //adding ar-labels class defined in site.css for arabic labels to customise fonts
                        properties.CSSClass = properties.CSSClass + " ar-texts";
                    }
                    var appLabels = SessionHelper.GetObjectFromJson<List<SM_LABELS>>(HttpContext.Session, "AppLabels");
                    if (appLabels != null)
                    {
                        var labelDM = appLabels.Where(x => x.Label_Id == properties.Error_Label_Id).FirstOrDefault();
                        if (labelDM != null)
                        {
                            ViewBag.ErrorMessage = lang == Language.Arabic ? labelDM.A_Label_Name : labelDM.L_Label_Name;

                        }
                        else
                        {
                            labelDM = commonBC.GetLabel((int)properties.Error_Label_Id);
                            if (labelDM != null)
                            {
                                ViewBag.ErrorMessage = lang == Language.Arabic ? labelDM.A_Label_Name : labelDM.L_Label_Name;

                            }
                        }
                    }
                    else
                    {
                        var labels = commonBC.GetAllLabels();
                        SessionHelper.SetObjectAsJson(HttpContext.Session, "AppLabels", labels);
                        var labelDM = labels.Where(x => x.Label_Id == properties.Error_Label_Id).FirstOrDefault();
                        if (labelDM != null)
                        {
                            ViewBag.ErrorMessage = lang == Language.Arabic ? labelDM.A_Label_Name : labelDM.L_Label_Name;

                        }
                    }

                    if (string.IsNullOrEmpty(ViewBag.ErrorMessage))
                    {
                        ViewBag.ErrorMessage = "This field is Required";
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.WriteToFile(logPath, "error in uc textbox for id:" + properties.Id, true);
                Helpers.WriteToFile(logPath, ex.ToString(), true);

            }

            return View(properties);
        }
    }
}
