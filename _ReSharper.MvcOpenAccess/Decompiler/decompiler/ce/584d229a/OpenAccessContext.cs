// Type: Telerik.OpenAccess.OpenAccessContext
// Assembly: Telerik.OpenAccess.35.Extensions, Version=2012.3.1012.1, Culture=neutral, PublicKeyToken=7ce17eeaf1d59342
// Assembly location: D:\Projects\WebApi\MvcOpenAccess\MvcOpenAccess\bin\Telerik.OpenAccess.35.Extensions.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Telerik.OpenAccess.FetchOptimization;
using Telerik.OpenAccess.Metadata;
using Telerik.OpenAccess.Query;
using Telerik.OpenAccess.RT;
using Telerik.OpenAccess.SPI;
using Telerik.OpenAccess.SPI.dataobjects;

namespace Telerik.OpenAccess
{
  /// <summary>
  /// OpenAccess context class for .NET 3.5 usage.
  /// 
  /// </summary>
  public class OpenAccessContext : OpenAccessContextBase
  {
    private FetchStrategy fetchStrategy;

    /// <summary>
    /// Controls the fetch strategy for this context.
    /// 
    /// </summary>
    /// 
    /// <remarks>
    /// The fetch strategy allows to control the amount of data that is prefetched by execution
    ///             of queries.
    /// 
    /// </remarks>
    /// 
    /// <value>
    /// Fetch strategy instance
    /// </value>
    public FetchStrategy FetchStrategy
    {
      get
      {
        return this.fetchStrategy;
      }
      set
      {
        if (value == null)
          return;
        IExtendedObjectScope scope = this.GetScope() as IExtendedObjectScope;
        FetchPlan fetchPlan = value.CreateFetchPlan(scope);
        scope.FetchPlan = fetchPlan;
        this.fetchStrategy = value;
      }
    }

    /// <summary>
    /// OpenAccessContext Constructor with MetadataSource
    /// 
    /// </summary>
    /// <param name="connectionString">The connection string name or the connection string. This is a required parameter</param><param name="backendConfiguration">A backend configuration instance. If the parameter is null the default settings are used.
    ///             For some backends the backend type cannot be derived from the connection string, the backend type must be set.
    ///             </param><param name="metadataSource">A metadata source. If non is specified the metadata is derived from the context itself.</param>
    public OpenAccessContext(string connectionString, BackendConfiguration backendConfiguration, MetadataSource metadataSource)
      : base(connectionString, backendConfiguration, metadataSource, metadataSource == null ? Assembly.GetCallingAssembly() : (Assembly) null)
    {
    }

    /// <summary>
    /// OpenAccessContext Constructor with MetadataContainer
    /// 
    /// </summary>
    /// <param name="connectionString">The connection string name or the connection string. This is a required parameter</param><param name="backendConfiguration">A backend configuration instance. If the parameter is null the default settings are used.
    ///             For some backends the backend type cannot be derived from the connection string. The backend type must be set then.
    ///             </param><param name="metadataContainer">A metadata container. If non is specified the metadata is derived from the context itself.</param>
    public OpenAccessContext(string connectionString, BackendConfiguration backendConfiguration, MetadataContainer metadataContainer)
      : base(connectionString, backendConfiguration, metadataContainer, metadataContainer == null ? Assembly.GetCallingAssembly() : (Assembly) null)
    {
    }

    /// <summary>
    /// OpenAccessContext Constructor with MetadataContainer
    /// 
    /// </summary>
    /// <param name="connectionString">The connection string name or the connection string. This is a required parameter</param><param name="backendConfiguration">A backend configuration instance. If the parameter is null the default settings are used.
    ///             For some backends the backend type cannot be derived from the connection string. The backend type must be set then.
    ///             </param><param name="metadataContainer">A metadata container. If non is specified the metadata is derived from the context itself.</param><param name="callingAssembly">The assembly to search for the attribute mapping if no metadata is specified.</param>
    protected OpenAccessContext(string connectionString, BackendConfiguration backendConfiguration, MetadataContainer metadataContainer, Assembly callingAssembly)
      : base(connectionString, backendConfiguration, metadataContainer, callingAssembly)
    {
    }

