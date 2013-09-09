using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;

namespace web
{
	public class Global : System.Web.HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{*tag}", // URL with parameters
				new { controller = "Home", action = "Index", tag = UrlParameter.Optional } // Parameter defaults
			);

		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			RegisterRoutes(RouteTable.Routes);

			Localizer = new WebLocalizer(Server.MapPath("~/Strings"));
		}

		internal static WebLocalizer Localizer;

		protected void Application_BeginRequest()
		{
			CultureInfo culture;
			try
			{
				culture = CultureInfo.CreateSpecificCulture(Request.UserLanguages[0]);
			}
			catch
			{
				culture = CultureInfo.InvariantCulture;
			}
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
		}
	}
}