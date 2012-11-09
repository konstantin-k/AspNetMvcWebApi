// Type: System.Web.Http.AuthorizeAttribute
// Assembly: System.Web.Http, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// Assembly location: C:\Program Files (x86)\Microsoft ASP.NET\ASP.NET MVC 4\Assemblies\System.Web.Http.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Properties;

namespace System.Web.Http
{
  /// <summary>
  /// Specifies the authorization filter that verifies the request's <see cref="T:System.Security.Principal.IPrincipal"/>.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
  public class AuthorizeAttribute : AuthorizationFilterAttribute
  {
    private static readonly string[] _emptyArray = new string[0];
    private readonly object _typeId = new object();
    private string[] _rolesSplit = AuthorizeAttribute._emptyArray;
    private string[] _usersSplit = AuthorizeAttribute._emptyArray;
    private string _roles;
    private string _users;

    /// <summary>
    /// Gets or sets the authorized roles.
    /// </summary>
    /// 
    /// <returns>
    /// The roles string.
    /// </returns>
    public string Roles
    {
      get
      {
        return this._roles ?? string.Empty;
      }
      set
      {
        this._roles = value;
        this._rolesSplit = AuthorizeAttribute.SplitString(value);
      }
    }

    /// <summary>
    /// Gets a unique identifier for this attribute.
    /// </summary>
    /// 
    /// <returns>
    /// A unique identifier for this attribute.
    /// </returns>
    public override object TypeId
    {
      get
      {
        return this._typeId;
      }
    }

    /// <summary>
    /// Gets or sets the authorized users.
    /// </summary>
    /// 
    /// <returns>
    /// The users string.
    /// </returns>
    public string Users
    {
      get
      {
        return this._users ?? string.Empty;
      }
      set
      {
        this._users = value;
        this._usersSplit = AuthorizeAttribute.SplitString(value);
      }
    }

    static AuthorizeAttribute()
    {
    }

    /// <summary>
    /// Indicates whether the specified control is authorized.
    /// </summary>
    /// 
    /// <returns>
    /// true if the control is authorized; otherwise, false.
    /// </returns>
    /// <param name="actionContext">The context.</param>
    protected virtual bool IsAuthorized(HttpActionContext actionContext)
    {
      if (actionContext == null)
        throw Error.ArgumentNull("actionContext");
      IPrincipal currentPrincipal = Thread.CurrentPrincipal;
      return currentPrincipal != null && currentPrincipal.Identity.IsAuthenticated && (this._usersSplit.Length <= 0 || Enumerable.Contains<string>((IEnumerable<string>) this._usersSplit, currentPrincipal.Identity.Name, (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase)) && (this._rolesSplit.Length <= 0 || Enumerable.Any<string>((IEnumerable<string>) this._rolesSplit, new Func<string, bool>(currentPrincipal.IsInRole)));
    }

    /// <summary>
    /// Calls when an action is being authorized.
    /// </summary>
    /// <param name="actionContext">The context.</param><exception cref="T:System.ArgumentNullException">The context parameter is null.</exception>
    public override void OnAuthorization(HttpActionContext actionContext)
    {
      if (actionContext == null)
        throw Error.ArgumentNull("actionContext");
      if (AuthorizeAttribute.SkipAuthorization(actionContext) || this.IsAuthorized(actionContext))
        return;
      this.HandleUnauthorizedRequest(actionContext);
    }

    /// <summary>
    /// Processes requests that fail authorization.
    /// </summary>
    /// <param name="actionContext">The context.</param>
    protected virtual void HandleUnauthorizedRequest(HttpActionContext actionContext)
    {
      if (actionContext == null)
        throw Error.ArgumentNull("actionContext");
      actionContext.Response = HttpRequestMessageExtensions.CreateErrorResponse(actionContext.ControllerContext.Request, HttpStatusCode.Unauthorized, SRResources.RequestNotAuthorized);
    }

    private static bool SkipAuthorization(HttpActionContext actionContext)
    {
      if (!Enumerable.Any<AllowAnonymousAttribute>((IEnumerable<AllowAnonymousAttribute>) actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>()))
        return Enumerable.Any<AllowAnonymousAttribute>((IEnumerable<AllowAnonymousAttribute>) actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>());
      else
        return true;
    }

    internal static string[] SplitString(string original)
    {
      if (string.IsNullOrEmpty(original))
        return AuthorizeAttribute._emptyArray;
      return Enumerable.ToArray<string>(Enumerable.Select(Enumerable.Where(Enumerable.Select((IEnumerable<string>) original.Split(new char[1]
      {
        ','
      }), piece => new
      {
        piece = piece,
        trimmed = piece.Trim()
      }), param0 => !string.IsNullOrEmpty(param0.trimmed)), param0 => param0.trimmed));
    }
  }
}
