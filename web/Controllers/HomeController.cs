using System.Globalization;
using System.Web.Mvc;

namespace web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string tag)
        {
            return Json(new { tag, value = Global.Localizer.Resolve(tag), locale = CultureInfo.CurrentUICulture.Name }, JsonRequestBehavior.AllowGet);
        }
    }
}