    /// <summary>
    /// OpenAccessContext Constructor with MetadataSource
    /// 
    /// </summary>
    /// <param name="connectionString">The connection string name or the connection string. This is a required parameter</param><param name="backendConfiguration">A backend configuration instance. If the parameter is null the default settings are used.
    ///             For some backends the backend type cannot be derived from the connection string. The backend type must be set then.
    ///             </param><param name="metadataSource">A metadata source. If non is specified the metadata is derived from the context itself.</param><param name="callingAssembly">The assembly to search for the attribute mapping if no metadata is specified.</param>
    protected OpenAccessContext(string connectionString, BackendConfiguration backendConfiguration, MetadataSource metadataSource, Assembly callingAssembly)
      : base(connectionString, backendConfiguration, metadataSource, callingAssembly)
    {
    }

    /// <summary>
    /// Copy constructor, the same database connection and configuration will be used.
    /// 
    /// </summary>
    /// <param name="otherContext">An existing not disposed context</param>
    public OpenAccessContext(OpenAccessContextBase otherContext)
      : base(otherContext)
    {
    }

    /// <summary>
    /// Provides an IQueryable instance usable for Linq queries.
    /// 
    /// </summary>
    /// 
    /// <remarks>
    /// This is the main entry point for constructing LINQ queries with OpenAccess.
    /// 
    /// </remarks>
    /// <typeparam name="T">The type of the persistent objects that should be queried.</typeparam>
    /// <returns>
    /// IQueryable instance that can be used to express queries.
    /// </returns>
    /// <seealso cref="F:ExtensionMethods.Matches"/>
    public IQueryable<T> GetAll<T>()
    {
      return this.GetAllCore<T>();
    }

    /// <summary>
    /// Provides an IQueryable instance usable for Linq queries.
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the persistent objects that should be queried.</typeparam>
    /// <returns>
    /// IQueryable instance that can be used to express queries.
    /// </returns>
    protected virtual IQueryable<T> GetAllCore<T>()
    {
      return (IQueryable<T>) ExtensionMethods.Extent<T>(this.GetScope()).ParallelFetch(true);
    }

    /// <summary>
    /// Overwrite to free additional resources
    /// 
    /// </summary>
    /// <param name="disposing">If true dispose is executed, if false nothing is done.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        this.FetchStrategy = (FetchStrategy) null;
      base.Dispose(disposing);
    }

    /// <summary>
    /// Creates and returns a detached copy of the persistent capable object that loads all the fields in accordance to the given fetch strategy.
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the persistent capable object.</typeparam><param name="entity">The persistent capable object.</param><param name="fetchStrategy">The fetch strategy to be used.</param>
    /// <returns>
    /// A copy of the persistent capable object that is detached.
    /// </returns>
    public T CreateDetachedCopy<T>(T entity, FetchStrategy fetchStrategy)
    {
      if ((object) entity == null)
        throw new ArgumentNullException("entity");
      if (!((object) entity is PersistenceCapable))
      {
        if ((object) entity is Array)
        {
          Type elementType = entity.GetType().GetElementType();
          if (typeof (PersistenceCapable).IsAssignableFrom(elementType))
          {
            ArrayList detachedCopies = this.CreateDetachedCopies((IEnumerable) (object) entity, fetchStrategy);
            object obj = (object) Array.CreateInstance(elementType, detachedCopies.Count);
            detachedCopies.CopyTo((Array) obj, 0);
            return (T) obj;
          }
        }
        else if ((object) entity is IEnumerable)
        {
          Type elementType = Telerik.OpenAccess.RT.TypeResolver.GetElementType(entity.GetType());
          if (elementType != null && typeof (PersistenceCapable).IsAssignableFrom(elementType))
          {
            ArrayList detachedCopies = this.CreateDetachedCopies((IEnumerable) (object) entity, fetchStrategy);
            Array instance = Array.CreateInstance(elementType, detachedCopies.Count);
            detachedCopies.CopyTo(instance, 0);
            return (T) Activator.CreateInstance(entity.GetType(), new object[1]
            {
              (object) instance
            });
          }
        }
      }
      IExtendedObjectScope scope = this.GetScope() as IExtendedObjectScope;
      ArrayList c = new ArrayList(1);
      c.Add((object) entity);
      scope.CreateDetachedCopies(c, fetchStrategy == null ? (FetchPlan) null : fetchStrategy.CreateFetchPlan(scope));
      return (T) c[0];
    }

