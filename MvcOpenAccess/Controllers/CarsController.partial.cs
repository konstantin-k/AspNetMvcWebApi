using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MvcOpenAccess.Controllers
{
	[Authorize(Roles = "Admin")]
	public partial class CarsController
    {
		public virtual IEnumerable<Car> GetByFilter(string model)
		{
			var filter = string.IsNullOrWhiteSpace(model) ? "all" : model;
			return filter == "all" ? this.repository.GetAll() : this.repository.GetAll().Where(c => c.Model == filter);
		}
    }
}
