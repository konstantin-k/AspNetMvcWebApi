// Type: System.Web.Mvc.Controller
// Assembly: System.Web.Mvc, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// Assembly location: C:\Program Files (x86)\Microsoft ASP.NET\ASP.NET MVC 4\Assemblies\System.Web.Mvc.dll

using System;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc.Async;
using System.Web.Mvc.Properties;
using System.Web.Profile;
using System.Web.Routing;

namespace System.Web.Mvc
{
  /// <summary>
  /// Provides methods that respond to HTTP requests that are made to an ASP.NET MVC Web site.
  /// </summary>
  public abstract class Controller : ControllerBase, IActionFilter, IAuthorizationFilter, IDisposable, IExceptionFilter, IResultFilter, IAsyncController, IController, IAsyncManagerContainer
  {
    private static readonly object _executeTag = new object();
    private static readonly object _executeCoreTag = new object();
    private readonly AsyncManager _asyncManager = new AsyncManager();
    private IActionInvoker _actionInvoker;
    private ModelBinderDictionary _binders;
    private RouteCollection _routeCollection;
    private ITempDataProvider _tempDataProvider;
    private ViewEngineCollection _viewEngineCollection;
    private IDependencyResolver _resolver;

    internal IDependencyResolver Resolver
    {
      get
      {
        return this._resolver ?? DependencyResolver.CurrentCache;
      }
      set
      {
        this._resolver = value;
      }
    }

    /// <summary>
    /// Provides asynchronous operations.
    /// </summary>
    /// 
    /// <returns>
    /// Returns <see cref="T:System.Web.Mvc.Async.AsyncManager"/>.
    /// </returns>
    public AsyncManager AsyncManager
    {
      get
      {
        return this._asyncManager;
      }
    }

    /// <summary>
    /// Disable asynchronous support to provide backward compatibility.
    /// </summary>
    /// 
    /// <returns>
    /// true if asynchronous support is disabled; otherwise false.
    /// </returns>
    protected virtual bool DisableAsyncSupport
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Gets the action invoker for the controller.
    /// </summary>
    /// 
    /// <returns>
    /// The action invoker.
    /// </returns>
    public IActionInvoker ActionInvoker
    {
      get
      {
        if (this._actionInvoker == null)
          this._actionInvoker = this.CreateActionInvoker();
        return this._actionInvoker;
      }
      set
      {
        this._actionInvoker = value;
      }
    }

    /// <summary>
    /// Gets or sets the binder.
    /// </summary>
    /// 
    /// <returns>
    /// The binder.
    /// </returns>
    protected internal ModelBinderDictionary Binders
    {
      get
      {
        if (this._binders == null)
          this._binders = ModelBinders.Binders;
        return this._binders;
      }
      set
      {
        this._binders = value;
      }
    }

    /// <summary>
    /// Gets HTTP-specific information about an individual HTTP request.
    /// </summary>
    /// 
    /// <returns>
    /// The HTTP context.
    /// </returns>
    public HttpContextBase HttpContext
    {
      get
      {
        if (this.ControllerContext != null)
          return this.ControllerContext.HttpContext;
        else
          return (HttpContextBase) null;
      }
    }

    /// <summary>
    /// Gets the model state dictionary object that contains the state of the model and of model-binding validation.
    /// </summary>
    /// 
    /// <returns>
    /// The model state dictionary.
    /// </returns>
    public ModelStateDictionary ModelState
    {
      get
      {
        return this.ViewData.ModelState;
      }
    }

    /// <summary>
    /// Gets the HTTP context profile.
    /// </summary>
    /// 
    /// <returns>
    /// The HTTP context profile.
    /// </returns>
    public ProfileBase Profile
    {
      get
      {
        if (this.HttpContext != null)
          return this.HttpContext.Profile;
        else
          return (ProfileBase) null;
      }
    }

    /// <summary>
    /// Gets the <see cref="T:System.Web.HttpRequestBase"/> object for the current HTTP request.
    /// </summary>
    /// 
    /// <returns>
    /// The request object.
    /// </returns>
    public HttpRequestBase Request
    {
      get
      {
        if (this.HttpContext != null)
          return this.HttpContext.Request;
        else
          return (HttpRequestBase) null;
      }
    }

    /// <summary>
    /// Gets the <see cref="T:System.Web.HttpResponseBase"/> object for the current HTTP response.
    /// </summary>
    /// 
    /// <returns>
    /// The response object.
    /// </returns>
    public HttpResponseBase Response
    {
      get
      {
        if (this.HttpContext != null)
          return this.HttpContext.Response;
        else
          return (HttpResponseBase) null;
      }
    }

    internal RouteCollection RouteCollection
    {
      get
      {
        if (this._routeCollection == null)
          this._routeCollection = RouteTable.Routes;
        return this._routeCollection;
      }
      set
      {
        this._routeCollection = value;
      }
    }

    /// <summary>
    /// Gets the route data for the current request.
    /// </summary>
    /// 
    /// <returns>
    /// The route data.
    /// </returns>
    public RouteData RouteData
    {
      get
      {
        if (this.ControllerContext != null)
          return this.ControllerContext.RouteData;
        else
          return (RouteData) null;
      }
    }

    /// <summary>
    /// Gets the <see cref="T:System.Web.HttpServerUtilityBase"/> object that provides methods that are used during Web request processing.
    /// </summary>
    /// 
    /// <returns>
    /// The HTTP server object.
    /// </returns>
    public HttpServerUtilityBase Server
    {
      get
      {
        if (this.HttpContext != null)
          return this.HttpContext.Server;
        else
          return (HttpServerUtilityBase) null;
      }
    }

    /// <summary>
    /// Gets the <see cref="T:System.Web.HttpSessionStateBase"/> object for the current HTTP request.
    /// </summary>
    /// 
    /// <returns>
    /// The HTTP session-state object for the current HTTP request.
    /// </returns>
    public HttpSessionStateBase Session
    {
      get
      {
        if (this.HttpContext != null)
          return this.HttpContext.Session;
        else
          return (HttpSessionStateBase) null;
      }
    }

    /// <summary>
    /// Gets the temporary-data provider object that is used to store data for the next request.
    /// </summary>
    /// 
    /// <returns>
    /// The temporary-data provider.
    /// </returns>
    public ITempDataProvider TempDataProvider
    {
      get
      {
        if (this._tempDataProvider == null)
          this._tempDataProvider = this.CreateTempDataProvider();
        return this._tempDataProvider;
      }
      set
      {
        this._tempDataProvider = value;
      }
    }

