using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MvcApplication1.Models;
using MvcOpenAccess;
using MvcOpenAccess.Code.Security;

namespace MvcApplication1.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		// GET: /Account/Login
		[AllowAnonymous]
		public ActionResult Login(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		// POST: /Account/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				var validUser = new SecurityContext().ValidateUser(model.UserName, model.Password);
				if (validUser)
				{
					CreateAuthCookie(model.UserName);

					if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
					{
						return Redirect(returnUrl);
					}

					return RedirectToAction("Index", "Home");
				}
			}

			return View(model);
		}

		// POST: /Account/LogOff
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			FormsAuthentication.SignOut();
			return RedirectToAction("Index", "Home");
		}

		private void CreateAuthCookie(string login)
		{
			var userRoles = new SecurityContext().GetUserRoler(login);

			var authTicket = new FormsAuthenticationTicket(
				1,
				login,
				DateTime.Now,
				DateTime.Now.AddMinutes(20),
				true,
				userRoles);

			string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

			var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
			Response.Cookies.Set(authCookie);
		}
	}
}
