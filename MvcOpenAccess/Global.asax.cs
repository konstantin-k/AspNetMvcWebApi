using System;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace MvcOpenAccess
{
	public class WebApiApplication : HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{
			HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
			if (authCookie == null || authCookie.Value == string.Empty)
			{
				return;
			}

			FormsAuthenticationTicket authTicket;
			try
			{
				authTicket = FormsAuthentication.Decrypt(authCookie.Value);
			}
			catch
			{
				return;
			}

			// retrieve roles from UserData
			var roles = authTicket.UserData.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

			var identity = new FormsIdentity(authTicket);
			Context.User = new GenericPrincipal(identity, roles);
		}
	}
}