    /// <summary>
    /// Gets the URL helper object that is used to generate URLs by using routing.
    /// </summary>
    /// 
    /// <returns>
    /// The URL helper object.
    /// </returns>
    public UrlHelper Url { get; set; }

    /// <summary>
    /// Gets the user security information for the current HTTP request.
    /// </summary>
    /// 
    /// <returns>
    /// The user security information for the current HTTP request.
    /// </returns>
    public IPrincipal User
    {
      get
      {
        if (this.HttpContext != null)
          return this.HttpContext.User;
        else
          return (IPrincipal) null;
      }
    }

    /// <summary>
    /// Gets the view engine collection.
    /// </summary>
    /// 
    /// <returns>
    /// The view engine collection.
    /// </returns>
    public ViewEngineCollection ViewEngineCollection
    {
      get
      {
        return this._viewEngineCollection ?? ViewEngines.Engines;
      }
      set
      {
        this._viewEngineCollection = value;
      }
    }

    static Controller()
    {
    }

    /// <summary>
    /// Creates a content result object by using a string.
    /// </summary>
    /// 
    /// <returns>
    /// The content result instance.
    /// </returns>
    /// <param name="content">The content to write to the response.</param>
    protected internal ContentResult Content(string content)
    {
      return this.Content(content, (string) null);
    }

    /// <summary>
    /// Creates a content result object by using a string and the content type.
    /// </summary>
    /// 
    /// <returns>
    /// The content result instance.
    /// </returns>
    /// <param name="content">The content to write to the response.</param><param name="contentType">The content type (MIME type).</param>
    protected internal ContentResult Content(string content, string contentType)
    {
      return this.Content(content, contentType, (Encoding) null);
    }

    /// <summary>
    /// Creates a content result object by using a string, the content type, and content encoding.
    /// </summary>
    /// 
    /// <returns>
    /// The content result instance.
    /// </returns>
    /// <param name="content">The content to write to the response.</param><param name="contentType">The content type (MIME type).</param><param name="contentEncoding">The content encoding.</param>
    protected internal virtual ContentResult Content(string content, string contentType, Encoding contentEncoding)
    {
      return new ContentResult()
      {
        Content = content,
        ContentType = contentType,
        ContentEncoding = contentEncoding
      };
    }

    /// <summary>
    /// Creates an action invoker.
    /// </summary>
    /// 
    /// <returns>
    /// An action invoker.
    /// </returns>
    protected virtual IActionInvoker CreateActionInvoker()
    {
      return (IActionInvoker) DependencyResolverExtensions.GetService<IAsyncActionInvoker>(this.Resolver) ?? DependencyResolverExtensions.GetService<IActionInvoker>(this.Resolver) ?? (IActionInvoker) new AsyncControllerActionInvoker();
    }

    /// <summary>
    /// Creates a temporary data provider.
    /// </summary>
    /// 
    /// <returns>
    /// A temporary data provider.
    /// </returns>
    protected virtual ITempDataProvider CreateTempDataProvider()
    {
      return DependencyResolverExtensions.GetService<ITempDataProvider>(this.Resolver) ?? (ITempDataProvider) new SessionStateTempDataProvider();
    }

