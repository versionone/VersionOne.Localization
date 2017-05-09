using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using VersionOne.Localization;

namespace web
{
	public class Global : HttpApplication
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

			_localizer = new WebLocalizer(Server.MapPath("~/Strings"));
		}

		private static readonly ILocalizerResolver Noop = new NoopLocalizer();

		private static WebLocalizer _localizer;
		internal static ILocalizerResolver Localizer
		{
			get { return IsLocalizationDisabled ? Noop : _localizer; }
		}

		private static bool IsLocalizationDisabled { get { return HttpContext.Current != null && HttpContext.Current.Request["noloc"] != null; } }

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

	internal class NoopLocalizer : ILocalizerResolver
	{
		public string Resolve(string tag)
		{
			return tag;
		}

		public IDictionary<string, string> Resolve(IEnumerable<string> tags)
		{
			return tags.Distinct().ToDictionary((tag) => tag, Resolve);
		}
	}
}