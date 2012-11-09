using System.Globalization;
using System.Linq;

namespace MvcOpenAccess.Code.Security
{
	public class SecurityContext
	{
		public bool ValidateUser(string login, string password)
		{
			using (var db = new SecurityModel())
			{
				return db.ValidateUsers(login, password);
			}
		}

		public string GetUserRoler(string login)
		{
			using (var db = new SecurityModel())
			{
				var roles = string.Join(";", db.GetUserRoles(login).Select(r => r.RoleName));
				return roles;
			}
		}

		public bool IsUserInRole(string login, string roleName)
		{
			using (var db = new SecurityModel())
			{
				var roles = db.GetUserRoles(login).Select(r => r.RoleName.ToLower(CultureInfo.InvariantCulture));

				return roles.Any(r => r == roleName.ToLower(CultureInfo.InvariantCulture));
			}
		}
	}
}