    private ArrayList CreateDetachedCopies(IEnumerable entities, FetchStrategy fetchStrategy)
    {
      IExtendedObjectScope scope = this.GetScope() as IExtendedObjectScope;
      ArrayList c = new ArrayList(1);
      foreach (object obj in entities)
        c.Add(obj);
      scope.CreateDetachedCopies(c, fetchStrategy == null ? (FetchPlan) null : fetchStrategy.CreateFetchPlan(scope));
      return c;
    }

    /// <summary>
    /// Creates and returns a detached collection of persistent capable objects that are loaded in accordance to the given fetch strategy.
    /// 
    /// </summary>
    /// 
    /// <example>
    /// The following example retrieves a collection of employee objects from the Nortwhind database and uses the CreateDetachedCopy method
    ///             to create a detached copy of the object. The employeeCopy object will not be tracked by OpenAccess and any changes
    ///             made to it will not be persisted.
    /// 
    /// <code>
    /// Northwind.Employee employee = context.GetAll&lt;Northwind.Employee&gt;().Single(x =&gt; x.Id == 1);
    ///                 Northwind.Employee employeeCopy = context.CreateDetachedCopy&lt;Northwind.Employee&gt;(employee, x =&gt; x.Orders, x =&gt; x.ReportsTo);
    /// 
    /// </code>
    /// 
    /// </example>
    /// <typeparam name="T">The type of the persistent capable objects.</typeparam><param name="entities">The collection of persistent capable objects.</param><param name="fetchStrategy">The fetch strategy to be used.</param>
    /// <returns>
    /// A collection of copied persistent capable objects that are detached.
    /// </returns>
    public IEnumerable<T> CreateDetachedCopy<T>(IEnumerable<T> entities, FetchStrategy fetchStrategy)
    {
      if (entities == null)
        throw new ArgumentNullException("entities");
      ArrayList detachedCopies = this.CreateDetachedCopies((IEnumerable) entities, fetchStrategy);
      T[] objArray = new T[detachedCopies.Count];
      detachedCopies.CopyTo((Array) objArray);
      return (IEnumerable<T>) objArray;
    }

    /// <summary>
    /// Creates and returns a detached copy of the persistent capable object that includes the specified reference properties.
    /// 
    /// </summary>
    /// 
    /// <example>
    /// The following example retrieves a single employee object from the Nortwhind database and uses the CreateDetachedCopy method
    ///             to create a detached copy of the object. The employeeCopy object will not be tracked by OpenAccess and any changes
    ///             made to it will not be persisted.
    /// 
    /// <code>
    /// Northwind.Employee employee = context.GetAll&lt;Northwind.Employee&gt;().Single(x =&gt; x.Id == 1);
    ///                 Northwind.Employee employeeCopy = context.CreateDetachedCopy&lt;Northwind.Employee&gt;(employee, x =&gt; x.Orders, x =&gt; x.ReportsTo);
    /// 
    /// </code>
    /// 
    /// </example>
    /// <typeparam name="T">The type of the persistent capable object.</typeparam><param name="entity">The persistent capable object.</param><param name="referenceProperties">The reference properties to be included when creating the copy.</param>
    /// <returns>
    /// A copy of the persistent capable object that is detached.
    /// </returns>
    public T CreateDetachedCopy<T>(T entity, params Expression<Func<T, object>>[] referenceProperties)
    {
      if ((object) entity == null)
        throw new ArgumentNullException("entity");
      if (referenceProperties == null || referenceProperties.Length == 0)
        return this.CreateDetachedCopy<T>(entity, (FetchStrategy) null);
      FetchStrategyBuilder fetchStrategyBuilder = new FetchStrategyBuilder();
      foreach (LambdaExpression lambda in referenceProperties)
      {
        if (lambda != null)
          fetchStrategyBuilder.Add(lambda);
      }
      IExtendedObjectScope sc = this.GetScope() as IExtendedObjectScope;
      FetchPlan fetch = fetchStrategyBuilder.ToFetchPlan(sc);
      ArrayList c = new ArrayList(1);
      c.Add((object) entity);
      sc.CreateDetachedCopies(c, fetch);
      return (T) c[0];
    }
  }
}
