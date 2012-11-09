using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcOpenAccess.Controllers
{
	[Authorize]
    public class UserCarsController : Controller
    {
        // GET: /UserCars/
        public ActionResult Index()
        {
            return View();
        }
    }
}