    /// <summary>
    /// Releases all resources that are used by the current instance of the <see cref="T:System.Web.Mvc.Controller"/> class.
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    /// <summary>
    /// Releases unmanaged resources and optionally releases managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>
    /// Invokes the action in the current controller context.
    /// </summary>
    protected override void ExecuteCore()
    {
      this.PossiblyLoadTempData();
      try
      {
        string requiredString = this.RouteData.GetRequiredString("action");
        if (this.ActionInvoker.InvokeAction(this.ControllerContext, requiredString))
          return;
        this.HandleUnknownAction(requiredString);
      }
      finally
      {
        this.PossiblySaveTempData();
      }
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.FileContentResult"/> object by using the file contents and file type.
    /// </summary>
    /// 
    /// <returns>
    /// The file-content result object.
    /// </returns>
    /// <param name="fileContents">The binary content to send to the response.</param><param name="contentType">The content type (MIME type).</param>
    protected internal FileContentResult File(byte[] fileContents, string contentType)
    {
      return this.File(fileContents, contentType, (string) null);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.FileContentResult"/> object by using the file contents, content type, and the destination file name.
    /// </summary>
    /// 
    /// <returns>
    /// The file-content result object.
    /// </returns>
    /// <param name="fileContents">The binary content to send to the response.</param><param name="contentType">The content type (MIME type).</param><param name="fileDownloadName">The file name to use in the file-download dialog box that is displayed in the browser.</param>
    protected internal virtual FileContentResult File(byte[] fileContents, string contentType, string fileDownloadName)
    {
      FileContentResult fileContentResult = new FileContentResult(fileContents, contentType);
      fileContentResult.FileDownloadName = fileDownloadName;
      return fileContentResult;
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.FileStreamResult"/> object by using the <see cref="T:System.IO.Stream"/> object and content type.
    /// </summary>
    /// 
    /// <returns>
    /// The file-content result object.
    /// </returns>
    /// <param name="fileStream">The stream to send to the response.</param><param name="contentType">The content type (MIME type).</param>
    protected internal FileStreamResult File(Stream fileStream, string contentType)
    {
      return this.File(fileStream, contentType, (string) null);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.FileStreamResult"/> object using the <see cref="T:System.IO.Stream"/> object, the content type, and the target file name.
    /// </summary>
    /// 
    /// <returns>
    /// The file-stream result object.
    /// </returns>
    /// <param name="fileStream">The stream to send to the response.</param><param name="contentType">The content type (MIME type)</param><param name="fileDownloadName">The file name to use in the file-download dialog box that is displayed in the browser.</param>
    protected internal virtual FileStreamResult File(Stream fileStream, string contentType, string fileDownloadName)
    {
      FileStreamResult fileStreamResult = new FileStreamResult(fileStream, contentType);
      fileStreamResult.FileDownloadName = fileDownloadName;
      return fileStreamResult;
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.FilePathResult"/> object by using the file name and the content type.
    /// </summary>
    /// 
    /// <returns>
    /// The file-stream result object.
    /// </returns>
    /// <param name="fileName">The path of the file to send to the response.</param><param name="contentType">The content type (MIME type).</param>
    protected internal FilePathResult File(string fileName, string contentType)
    {
      return this.File(fileName, contentType, (string) null);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.FilePathResult"/> object by using the file name, the content type, and the file download name.
    /// </summary>
    /// 
    /// <returns>
    /// The file-stream result object.
    /// </returns>
    /// <param name="fileName">The path of the file to send to the response.</param><param name="contentType">The content type (MIME type).</param><param name="fileDownloadName">The file name to use in the file-download dialog box that is displayed in the browser.</param>
    protected internal virtual FilePathResult File(string fileName, string contentType, string fileDownloadName)
    {
      FilePathResult filePathResult = new FilePathResult(fileName, contentType);
      filePathResult.FileDownloadName = fileDownloadName;
      return filePathResult;
    }

    /// <summary>
    /// Called when a request matches this controller, but no method with the specified action name is found in the controller.
    /// </summary>
    /// <param name="actionName">The name of the attempted action.</param>
    protected virtual void HandleUnknownAction(string actionName)
    {
      throw new HttpException(404, string.Format((IFormatProvider) CultureInfo.CurrentCulture, MvcResources.Controller_UnknownAction, new object[2]
      {
        (object) actionName,
        (object) this.GetType().FullName
      }));
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.HttpNotFoundResult"/> class.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.HttpNotFoundResult"/> class.
    /// </returns>
    protected internal HttpNotFoundResult HttpNotFound()
    {
      return this.HttpNotFound((string) null);
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.HttpNotFoundResult"/> class.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.HttpNotFoundResult"/> class.
    /// </returns>
    /// <param name="statusDescription">The status description.</param>
    protected internal virtual HttpNotFoundResult HttpNotFound(string statusDescription)
    {
      return new HttpNotFoundResult(statusDescription);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.JavaScriptResult"/> object.
    /// </summary>
    /// 
    /// <returns>
    /// The <see cref="T:System.Web.Mvc.JavaScriptResult"/> object that writes the script to the response.
    /// </returns>
    /// <param name="script">The JavaScript code to run on the client</param>
    protected internal virtual JavaScriptResult JavaScript(string script)
    {
      return new JavaScriptResult()
      {
        Script = script
      };
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.JsonResult"/> object that serializes the specified object to JavaScript Object Notation (JSON).
    /// </summary>
    /// 
    /// <returns>
    /// The JSON result object that serializes the specified object to JSON format. The result object that is prepared by this method is written to the response by the ASP.NET MVC framework when the object is executed.
    /// </returns>
    /// <param name="data">The JavaScript object graph to serialize.</param>
    protected internal JsonResult Json(object data)
    {
      return this.Json(data, (string) null, (Encoding) null, JsonRequestBehavior.DenyGet);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.JsonResult"/> object that serializes the specified object to JavaScript Object Notation (JSON) format.
    /// </summary>
    /// 
    /// <returns>
    /// The JSON result object that serializes the specified object to JSON format.
    /// </returns>
    /// <param name="data">The JavaScript object graph to serialize.</param><param name="contentType">The content type (MIME type).</param>
    protected internal JsonResult Json(object data, string contentType)
    {
      return this.Json(data, contentType, (Encoding) null, JsonRequestBehavior.DenyGet);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.JsonResult"/> object that serializes the specified object to JavaScript Object Notation (JSON) format.
    /// </summary>
    /// 
    /// <returns>
    /// The JSON result object that serializes the specified object to JSON format.
    /// </returns>
    /// <param name="data">The JavaScript object graph to serialize.</param><param name="contentType">The content type (MIME type).</param><param name="contentEncoding">The content encoding.</param>
    protected internal virtual JsonResult Json(object data, string contentType, Encoding contentEncoding)
    {
      return this.Json(data, contentType, contentEncoding, JsonRequestBehavior.DenyGet);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.JsonResult"/> object that serializes the specified object to JavaScript Object Notation (JSON) format using the specified JSON request behavior.
    /// </summary>
    /// 
    /// <returns>
    /// The result object that serializes the specified object to JSON format.
    /// </returns>
    /// <param name="data">The JavaScript object graph to serialize.</param><param name="behavior">The JSON request behavior.</param>
    protected internal JsonResult Json(object data, JsonRequestBehavior behavior)
    {
      return this.Json(data, (string) null, (Encoding) null, behavior);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.JsonResult"/> object that serializes the specified object to JavaScript Object Notation (JSON) format using the specified content type and JSON request behavior.
    /// </summary>
    /// 
    /// <returns>
    /// The result object that serializes the specified object to JSON format.
    /// </returns>
    /// <param name="data">The JavaScript object graph to serialize.</param><param name="contentType">The content type (MIME type).</param><param name="behavior">The JSON request behavior</param>
    protected internal JsonResult Json(object data, string contentType, JsonRequestBehavior behavior)
    {
      return this.Json(data, contentType, (Encoding) null, behavior);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.JsonResult"/> object that serializes the specified object to JavaScript Object Notation (JSON) format using the content type, content encoding, and the JSON request behavior.
    /// </summary>
    /// 
    /// <returns>
    /// The result object that serializes the specified object to JSON format.
    /// </returns>
    /// <param name="data">The JavaScript object graph to serialize.</param><param name="contentType">The content type (MIME type).</param><param name="contentEncoding">The content encoding.</param><param name="behavior">The JSON request behavior </param>
    protected internal virtual JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
    {
      return new JsonResult()
      {
        Data = data,
        ContentType = contentType,
        ContentEncoding = contentEncoding,
        JsonRequestBehavior = behavior
      };
    }

    /// <summary>
    /// Initializes data that might not be available when the constructor is called.
    /// </summary>
    /// <param name="requestContext">The HTTP context and route data.</param>
    protected override void Initialize(RequestContext requestContext)
    {
      base.Initialize(requestContext);
      this.Url = new UrlHelper(requestContext);
    }

    /// <summary>
    /// Called before the action method is invoked.
    /// </summary>
    /// <param name="filterContext">Information about the current request and action.</param>
    protected virtual void OnActionExecuting(ActionExecutingContext filterContext)
    {
    }

    /// <summary>
    /// Called after the action method is invoked.
    /// </summary>
    /// <param name="filterContext">Information about the current request and action.</param>
    protected virtual void OnActionExecuted(ActionExecutedContext filterContext)
    {
    }

    /// <summary>
    /// Called when authorization occurs.
    /// </summary>
    /// <param name="filterContext">Information about the current request and action.</param>
    protected virtual void OnAuthorization(AuthorizationContext filterContext)
    {
    }

    /// <summary>
    /// Called when an unhandled exception occurs in the action.
    /// </summary>
    /// <param name="filterContext">Information about the current request and action.</param>
    protected virtual void OnException(ExceptionContext filterContext)
    {
    }

    /// <summary>
    /// Called after the action result that is returned by an action method is executed.
    /// </summary>
    /// <param name="filterContext">Information about the current request and action result</param>
    protected virtual void OnResultExecuted(ResultExecutedContext filterContext)
    {
    }

    /// <summary>
    /// Called before the action result that is returned by an action method is executed.
    /// </summary>
    /// <param name="filterContext">Information about the current request and action result</param>
    protected virtual void OnResultExecuting(ResultExecutingContext filterContext)
    {
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.PartialViewResult"/> object that renders a partial view.
    /// </summary>
    /// 
    /// <returns>
    /// A partial-view result object.
    /// </returns>
    protected internal PartialViewResult PartialView()
    {
      return this.PartialView((string) null, (object) null);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.PartialViewResult"/> object that renders a partial view, by using the specified model.
    /// </summary>
    /// 
    /// <returns>
    /// A partial-view result object.
    /// </returns>
    /// <param name="model">The model that is rendered by the partial view</param>
    protected internal PartialViewResult PartialView(object model)
    {
      return this.PartialView((string) null, model);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.PartialViewResult"/> object that renders a partial view, by using the specified view name.
    /// </summary>
    /// 
    /// <returns>
    /// A partial-view result object.
    /// </returns>
    /// <param name="viewName">The name of the view that is rendered to the response.</param>
    protected internal PartialViewResult PartialView(string viewName)
    {
      return this.PartialView(viewName, (object) null);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.PartialViewResult"/> object that renders a partial view, by using the specified view name and model.
    /// </summary>
    /// 
    /// <returns>
    /// A partial-view result object.
    /// </returns>
    /// <param name="viewName">The name of the view that is rendered to the response.</param><param name="model">The model that is rendered by the partial view</param>
    protected internal virtual PartialViewResult PartialView(string viewName, object model)
    {
      if (model != null)
        this.ViewData.Model = model;
      PartialViewResult partialViewResult = new PartialViewResult();
      partialViewResult.ViewName = viewName;
      partialViewResult.ViewData = this.ViewData;
      partialViewResult.TempData = this.TempData;
      partialViewResult.ViewEngineCollection = this.ViewEngineCollection;
      return partialViewResult;
    }

    internal void PossiblyLoadTempData()
    {
      if (this.ControllerContext.IsChildAction)
        return;
      this.TempData.Load(this.ControllerContext, this.TempDataProvider);
    }

    internal void PossiblySaveTempData()
    {
      if (this.ControllerContext.IsChildAction)
        return;
      this.TempData.Save(this.ControllerContext, this.TempDataProvider);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.RedirectResult"/> object that redirects to the specified URL.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect result object.
    /// </returns>
    /// <param name="url">The URL to redirect to.</param>
    protected internal virtual RedirectResult Redirect(string url)
    {
      if (string.IsNullOrEmpty(url))
        throw new ArgumentException(MvcResources.Common_NullOrEmpty, "url");
      else
        return new RedirectResult(url);
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true.
    /// </returns>
    /// <param name="url">The URL to redirect to.</param>
    protected internal virtual RedirectResult RedirectPermanent(string url)
    {
      if (string.IsNullOrEmpty(url))
        throw new ArgumentException(MvcResources.Common_NullOrEmpty, "url");
      bool permanent = true;
      return new RedirectResult(url, permanent);
    }

    /// <summary>
    /// Redirects to the specified action using the action name.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect result object.
    /// </returns>
    /// <param name="actionName">The name of the action.</param>
    protected internal RedirectToRouteResult RedirectToAction(string actionName)
    {
      return this.RedirectToAction(actionName, (RouteValueDictionary) null);
    }

    /// <summary>
    /// Redirects to the specified action using the action name and route values.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect result object.
    /// </returns>
    /// <param name="actionName">The name of the action.</param><param name="routeValues">The parameters for a route. </param>
    protected internal RedirectToRouteResult RedirectToAction(string actionName, object routeValues)
    {
      return this.RedirectToAction(actionName, new RouteValueDictionary(routeValues));
    }

    /// <summary>
    /// Redirects to the specified action using the action name and route dictionary.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect result object.
    /// </returns>
    /// <param name="actionName">The name of the action.</param><param name="routeValues">The parameters for a route.</param>
    protected internal RedirectToRouteResult RedirectToAction(string actionName, RouteValueDictionary routeValues)
    {
      return this.RedirectToAction(actionName, (string) null, routeValues);
    }

    /// <summary>
    /// Redirects to the specified action using the action name and controller name.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect result object.
    /// </returns>
    /// <param name="actionName">The name of the action.</param><param name="controllerName">The name of the controller</param>
    protected internal RedirectToRouteResult RedirectToAction(string actionName, string controllerName)
    {
      return this.RedirectToAction(actionName, controllerName, (RouteValueDictionary) null);
    }

    /// <summary>
    /// Redirects to the specified action using the action name, controller name, and route values.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect result object.
    /// </returns>
    /// <param name="actionName">The name of the action.</param><param name="controllerName">The name of the controller</param><param name="routeValues">The parameters for a route. </param>
    protected internal RedirectToRouteResult RedirectToAction(string actionName, string controllerName, object routeValues)
    {
      return this.RedirectToAction(actionName, controllerName, new RouteValueDictionary(routeValues));
    }

    /// <summary>
    /// Redirects to the specified action using the action name, controller name, and route dictionary.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect result object.
    /// </returns>
    /// <param name="actionName">The name of the action.</param><param name="controllerName">The name of the controller</param><param name="routeValues">The parameters for a route.</param>
    protected internal virtual RedirectToRouteResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues)
    {
      RouteValueDictionary routeValues1;
      if (this.RouteData == null)
      {
        bool includeImplicitMvcValues = true;
        routeValues1 = RouteValuesHelpers.MergeRouteValues(actionName, controllerName, (RouteValueDictionary) null, routeValues, includeImplicitMvcValues);
      }
      else
      {
        bool includeImplicitMvcValues = true;
        routeValues1 = RouteValuesHelpers.MergeRouteValues(actionName, controllerName, this.RouteData.Values, routeValues, includeImplicitMvcValues);
      }
      return new RedirectToRouteResult(routeValues1);
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name, controller name, and route values.
    /// </returns>
    /// <param name="actionName">The action name.</param>
    protected internal RedirectToRouteResult RedirectToActionPermanent(string actionName)
    {
      return this.RedirectToActionPermanent(actionName, (RouteValueDictionary) null);
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name, and route values.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name, and route values.
    /// </returns>
    /// <param name="actionName">The action name.</param><param name="routeValues">The route values.</param>
    protected internal RedirectToRouteResult RedirectToActionPermanent(string actionName, object routeValues)
    {
      return this.RedirectToActionPermanent(actionName, new RouteValueDictionary(routeValues));
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name,  and route values.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name,  and route values.
    /// </returns>
    /// <param name="actionName">The action name.</param><param name="routeValues">The route values.</param>
    protected internal RedirectToRouteResult RedirectToActionPermanent(string actionName, RouteValueDictionary routeValues)
    {
      return this.RedirectToActionPermanent(actionName, (string) null, routeValues);
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name,  and controller name.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name,  and controller name.
    /// </returns>
    /// <param name="actionName">The action name.</param><param name="controllerName">The controller name.</param>
    protected internal RedirectToRouteResult RedirectToActionPermanent(string actionName, string controllerName)
    {
      return this.RedirectToActionPermanent(actionName, controllerName, (RouteValueDictionary) null);
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name, controller name, and route values.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true.
    /// </returns>
    /// <param name="actionName">The action name.</param><param name="controllerName">The controller name.</param><param name="routeValues">The route values.</param>
    protected internal RedirectToRouteResult RedirectToActionPermanent(string actionName, string controllerName, object routeValues)
    {
      return this.RedirectToActionPermanent(actionName, controllerName, new RouteValueDictionary(routeValues));
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name, controller name, and route values.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified action name, controller name, and route values.
    /// </returns>
    /// <param name="actionName">The action name.</param><param name="controllerName">The controller name.</param><param name="routeValues">The route values.</param>
    protected internal virtual RedirectToRouteResult RedirectToActionPermanent(string actionName, string controllerName, RouteValueDictionary routeValues)
    {
      RouteValueDictionary implicitRouteValues = this.RouteData != null ? this.RouteData.Values : (RouteValueDictionary) null;
      bool includeImplicitMvcValues = true;
      return new RedirectToRouteResult((string) null, RouteValuesHelpers.MergeRouteValues(actionName, controllerName, implicitRouteValues, routeValues, includeImplicitMvcValues), true);
    }

    /// <summary>
    /// Redirects to the specified route using the specified route values.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect-to-route result object.
    /// </returns>
    /// <param name="routeValues">The parameters for a route. </param>
    protected internal RedirectToRouteResult RedirectToRoute(object routeValues)
    {
      return this.RedirectToRoute(new RouteValueDictionary(routeValues));
    }

    /// <summary>
    /// Redirects to the specified route using the route dictionary.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect-to-route result object.
    /// </returns>
    /// <param name="routeValues">The parameters for a route.</param>
    protected internal RedirectToRouteResult RedirectToRoute(RouteValueDictionary routeValues)
    {
      return this.RedirectToRoute((string) null, routeValues);
    }

    /// <summary>
    /// Redirects to the specified route using the route name.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect-to-route result object.
    /// </returns>
    /// <param name="routeName">The name of the route</param>
    protected internal RedirectToRouteResult RedirectToRoute(string routeName)
    {
      return this.RedirectToRoute(routeName, (RouteValueDictionary) null);
    }

    /// <summary>
    /// Redirects to the specified route using the route name and route values.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect-to-route result object.
    /// </returns>
    /// <param name="routeName">The name of the route</param><param name="routeValues">The parameters for a route. </param>
    protected internal RedirectToRouteResult RedirectToRoute(string routeName, object routeValues)
    {
      return this.RedirectToRoute(routeName, new RouteValueDictionary(routeValues));
    }

    /// <summary>
    /// Redirects to the specified route using the route name and route dictionary.
    /// </summary>
    /// 
    /// <returns>
    /// The redirect-to-route result object.
    /// </returns>
    /// <param name="routeName">The name of the route</param><param name="routeValues">The parameters for a route.</param>
    protected internal virtual RedirectToRouteResult RedirectToRoute(string routeName, RouteValueDictionary routeValues)
    {
      return new RedirectToRouteResult(routeName, RouteValuesHelpers.GetRouteValues(routeValues));
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified route values.
    /// </summary>
    /// 
    /// <returns>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true.
    /// </returns>
    /// <param name="routeValues">The route name.</param>
    protected internal RedirectToRouteResult RedirectToRoutePermanent(object routeValues)
    {
      return this.RedirectToRoutePermanent(new RouteValueDictionary(routeValues));
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified route values.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified route values.
    /// </returns>
    /// <param name="routeValues">The route values.</param>
    protected internal RedirectToRouteResult RedirectToRoutePermanent(RouteValueDictionary routeValues)
    {
      return this.RedirectToRoutePermanent((string) null, routeValues);
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified route name.
    /// </summary>
    /// 
    /// <returns>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified route name.
    /// </returns>
    /// <param name="routeName">The route name.</param>
    protected internal RedirectToRouteResult RedirectToRoutePermanent(string routeName)
    {
      return this.RedirectToRoutePermanent(routeName, (RouteValueDictionary) null);
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified route name and route values.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true.
    /// </returns>
    /// <param name="routeName">The route name.</param><param name="routeValues">The route values.</param>
    protected internal RedirectToRouteResult RedirectToRoutePermanent(string routeName, object routeValues)
    {
      return this.RedirectToRoutePermanent(routeName, new RouteValueDictionary(routeValues));
    }

    /// <summary>
    /// Returns an instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified route name and route values.
    /// </summary>
    /// 
    /// <returns>
    /// An instance of the <see cref="T:System.Web.Mvc.RedirectResult"/> class with the <see cref="P:System.Web.Mvc.RedirectResult.Permanent"/> property set to true using the specified route name and route values.
    /// </returns>
    /// <param name="routeName">The route name.</param><param name="routeValues">The route values.</param>
    protected internal virtual RedirectToRouteResult RedirectToRoutePermanent(string routeName, RouteValueDictionary routeValues)
    {
      bool permanent = true;
      return new RedirectToRouteResult(routeName, RouteValuesHelpers.GetRouteValues(routeValues), permanent);
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller's current value provider.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><typeparam name="TModel">The type of the model object.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="model"/> parameter or the <see cref="P:System.Web.Mvc.ControllerBase.ValueProvider"/> property is null.</exception>
    protected internal bool TryUpdateModel<TModel>(TModel model) where TModel : class
    {
      return this.TryUpdateModel<TModel>(model, (string) null, (string[]) null, (string[]) null, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller's current value provider and a prefix.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><param name="prefix">The prefix to use when looking up values in the value provider.</param><typeparam name="TModel">The type of the model object.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="model"/> parameter or the <see cref="P:System.Web.Mvc.ControllerBase.ValueProvider"/> property is null.</exception>
    protected internal bool TryUpdateModel<TModel>(TModel model, string prefix) where TModel : class
    {
      return this.TryUpdateModel<TModel>(model, prefix, (string[]) null, (string[]) null, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller's current value provider and included properties.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><param name="includeProperties">A list of properties of the model to update.</param><typeparam name="TModel">The type of the model object.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="model"/> parameter or the <see cref="P:System.Web.Mvc.ControllerBase.ValueProvider"/> property is null.</exception>
    protected internal bool TryUpdateModel<TModel>(TModel model, string[] includeProperties) where TModel : class
    {
      return this.TryUpdateModel<TModel>(model, (string) null, includeProperties, (string[]) null, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller's current value provider, a prefix, and included properties.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><param name="prefix">The prefix to use when looking up values in the value provider.</param><param name="includeProperties">A list of properties of the model to update.</param><typeparam name="TModel">The type of the model object.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="model"/> parameter or the <see cref="P:System.Web.Mvc.ControllerBase.ValueProvider"/> property is null.</exception>
    protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties) where TModel : class
    {
      return this.TryUpdateModel<TModel>(model, prefix, includeProperties, (string[]) null, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller's current value provider, a prefix, a list of properties to exclude, and a list of properties to include.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><param name="prefix">The prefix to use when looking up values in the value provider</param><param name="includeProperties">A list of properties of the model to update.</param><param name="excludeProperties">A list of properties to explicitly exclude from the update. These are excluded even if they are listed in the <paramref name="includeProperties"/> parameter list.</param><typeparam name="TModel">The type of the model object.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="model"/> parameter or the <see cref="P:System.Web.Mvc.ControllerBase.ValueProvider"/> property is null.</exception>
    protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class
    {
      return this.TryUpdateModel<TModel>(model, prefix, includeProperties, excludeProperties, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal bool TryUpdateModel<TModel>(TModel model, IValueProvider valueProvider) where TModel : class
    {
      return this.TryUpdateModel<TModel>(model, (string) null, (string[]) null, (string[]) null, valueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider and a prefix.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><param name="prefix">The prefix to use when looking up values in the value provider.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, IValueProvider valueProvider) where TModel : class
    {
      return this.TryUpdateModel<TModel>(model, prefix, (string[]) null, (string[]) null, valueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider and a list of properties to include.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><param name="includeProperties">A list of properties of the model to update.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal bool TryUpdateModel<TModel>(TModel model, string[] includeProperties, IValueProvider valueProvider) where TModel : class
    {
      return this.TryUpdateModel<TModel>(model, (string) null, includeProperties, (string[]) null, valueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider, a prefix, and included properties.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><param name="prefix">The prefix to use when looking up values in the value provider.</param><param name="includeProperties">A list of properties of the model to update.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, IValueProvider valueProvider) where TModel : class
    {
      return this.TryUpdateModel<TModel>(model, prefix, includeProperties, (string[]) null, valueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider, a prefix, a list of properties to exclude , and a list of properties to include.
    /// </summary>
    /// 
    /// <returns>
    /// true if the update is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to update.</param><param name="prefix">The prefix to use when looking up values in the value provider.</param><param name="includeProperties">A list of properties of the model to update.</param><param name="excludeProperties">A list of properties to explicitly exclude from the update. These are excluded even if they are listed in the <paramref name="includeProperties"/> parameter list.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties, IValueProvider valueProvider) where TModel : class
    {
      if ((object) model == null)
        throw new ArgumentNullException("model");
      if (valueProvider == null)
        throw new ArgumentNullException("valueProvider");
      Predicate<string> predicate = (Predicate<string>) (propertyName => BindAttribute.IsPropertyAllowed(propertyName, includeProperties, excludeProperties));
      this.Binders.GetBinder(typeof (TModel)).BindModel(this.ControllerContext, new ModelBindingContext()
      {
        ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType((Func<object>) (() => (object) (TModel) model), typeof (TModel)),
        ModelName = prefix,
        ModelState = this.ModelState,
        PropertyFilter = predicate,
        ValueProvider = valueProvider
      });
      return this.ModelState.IsValid;
    }

    /// <summary>
    /// Validates the specified model instance.
    /// </summary>
    /// 
    /// <returns>
    /// true if the model validation is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model instance to validate.</param>
    protected internal bool TryValidateModel(object model)
    {
      return this.TryValidateModel(model, (string) null);
    }

    /// <summary>
    /// Validates the specified model instance using an HTML prefix.
    /// </summary>
    /// 
    /// <returns>
    /// true if the model validation is successful; otherwise, false.
    /// </returns>
    /// <param name="model">The model to validate.</param><param name="prefix">The prefix to use when looking up values in the model provider.</param>
    protected internal bool TryValidateModel(object model, string prefix)
    {
      if (model == null)
        throw new ArgumentNullException("model");
      foreach (ModelValidationResult validationResult in ModelValidator.GetModelValidator(ModelMetadataProviders.Current.GetMetadataForType((Func<object>) (() => model), model.GetType()), this.ControllerContext).Validate((object) null))
        this.ModelState.AddModelError(DefaultModelBinder.CreateSubPropertyName(prefix, validationResult.MemberName), validationResult.Message);
      return this.ModelState.IsValid;
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller's current value provider.
    /// </summary>
    /// <param name="model">The model instance to update.</param><typeparam name="TModel">The type of the model object.</typeparam><exception cref="T:System.InvalidOperationException">The model was not successfully updated.</exception>
    protected internal void UpdateModel<TModel>(TModel model) where TModel : class
    {
      this.UpdateModel<TModel>(model, (string) null, (string[]) null, (string[]) null, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller's current value provider and a prefix.
    /// </summary>
    /// <param name="model">The model instance to update.</param><param name="prefix">A prefix to use when looking up values in the value provider.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal void UpdateModel<TModel>(TModel model, string prefix) where TModel : class
    {
      this.UpdateModel<TModel>(model, prefix, (string[]) null, (string[]) null, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller object's current value provider.
    /// </summary>
    /// <param name="model">The model instance to update.</param><param name="includeProperties">A list of properties of the model to update.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal void UpdateModel<TModel>(TModel model, string[] includeProperties) where TModel : class
    {
      this.UpdateModel<TModel>(model, (string) null, includeProperties, (string[]) null, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller's current value provider, a prefix, and included properties.
    /// </summary>
    /// <param name="model">The model instance to update.</param><param name="prefix">A prefix to use when looking up values in the value provider.</param><param name="includeProperties">A list of properties of the model to update.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal void UpdateModel<TModel>(TModel model, string prefix, string[] includeProperties) where TModel : class
    {
      this.UpdateModel<TModel>(model, prefix, includeProperties, (string[]) null, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the controller's current value provider, a prefix, a list of properties to exclude, and a list of properties to include.
    /// </summary>
    /// <param name="model">The model instance to update.</param><param name="prefix">A prefix to use when looking up values in the value provider.</param><param name="includeProperties">A list of properties of the model to update.</param><param name="excludeProperties">A list of properties to explicitly exclude from the update. These are excluded even if they are listed in the <paramref name="includeProperties"/> list.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal void UpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class
    {
      this.UpdateModel<TModel>(model, prefix, includeProperties, excludeProperties, this.ValueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider.
    /// </summary>
    /// <param name="model">The model instance to update.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal void UpdateModel<TModel>(TModel model, IValueProvider valueProvider) where TModel : class
    {
      this.UpdateModel<TModel>(model, (string) null, (string[]) null, (string[]) null, valueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider and a prefix.
    /// </summary>
    /// <param name="model">The model instance to update.</param><param name="prefix">The prefix to use when looking up values in the value provider.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal void UpdateModel<TModel>(TModel model, string prefix, IValueProvider valueProvider) where TModel : class
    {
      this.UpdateModel<TModel>(model, prefix, (string[]) null, (string[]) null, valueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider, a prefix, and a list of properties to include.
    /// </summary>
    /// <param name="model">The model instance to update.</param><param name="includeProperties">A list of properties of the model to update.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal void UpdateModel<TModel>(TModel model, string[] includeProperties, IValueProvider valueProvider) where TModel : class
    {
      this.UpdateModel<TModel>(model, (string) null, includeProperties, (string[]) null, valueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider, a prefix, and a list of properties to include.
    /// </summary>
    /// <param name="model">The model instance to update.</param><param name="prefix">The prefix to use when looking up values in the value provider.</param><param name="includeProperties">A list of properties of the model to update.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal void UpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, IValueProvider valueProvider) where TModel : class
    {
      this.UpdateModel<TModel>(model, prefix, includeProperties, (string[]) null, valueProvider);
    }

    /// <summary>
    /// Updates the specified model instance using values from the value provider, a prefix, a list of properties to exclude, and a list of properties to include.
    /// </summary>
    /// <param name="model">The model instance to update.</param><param name="prefix">The prefix to use when looking up values in the value provider.</param><param name="includeProperties">A list of properties of the model to update.</param><param name="excludeProperties">A list of properties to explicitly exclude from the update. These are excluded even if they are listed in the <paramref name="includeProperties"/> parameter list.</param><param name="valueProvider">A dictionary of values that is used to update the model.</param><typeparam name="TModel">The type of the model object.</typeparam>
    protected internal void UpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties, IValueProvider valueProvider) where TModel : class
    {
      if (this.TryUpdateModel<TModel>(model, prefix, includeProperties, excludeProperties, valueProvider))
        return;
      throw new InvalidOperationException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, MvcResources.Controller_UpdateModel_UpdateUnsuccessful, new object[1]
      {
        (object) typeof (TModel).FullName
      }));
    }

    /// <summary>
    /// Validates the specified model instance.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    protected internal void ValidateModel(object model)
    {
      this.ValidateModel(model, (string) null);
    }

    /// <summary>
    /// Validates the specified model instance using an HTML prefix.
    /// </summary>
    /// <param name="model">The model to validate.</param><param name="prefix">The prefix to use when looking up values in the model provider.</param>
    protected internal void ValidateModel(object model, string prefix)
    {
      if (this.TryValidateModel(model, prefix))
        return;
      throw new InvalidOperationException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, MvcResources.Controller_Validate_ValidationFailed, new object[1]
      {
        (object) model.GetType().FullName
      }));
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.ViewResult"/> object that renders a view to the response.
    /// </summary>
    /// 
    /// <returns>
    /// The view result that renders a view to the response.
    /// </returns>
    protected internal ViewResult View()
    {
      Controller controller = this;
      string str1 = (string) null;
      string str2 = (string) null;
      object obj = (object) null;
      string viewName = str1;
      string masterName = str2;
      object model = obj;
      return controller.View(viewName, masterName, model);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.ViewResult"/> object by using the model that renders a view to the response.
    /// </summary>
    /// 
    /// <returns>
    /// The view result.
    /// </returns>
    /// <param name="model">The model that is rendered by the view.</param>
    protected internal ViewResult View(object model)
    {
      return this.View((string) null, (string) null, model);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.ViewResult"/> object by using the view name that renders a view.
    /// </summary>
    /// 
    /// <returns>
    /// The view result.
    /// </returns>
    /// <param name="viewName">The name of the view that is rendered to the response.</param>
    protected internal ViewResult View(string viewName)
    {
      Controller controller = this;
      string str = (string) null;
      object obj = (object) null;
      string viewName1 = viewName;
      string masterName = str;
      object model = obj;
      return controller.View(viewName1, masterName, model);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.ViewResult"/> object using the view name and master-page name that renders a view to the response.
    /// </summary>
    /// 
    /// <returns>
    /// The view result.
    /// </returns>
    /// <param name="viewName">The name of the view that is rendered to the response.</param><param name="masterName">The name of the master page or template to use when the view is rendered.</param>
    protected internal ViewResult View(string viewName, string masterName)
    {
      return this.View(viewName, masterName, (object) null);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.ViewResult"/> object by using the view name and model that renders a view to the response.
    /// </summary>
    /// 
    /// <returns>
    /// The view result.
    /// </returns>
    /// <param name="viewName">The name of the view that is rendered to the response.</param><param name="model">The model that is rendered by the view.</param>
    protected internal ViewResult View(string viewName, object model)
    {
      return this.View(viewName, (string) null, model);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.ViewResult"/> object using the view name, master-page name, and model that renders a view.
    /// </summary>
    /// 
    /// <returns>
    /// The view result.
    /// </returns>
    /// <param name="viewName">The name of the view that is rendered to the response.</param><param name="masterName">The name of the master page or template to use when the view is rendered.</param><param name="model">The model that is rendered by the view.</param>
    protected internal virtual ViewResult View(string viewName, string masterName, object model)
    {
      if (model != null)
        this.ViewData.Model = model;
      ViewResult viewResult = new ViewResult();
      viewResult.ViewName = viewName;
      viewResult.MasterName = masterName;
      viewResult.ViewData = this.ViewData;
      viewResult.TempData = this.TempData;
      viewResult.ViewEngineCollection = this.ViewEngineCollection;
      return viewResult;
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.ViewResult"/> object that renders the specified <see cref="T:System.Web.Mvc.IView"/> object.
    /// </summary>
    /// 
    /// <returns>
    /// The view result.
    /// </returns>
    /// <param name="view">The view that is rendered to the response.</param>
    protected internal ViewResult View(IView view)
    {
      return this.View(view, (object) null);
    }

    /// <summary>
    /// Creates a <see cref="T:System.Web.Mvc.ViewResult"/> object that renders the specified <see cref="T:System.Web.Mvc.IView"/> object.
    /// </summary>
    /// 
    /// <returns>
    /// The view result.
    /// </returns>
    /// <param name="view">The view that is rendered to the response.</param><param name="model">The model that is rendered by the view.</param>
    protected internal virtual ViewResult View(IView view, object model)
    {
      if (model != null)
        this.ViewData.Model = model;
      ViewResult viewResult = new ViewResult();
      viewResult.View = view;
      viewResult.ViewData = this.ViewData;
      viewResult.TempData = this.TempData;
      return viewResult;
    }

    IAsyncResult IAsyncController.BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
    {
      return this.BeginExecute(requestContext, callback, state);
    }

    void IAsyncController.EndExecute(IAsyncResult asyncResult)
    {
      this.EndExecute(asyncResult);
    }

    /// <summary>
    /// Begins execution of the specified request context
    /// </summary>
    /// 
    /// <returns>
    /// Returns an IAsyncController instance.
    /// </returns>
    /// <param name="requestContext">The request context.</param><param name="callback">The callback.</param><param name="state">The state.</param>
    protected virtual IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
    {
      if (this.DisableAsyncSupport)
      {
        Action action = (Action) (() => this.Execute(requestContext));
        return AsyncResultWrapper.BeginSynchronous(callback, state, action, Controller._executeTag);
      }
      else
      {
        if (requestContext == null)
          throw new ArgumentNullException("requestContext");
        this.VerifyExecuteCalledOnce();
        this.Initialize(requestContext);
        return AsyncResultWrapper.Begin(callback, state, new BeginInvokeDelegate(this.BeginExecuteCore), new EndInvokeDelegate(this.EndExecuteCore), Controller._executeTag);
      }
    }

    /// <summary>
    /// Begins to invoke the action in the current controller context.
    /// </summary>
    /// 
    /// <returns>
    /// Returns an IAsyncController instance.
    /// </returns>
    /// <param name="callback">The callback.</param><param name="state">The state.</param>
    protected virtual IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
    {
      this.PossiblyLoadTempData();
      try
      {
        string actionName = this.RouteData.GetRequiredString("action");
        IActionInvoker invoker = this.ActionInvoker;
        IAsyncActionInvoker asyncInvoker = invoker as IAsyncActionInvoker;
        if (asyncInvoker != null)
        {
          BeginInvokeDelegate beginDelegate = (BeginInvokeDelegate) ((asyncCallback, asyncState) => asyncInvoker.BeginInvokeAction(this.ControllerContext, actionName, asyncCallback, asyncState));
          EndInvokeDelegate endDelegate = (EndInvokeDelegate) (asyncResult =>
          {
            if (asyncInvoker.EndInvokeAction(asyncResult))
              return;
            this.HandleUnknownAction(actionName);
          });
          return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, Controller._executeCoreTag);
        }
        else
        {
          Action action = (Action) (() =>
          {
            if (invoker.InvokeAction(this.ControllerContext, actionName))
              return;
            this.HandleUnknownAction(actionName);
          });
          return AsyncResultWrapper.BeginSynchronous(callback, state, action, Controller._executeCoreTag);
        }
      }
      catch
      {
        this.PossiblySaveTempData();
        throw;
      }
    }

    /// <summary>
    /// Ends the  invocation of  the action in the current controller context.
    /// </summary>
    /// <param name="asyncResult">The asynchronous result.</param>
    protected virtual void EndExecute(IAsyncResult asyncResult)
    {
      AsyncResultWrapper.End(asyncResult, Controller._executeTag);
    }

    /// <summary>
    /// Ends the execute core.
    /// </summary>
    /// <param name="asyncResult">The asynchronous result.</param>
    protected virtual void EndExecuteCore(IAsyncResult asyncResult)
    {
      try
      {
        AsyncResultWrapper.End(asyncResult, Controller._executeCoreTag);
      }
      finally
      {
        this.PossiblySaveTempData();
      }
    }

    void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
    {
      this.OnActionExecuting(filterContext);
    }

    void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
    {
      this.OnActionExecuted(filterContext);
    }

    void IAuthorizationFilter.OnAuthorization(AuthorizationContext filterContext)
    {
      this.OnAuthorization(filterContext);
    }

    void IExceptionFilter.OnException(ExceptionContext filterContext)
    {
      this.OnException(filterContext);
    }

    void IResultFilter.OnResultExecuting(ResultExecutingContext filterContext)
    {
      this.OnResultExecuting(filterContext);
    }

    void IResultFilter.OnResultExecuted(ResultExecutedContext filterContext)
    {
      this.OnResultExecuted(filterContext);
    }
  }
}
