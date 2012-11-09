// Type: Telerik.OpenAccess.OpenAccessContextBase
// Assembly: Telerik.OpenAccess, Version=2012.3.1012.1, Culture=neutral, PublicKeyToken=7ce17eeaf1d59342
// Assembly location: C:\Windows\assembly\GAC_MSIL\Telerik.OpenAccess\2012.3.1012.1__7ce17eeaf1d59342\Telerik.OpenAccess.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Telerik.OpenAccess.Data.Common;
using Telerik.OpenAccess.Exceptions;
using Telerik.OpenAccess.Metadata;
using Telerik.OpenAccess.RT;
using Telerik.OpenAccess.SPI;
using Telerik.OpenAccess.SPI.dataobjects;
using Telerik.OpenAccess.SPI.Util;

namespace Telerik.OpenAccess
{
  /// <summary>
  /// OpenAccess context base class for .NET 2.0 usage.
  /// 
  /// </summary>
  public class OpenAccessContextBase : ILevelOneCache, ILevelTwoCache, IDisposable, IOpenAccessContextOptions
  {
    private static readonly PersistenceStateHelper persistenceState = new PersistenceStateHelper();
    private string connectionString;
    private BackendConfiguration backendConfiguration;
    private MetadataContainer metadataContainer;
    private Database database;
    private IExtendedObjectScope objectScope;
    private bool disposed;
    private string name;

    /// <summary>
    /// Returns true if the context contains changes.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// True if the context contains changes.
    /// </value>
    public bool HasChanges
    {
      get
      {
        return this.GetScope().Transaction.IsDirty;
      }
    }

    /// <summary>
    /// Gets helper instance to obtain the state information for persistent objects.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// Instance to get state information from.
    /// </value>
    public static PersistenceStateHelper PersistenceState
    {
      get
      {
        return OpenAccessContextBase.persistenceState;
      }
    }

    /// <summary>
    /// Gets the metadata tree for this context.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// Returns the metadata tree for the actual database connection.
    /// </returns>
    public MetadataContainer Metadata
    {
      get
      {
        this.CheckNotDisposed();
        if (this.database == null)
        {
          Database database = (Database) null;
          try
          {
            database = Database.Get(this.connectionString, this.backendConfiguration, this.metadataContainer, new DBRegistry.OpenNotification(this.PrepareOpenMeta));
            return database.MetaData;
          }
          finally
          {
            if (database != null && !database.IsCached())
              database.Dispose();
          }
        }
        else
        {
          this.GetScope();
          return this.objectScope.GetMetaData();
        }
      }
    }

    /// <summary>
    /// Specifies the destination to write the SQL query or command.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// TextWriter instance where this context writes events to.
    /// </value>
    public TextWriter Log
    {
      get
      {
        return this.GetScope().Log;
      }
      set
      {
        this.GetScope().Log = value;
      }
    }

    /// <summary>
    /// Gets the Telerik.OpenAccess.OpenAccessContextOptions instance
    ///             that contains options that affect the behavior of the Telerik.OpenAccess.OpenAccessContextBase.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// The <see cref="T:Telerik.OpenAccess.OpenAccessContextOptions"/> instance
    ///             that contains options that affect the behavior of the <see cref="T:Telerik.OpenAccess.OpenAccessContextBase"/>.
    /// 
    /// </returns>
    public IOpenAccessContextOptions ContextOptions
    {
      get
      {
        return (IOpenAccessContextOptions) this;
      }
    }

    bool IOpenAccessContextOptions.EnableDataSynchronization
    {
      get
      {
        return this.GetScope().TransactionProperties.EnableDataSynchronization;
      }
      set
      {
        this.GetScope().TransactionProperties.EnableDataSynchronization = value;
      }
    }

    bool IOpenAccessContextOptions.MaintainOriginalValues
    {
      get
      {
        this.GetScope();
        return this.objectScope.MaintainOriginalValues;
      }
      set
      {
        this.GetScope();
        this.objectScope.MaintainOriginalValues = value;
      }
    }

    bool IOpenAccessContextOptions.RefreshObjectsAfterSaveChanges { get; set; }

    /// <summary>
    /// Gets or sets an application specific name that can be used for correlation.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// Application specific string or <c>null</c>
    /// </value>
    public string Name
    {
      get
      {
        return this.name;
      }
      set
      {
        if (string.Equals(this.name, value))
          return;
        if (this.objectScope != null)
          this.objectScope.Correlate(value);
        this.name = value;
      }
    }

    /// <summary>
    /// Controls if the context prevents changes to any data it is managing.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// Controls if the context prevents changes to any data it is managing.
    /// </value>
    public bool ReadOnly
    {
      get
      {
        return this.GetScope().Transaction.Level == 0;
      }
      set
      {
        (this.GetScope() as IExtendedObjectScope).MakeReadOnly(value);
      }
    }

    /// <summary>
    /// Gets the connection used by the OpenAccess context.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// Connection that can be used to perform ADO operations
    /// </value>
    public OAConnection Connection
    {
      get
      {
        bool isNew = false;
        return this.GetConnection(out isNew);
      }
    }

    /// <summary>
    /// Returns the level one cache directly bound to this context.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// The level one cache directly bound to this context.
    /// </value>
    public ILevelOneCache Cache
    {
      get
      {
        return (ILevelOneCache) this;
      }
    }

    /// <summary>
    /// Returns the level two cache bound to all contexts using the same connection string.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// The level two cache bound to all contexts using the same connection string.
    /// </value>
    public ILevelTwoCache LevelTwoCache
    {
      get
      {
        return (ILevelTwoCache) this;
      }
    }

    /// <summary>
    /// Gets the associated change tracking instance.
    /// 
    /// </summary>
    /// 
    /// <value>
    /// The associated tracking instance.
    /// </value>
    public IContextEvents Events
    {
      get
      {
        return (IContextEvents) this.GetScope().Tracking;
      }
    }

    static OpenAccessContextBase()
    {
    }

    /// <summary>
    /// OpenAccessContextBase Constructor with MetadataSource
    /// 
    /// </summary>
    /// <param name="connectionString">The connection string name or the connection string. This is a required parameter</param><param name="backendConfiguration">A backend configuration instance. If the parameter is null the default settings are used.
    ///             For some backends the backend type cannot be derived from the connection string, the backend type must be set.
    ///             </param><param name="metadataSource">A metadata source. If non is specified the metadata is derived from the context itself.</param>
    public OpenAccessContextBase(string connectionString, BackendConfiguration backendConfiguration, MetadataSource metadataSource)
      : this(connectionString, backendConfiguration, OpenAccessContextBase.GetMetadataContainerFromSource(metadataSource))
    {
    }

    /// <summary>
    /// OpenAccessContextBase Constructor with MetadataContainer
    /// 
    /// </summary>
    /// <param name="connectionString">The connection string name or the connection string. This is a required parameter</param><param name="backendConfiguration">A backend configuration instance. If the parameter is null the default settings are used.
    ///             For some backends the backend type cannot be derived from the connection string, the backend type must be set.
    ///             </param><param name="metadataContainer">A metadata container. If non is specified the metadata is derived from the context itself.</param>
    public OpenAccessContextBase(string connectionString, BackendConfiguration backendConfiguration, MetadataContainer metadataContainer)
      : this(connectionString, backendConfiguration, metadataContainer, metadataContainer == null ? Assembly.GetCallingAssembly() : (Assembly) null)
    {
    }

    /// <summary>
    /// OpenAccessContextBase Constructor with MetadataContainer
    /// 
    /// </summary>
    /// <param name="connectionString">The connection string name or the connection string. This is a required parameter</param><param name="backendConfiguration">A backend configuration instance. If the parameter is null the default settings are used.
    ///             For some backends the backend type cannot be derived from the connection string. The backend type must be set then.
    ///             </param><param name="metadataContainer">A metadata container. If non is specified the metadata is derived from the context itself.</param><param name="callingAssembly">The assembly to search for the attribute mapping if no metadata is specified.</param>
    protected OpenAccessContextBase(string connectionString, BackendConfiguration backendConfiguration, MetadataContainer metadataContainer, Assembly callingAssembly)
    {
      this.Init(connectionString, backendConfiguration, metadataContainer, callingAssembly);
    }

    /// <summary>
    /// OpenAccessContextBase Constructor with MetadataSource
    /// 
    /// </summary>
    /// <param name="connectionString">The connection string name or the connection string. This is a required parameter</param><param name="backendConfiguration">A backend configuration instance. If the parameter is null the default settings are used.
    ///             For some backends the backend type cannot be derived from the connection string. The backend type must be set then.
    ///             </param><param name="metadataSource">A metadata source. If non is specified the metadata is derived from the context itself.</param><param name="callingAssembly">The assembly to search for the attribute mapping if no metadata is specified.</param>
    protected OpenAccessContextBase(string connectionString, BackendConfiguration backendConfiguration, MetadataSource metadataSource, Assembly callingAssembly)
      : this(connectionString, backendConfiguration, OpenAccessContextBase.GetMetadataContainerFromSource(metadataSource), callingAssembly)
    {
    }

    /// <summary>
    /// Copy constructor, the same database connection and configuration will be used.
    /// 
    /// </summary>
    /// <param name="otherContext">An existing not disposed context</param>
    public OpenAccessContextBase(OpenAccessContextBase otherContext)
    {
      this.connectionString = otherContext.connectionString;
      this.backendConfiguration = otherContext.backendConfiguration;
      this.metadataContainer = otherContext.metadataContainer;
      this.database = otherContext.database;
      this.GetScope();
      IOpenAccessContextOptions accessContextOptions1 = (IOpenAccessContextOptions) this;
      IOpenAccessContextOptions accessContextOptions2 = (IOpenAccessContextOptions) otherContext;
      accessContextOptions1.EnableDataSynchronization = accessContextOptions2.EnableDataSynchronization;
      accessContextOptions1.MaintainOriginalValues = accessContextOptions2.MaintainOriginalValues;
      accessContextOptions1.RefreshObjectsAfterSaveChanges = accessContextOptions2.RefreshObjectsAfterSaveChanges;
    }

    /// <summary>
    /// Initializes this context instance.
    /// 
    /// </summary>
    /// <param name="connectionString">A connections string.</param><param name="backendConfiguration">A BackendConfiguration instance.</param><param name="metadataContainer">A MetadataContainer instance.</param><param name="callingAssembly">The calling assembly.</param>
    protected virtual void Init(string connectionString, BackendConfiguration backendConfiguration, MetadataContainer metadataContainer, Assembly callingAssembly)
    {
      this.Init(connectionString, backendConfiguration, metadataContainer);
      if (this.metadataContainer != null)
        return;
      this.metadataContainer = OpenAccessContextBase.GetMetadataContainerFromAssembly(callingAssembly, this.GetType());
    }

    /// <summary>
    /// Initializes this context instance.
    /// 
    /// </summary>
    /// <param name="connectionString">A connections string.</param><param name="backendConfiguration">A BackendConfiguration instance.</param><param name="metadataContainer">A MetadataContainer instance.</param>
    protected virtual void Init(string connectionString, BackendConfiguration backendConfiguration, MetadataContainer metadataContainer)
    {
      if (string.IsNullOrEmpty(connectionString))
        throw new ArgumentNullException("connectionString", "Connection string has to be specified.");
      this.connectionString = connectionString;
      this.backendConfiguration = backendConfiguration;
      this.metadataContainer = metadataContainer;
      if (this.backendConfiguration == null)
        this.backendConfiguration = new BackendConfiguration();
      this.RefreshObjectsAfterSaveChanges = true;
    }

    private static MetadataContainer GetMetadataContainerFromSource(MetadataSource metadataSource)
    {
      MetadataContainer metadataContainer = (MetadataContainer) null;
      if (metadataSource != null)
        metadataContainer = metadataSource.GetModel();
      return metadataContainer;
    }

    private static MetadataContainer GetMetadataContainerFromAssembly(Assembly assembly, Type contextType)
    {
      string fullName = contextType.Assembly.FullName;
      return (fullName.Equals("Telerik.OpenAccess") || fullName.Equals("Telerik.OpenAccess.35.Extensions") ? (MetadataSource) AttributesMetadataSource.FromAssembly(assembly) : (MetadataSource) AttributesMetadataSource.FromContext(contextType)).GetModel();
    }

    private static PersistenceCapable GetPersistenceCapable(object entity)
    {
      if (entity == null)
        throw new ArgumentNullException("entity");
      else
        return entity as PersistenceCapable;
    }

    /// <summary>
    /// Replaces the metadata definition with a running context instance used for new instances.
    /// 
    /// </summary>
    /// <param name="context">The context that has initiated the mapping update.</param><param name="newMetadata">Metadata definition to use for new contextes.</param><param name="callback">A callback to integrate schema migration tasks if necessary.</param>
    public static void ReplaceMetadata(OpenAccessContextBase context, MetadataContainer newMetadata, SchemaUpdateCallback callback)
    {
      Database.ReplaceMetadata(context.GetScope(), newMetadata, callback);
    }

    /// <summary>
    /// Saves all changes in the context
    /// 
    /// </summary>
    public void SaveChanges()
    {
      this.SaveChanges(ConcurrencyConflictsProcessingMode.StopOnFirst);
    }

    /// <summary>
    /// Saves the changes with the specified concurency mode
    /// 
    /// </summary>
    /// <param name="failureMode">Mode to use</param>
    public virtual void SaveChanges(ConcurrencyConflictsProcessingMode failureMode)
    {
      IExtendedObjectScope extendedObjectScope = (IExtendedObjectScope) this.GetScope();
      if (failureMode == ConcurrencyConflictsProcessingMode.AggregateAll)
        extendedObjectScope.TransactionProperties.FailFast = false;
      else
        extendedObjectScope.TransactionProperties.FailFast = true;
      extendedObjectScope.TransactionProperties.RefreshReadObjectsInNewTransaction = this.RefreshObjectsAfterSaveChanges;
      extendedObjectScope.CommitChanges();
    }

    /// <summary>
    /// Rolls back all changes in the context
    /// 
    /// </summary>
    public void ClearChanges()
    {
      IExtendedObjectScope extendedObjectScope = (IExtendedObjectScope) this.GetScope();
      extendedObjectScope.TransactionProperties.RefreshReadObjectsInNewTransaction = this.RefreshObjectsAfterSaveChanges;
      extendedObjectScope.ClearChanges();
    }

    /// <summary>
    /// Flushes all current changes to the database but keeps the transaction running.
    /// 
    /// </summary>
    public void FlushChanges()
    {
      this.FlushChanges(false);
    }

    /// <summary>
    /// Flushes all current changes to the database but keeps the transaction running.
    /// 
    /// </summary>
    /// <param name="releaseMemory">Clean up the used memory of the participating objects.</param>
    public void FlushChanges(bool releaseMemory)
    {
      ((IExtendedObjectScope) this.GetScope()).FlushChanges(releaseMemory);
    }

    /// <summary>
    /// Returns all objects that will be inserted, deleted or updated during the next commit.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// <see cref="T:Telerik.OpenAccess.ContextChanges"/> to access all pending inserts, deletes or updates.
    /// </returns>
    public ContextChanges GetChanges()
    {
      return new ContextChanges((IObjectContext) this.GetScope());
    }

    /// <summary>
    /// Gets schema handling instance. Must be called and used before any object scope is obtained.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// Schema Handler instance
    /// </returns>
    public ISchemaHandler GetSchemaHandler()
    {
      return this.GetDatabase().GetSchemaHandler();
    }

    /// <summary>
    /// Executes a stored procedure
    /// 
    /// </summary>
    /// <typeparam name="T">The return type, can be a persistent class or 'Object' or Object[]</typeparam><param name="procedureName">The name of the stored procedure, can be delimited if necessary.</param><param name="parameterDefinition">A list of <see cref="C:SqlParameter"/> defining the stored procedure parameter.</param><param name="parameterValues">The parameter values.</param>
    /// <returns>
    /// List of persistent classes or a IList&lt;object&gt; or a IList&lt;object[]&gt;
    /// </returns>
    public T[] ExecuteStoredProcedure<T>(string procedureName, IEnumerable<SqlParameter> parameterDefinition, params object[] parameterValues)
    {
      return ((IExtendedObjectScope) this.GetScope()).ExecuteStoredProcedure<T>(procedureName, parameterDefinition, parameterValues);
    }

    /// <summary>
    /// Executes a stored procedure
    /// 
    /// </summary>
    /// <typeparam name="T">The return type, can be a persistent class, 'Object' or Object[]</typeparam><param name="procedureName">The name of the stored procedure, can be delimited if necessary.</param><param name="parameterDefinition">A list of <see cref="C:SqlParameter"/> defining the stored procedure parameter.</param><param name="parameterValues">The parameter values.</param><param name="outParameter">If the stored procedure has out parameter the dictionary contains</param>
    /// <returns>
    /// List of persistent classes or a IList&lt;object&gt; or a IList&lt;object[]&gt;
    /// </returns>
    public T[] ExecuteStoredProcedure<T>(string procedureName, IEnumerable<SqlParameter> parameterDefinition, out IDictionary<string, object> outParameter, params object[] parameterValues)
    {
      return ((IExtendedObjectScope) this.GetScope()).ExecuteStoredProcedure<T>(procedureName, parameterDefinition, out outParameter, parameterValues);
    }

    /// <summary>
    /// Executes the command text using the context owned connection and materializes the results to instances of T.
    /// 
    /// </summary>
    /// <typeparam name="T">The type to materialize each row into</typeparam><param name="commandText">The text of the command to run against the data source.</param><param name="parameters">The parameters(<see cref="T:Telerik.OpenAccess.Data.Common.OAParameter"/> or <see cref="T:System.Data.Common.DbParameter"/>) required to execute the statement</param>
    /// <returns>
    /// A list of objects of type T
    /// </returns>
    public IList<T> ExecuteQuery<T>(string commandText, params DbParameter[] parameters)
    {
      return this.ExecuteQuery<T>(commandText, CommandType.Text, parameters);
    }

    /// <summary>
    /// Executes the command text using the context owned connection and materializes the results to instances of T.
    /// 
    /// </summary>
    /// <typeparam name="T">The type to materialize each row into</typeparam><param name="commandText">The text of the command to run against the data source.</param><param name="parameters">The parameters(<see cref="T:Telerik.OpenAccess.Data.Common.OAParameter"/> or <see cref="T:System.Data.Common.DbParameter"/>) required to execute the statement</param><param name="commandType">The command type</param>
    /// <returns>
    /// A list of objects of type T
    /// </returns>
    public IList<T> ExecuteQuery<T>(string commandText, CommandType commandType, params DbParameter[] parameters)
    {
      bool isNew = true;
      OAConnection oaConnection = (OAConnection) null;
      try
      {
        oaConnection = this.GetConnection(out isNew);
        if (oaConnection.State != ConnectionState.Open)
          throw new System.InvalidOperationException("You need an open connection to perform the requested operation");
        using (OACommand command = oaConnection.CreateCommand())
        {
          command.CommandText = commandText;
          command.CommandType = commandType;
          if (parameters != null && parameters.Length != 0)
            command.Parameters.AddRange((Array) parameters);
          ReadOnlyCollection<T> readOnlyCollection = (ReadOnlyCollection<T>) null;
          using (OADataReader oaDataReader = command.ExecuteReader())
          {
            if (oaDataReader == null)
              return (IList<T>) null;
            readOnlyCollection = new ReadOnlyCollection<T>((IList<T>) new List<T>(this.Translate<T>((DbDataReader) oaDataReader)));
          }
          command.Parameters.Clear();
          return (IList<T>) readOnlyCollection;
        }
      }
      finally
      {
        if (isNew && oaConnection != null)
          oaConnection.Dispose();
      }
    }

    /// <summary>
    /// Executes the query and returns the first column of the first row in the result set returned by the query.
    ///             All other columns and rows are ignored.
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the expected scalar value</typeparam><param name="commandText">The sql to execute</param><param name="parameters">The parameters required to execute the sql</param>
    /// <returns>
    /// The value in the first column of the first row in the result set
    /// </returns>
    public T ExecuteScalar<T>(string commandText, params DbParameter[] parameters)
    {
      return this.ExecuteScalar<T>(commandText, CommandType.Text, parameters);
    }

    /// <summary>
    /// Executes the query and returns the first column of the first row in the result set returned by the query.
    ///             All other columns and rows are ignored.
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the expected scalar value</typeparam><param name="commandText">The sql to execute</param><param name="commandType">The command type</param><param name="parameters">The parameters required to execute the sql</param>
    /// <returns>
    /// The value in the first column of the first row in the result set
    /// </returns>
    public T ExecuteScalar<T>(string commandText, CommandType commandType, params DbParameter[] parameters)
    {
      bool isNew = true;
      OAConnection oaConnection = (OAConnection) null;
      try
      {
        oaConnection = this.GetConnection(out isNew);
        if (oaConnection.State != ConnectionState.Open)
          throw new System.InvalidOperationException("You need an open connection to perform the requested operation");
        using (OACommand command = this.Connection.CreateCommand())
        {
          command.CommandText = commandText;
          command.CommandType = commandType;
          if (parameters != null && parameters.Length != 0)
            command.Parameters.AddRange((Array) parameters);
          object obj = command.ExecuteScalar();
          command.Parameters.Clear();
          if (obj == null || obj == DBNull.Value)
            return default (T);
          else
            return (T) obj;
        }
      }
      finally
      {
        if (isNew && oaConnection != null)
          oaConnection.Dispose();
      }
    }

    /// <summary>
    /// Executes a SQL statement using the context owned connection.
    /// 
    /// </summary>
    /// <param name="commandText">The text of the command to run against the data source.</param><param name="parameters">The parameters(<see cref="T:Telerik.OpenAccess.Data.Common.OAParameter"/> or <see cref="T:System.Data.Common.DbParameter"/>) required to execute the statement</param>
    /// <returns>
    /// The number of rows affected.
    /// </returns>
    public int ExecuteNonQuery(string commandText, params DbParameter[] parameters)
    {
      return this.ExecuteNonQuery(commandText, CommandType.Text, parameters);
    }

    /// <summary>
    /// Executes a SQL statement using the context owned connection.
    /// 
    /// </summary>
    /// <param name="commandText">The text of the command to run against the data source.</param><param name="commandType">The command type</param><param name="parameters">The parameters(<see cref="T:Telerik.OpenAccess.Data.Common.OAParameter"/> or <see cref="T:System.Data.Common.DbParameter"/>) required to execute the statement</param>
    /// <returns>
    /// The number of rows affected.
    /// </returns>
    public int ExecuteNonQuery(string commandText, CommandType commandType, params DbParameter[] parameters)
    {
      bool isNew = true;
      OAConnection oaConnection = (OAConnection) null;
      try
      {
        oaConnection = this.GetConnection(out isNew);
        if (oaConnection.State != ConnectionState.Open)
          throw new System.InvalidOperationException("You need an open connection to perform the requested operation");
        using (OACommand command = oaConnection.CreateCommand())
        {
          command.CommandText = commandText;
          command.CommandType = commandType;
          if (parameters != null && parameters.Length != 0)
            command.Parameters.AddRange((Array) parameters);
          int num = command.ExecuteNonQuery();
          command.Parameters.Clear();
          return num;
        }
      }
      finally
      {
        if (isNew && oaConnection != null)
          oaConnection.Dispose();
      }
    }

    /// <summary>
    /// Lazily materializes the current result of a DbDataReader into a specified type.
    /// 
    /// </summary>
    /// <typeparam name="T">Target type</typeparam><param name="reader">Source data reader</param>
    /// <returns>
    /// Materialized result
    /// </returns>
    public IEnumerable<T> Translate<T>(DbDataReader reader)
    {
      Primitives.CheckClosed(reader);
      IExtendedObjectScope extendedObjectScope = (IExtendedObjectScope) this.GetScope();
      IEnumerable<T> enumerable;
      if (!typeof (T).IsValueType && typeof (PersistenceCapable).IsAssignableFrom(typeof (T)))
      {
        enumerable = extendedObjectScope.MaterializeAttached<T>(reader);
      }
      else
      {
        OADataReader oaDataReader = reader as OADataReader;
        if (oaDataReader == null)
        {
          oaDataReader = new OADataReader((OACommand) null, reader);
          extendedObjectScope.SetupDataReader(oaDataReader);
        }
        enumerable = new NonPersistentCapableMaterializer<T>(oaDataReader).Materialize();
      }
      return (IEnumerable<T>) new TypedEnumerable<T>((IEnumerable) enumerable, (TypedEnumerable<T>.CheckBeforeEnumeration) (e => Primitives.CheckClosed(reader)));
    }

    /// <summary>
    /// Refreshes a collection of entity objects according to the specified mode.
    /// 
    /// </summary>
    /// <param name="mode">Defines if dirty fields stay dirty.</param><param name="entities">List of entities to be refreshed.</param>
    public void Refresh(RefreshMode mode, IEnumerable entities)
    {
      this.Refresh(mode, (object) entities);
    }

    /// <summary>
    /// Refreshes a number of entities according to the specified mode.
    /// 
    /// </summary>
    /// <param name="mode">Defines if dirty fields stay dirty.</param><param name="entities">List of entities to be refreshed.</param>
    public void Refresh(RefreshMode mode, params object[] entities)
    {
      if (entities == null)
        throw new ArgumentException("entities", "At least one entity has to be specified.");
      this.Refresh(mode, (object) entities);
    }

    /// <summary>
    /// Refreshes a collection of entity objects according to the specified mode.
    /// 
    /// </summary>
    /// <param name="mode">Defines if dirty fields stay dirty.</param><param name="entity">Entity to be refreshed.</param>
    public void Refresh(RefreshMode mode, object entity)
    {
      IObjectScope scope = this.GetScope();
      switch (mode)
      {
        case RefreshMode.PreserveChanges:
          scope.Retrieve(entity, new FetchPlan(new string[1]
          {
            "all"
          }, 1, -1));
          break;
        case RefreshMode.OverwriteChangesFromStore:
          scope.Refresh(entity);
          break;
        default:
          throw new ArgumentException("mode", "RefreshMode value not supported");
      }
    }

    /// <summary>
    /// Returns the persistent object defined by the key parameter.
    /// 
    /// </summary>
    /// <param name="key">The unique describing a persistent entity.</param><typeparam name="T">The expected object type. If unknown use <see cref="T:System.Object"/></typeparam>
    /// <returns>
    /// The persistent object found in the context or loaded from the database
    /// </returns>
    [Obsolete("Use GetObjectByKey<T> instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public T GetObject<T>(ObjectKey key)
    {
      ObjectKeyUtil.CheckArgumentNullability<ObjectKey>(key, "key");
      IObjectId objectId = key.GetTag() as IObjectId;
      if (objectId == null)
        return this.GetObjectByKey<T>(key);
      else
        return (T) this.GetScope().GetObjectById(objectId);
    }

    /// <summary>
    /// Creates an ObjectKey instance from a given persistent entity
    /// 
    /// </summary>
    /// <param name="entity">Persistent entity whose ObjectKey instance is to be created</param>
    /// <returns>
    /// ObjectKey instance representing the identity information of the <paramref name="entity"/> parameter
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="entity"/> is a null reference</exception><exception cref="T:System.ArgumentException"><paramref name="entity"/> is not managed by this context</exception>
    public ObjectKey CreateObjectKey(object entity)
    {
      ObjectKeyUtil.CheckArgumentNullability<object>(entity, "entity");
      ObjectKeyUtil.EnsureObjectContext(entity);
      return ((IExtendedObjectScope) this.GetScope()).GetObjectKey(entity, false);
    }

    /// <summary>
    /// Creates an ObjectKey instance with version information, from a given persistent entity.
    /// 
    /// </summary>
    /// <param name="entity">Persistent entity whose ObjectKey is to be created</param>
    /// <returns>
    /// ObjectKey instance representing the identity and version information of the <paramref name="entity"/> parameter
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="entity"/> is a null reference</exception><exception cref="T:System.ArgumentException"><paramref name="entity"/> is not managed by this context</exception>
    public ObjectKey CreateObjectKeyWithVersion(object entity)
    {
      ObjectKeyUtil.CheckArgumentNullability<object>(entity, "entity");
      ObjectKeyUtil.EnsureObjectContext(entity);
      return ((IExtendedObjectScope) this.GetScope()).GetObjectKey(entity, true);
    }

    /// <summary>
    /// Returns an object identified by the specified object key.
    /// 
    /// </summary>
    /// <param name="key">The key of the object to be found</param>
    /// <returns>
    /// The persistent object found in the context or loaded from the database
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference.</exception><exception cref="T:System.ArgumentException">Incompatible metadata specified in the <paramref name="key"/>.</exception><exception cref="T:Telerik.OpenAccess.Exceptions.NoSuchObjectException">When object with specified identity and version is not found.</exception><exception cref="T:Telerik.OpenAccess.Exceptions.OptimisticVerificationException">When object with specified identity is found but the version does not match.</exception>
    public object GetObjectByKey(ObjectKey key)
    {
      ObjectKeyUtil.CheckArgumentNullability<ObjectKey>(key, "key");
      if (string.IsNullOrEmpty(key.TypeName))
        throw new System.InvalidOperationException("The TypeName cannot be null or empty.");
      if (key.ObjectKeyValues == null)
        throw new System.InvalidOperationException("The ObjectKeyValues cannot be null.");
      else
        return ((IExtendedObjectScope) this.GetScope()).GetObjectByKey(key);
    }

    /// <summary>
    /// Returns an object identified by the specified object key.
    /// 
    /// </summary>
    /// <param name="key">The key of the object to be found</param><param name="entity">When this method returns contains the persistent object, found in the context or loaded from the database</param>
    /// <returns>
    /// true if the object was retrieved successfully.false if the object with the specified identity and version is not found.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference.</exception><exception cref="T:System.ArgumentException">Incompatible metadata specified in the <paramref name="key"/>.</exception>
    public bool TryGetObjectByKey(ObjectKey key, out object entity)
    {
      entity = (object) null;
      try
      {
        entity = this.GetObjectByKey(key);
      }
      catch (OptimisticVerificationException ex)
      {
        return false;
      }
      catch (NoSuchObjectException ex)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Returns an object identified by the specified object key.
    /// 
    /// </summary>
    /// <param name="key">The key of the object to be found</param><typeparam name="T">The type of the expected object. If unknown use <see cref="T:System.Object"/></typeparam>
    /// <returns>
    /// The persistent object found in the context or loaded from the database
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference.</exception><exception cref="T:System.ArgumentException">Incompatible metadata specified in the <paramref name="key"/>.</exception><exception cref="T:Telerik.OpenAccess.Exceptions.NoSuchObjectException">When object with specified identity and version is not found.</exception><exception cref="T:Telerik.OpenAccess.Exceptions.OptimisticVerificationException">When object with specified identity is found but the version does not match.</exception>
    public T GetObjectByKey<T>(ObjectKey key)
    {
      Type type = typeof (T);
      key.FixupType(type.FullName);
      return (T) this.GetObjectByKey(key);
    }

    /// <summary>
    /// Returns an object identified by the specified object key.
    /// 
    /// </summary>
    /// <param name="key">The key of the object to be found</param><param name="entity">When this method returns contains the persistent object, found in the context or loaded from the database</param><typeparam name="T">The type of the expected object. If unknown use <see cref="T:System.Object"/></typeparam>
    /// <returns>
    /// true if the object was retrieved successfully.false if the object with the specified identity and version is not found.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference.</exception><exception cref="T:System.ArgumentException">Incompatible metadata specified in the <paramref name="key"/>.</exception>
    public bool TryGetObjectByKey<T>(ObjectKey key, out T entity)
    {
      object entity1;
      bool objectByKey = this.TryGetObjectByKey(key, out entity1);
      entity = !objectByKey ? default (T) : (T) entity1;
      return objectByKey;
    }

    /// <summary>
    /// Returns an object identified by the specified object key if available in the cache.
    /// 
    /// </summary>
    /// <param name="key">The key of the object to be found</param><typeparam name="T">The type of the expected object. If unknown use <see cref="T:System.Object"/></typeparam>
    /// <returns>
    /// The persistent object found in the context
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference.</exception><exception cref="T:System.ArgumentException">Incompatible metadata specified in the <paramref name="key"/>.</exception>
    public T LookupObjectByKey<T>(ObjectKey key)
    {
      Type type = typeof (T);
      key.FixupType(type.FullName);
      return (T) this.LookupObjectByKey(key);
    }

    /// <summary>
    /// Returns an object identified by the specified object key if available in the cache.
    /// 
    /// </summary>
    /// <param name="key">The key of the object to be found</param>
    /// <returns>
    /// The persistent object found in the context
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference.</exception><exception cref="T:System.ArgumentException">Incompatible metadata specified in the <paramref name="key"/>.</exception>
    public object LookupObjectByKey(ObjectKey key)
    {
      ObjectKeyUtil.CheckArgumentNullability<ObjectKey>(key, "key");
      if (string.IsNullOrEmpty(key.TypeName))
        throw new System.InvalidOperationException("The TypeName cannot be null or empty.");
      if (key.ObjectKeyValues == null)
        throw new System.InvalidOperationException("The ObjectKeyValues cannot be null.");
      else
        return ((IExtendedObjectScope) this.GetScope()).LookupObjectByKey(key);
    }

    /// <summary>
    /// Marks the entities as to be deleted.
    /// 
    /// </summary>
    /// <param name="entities">The entities to be deleted from the database.</param>
    public void Delete(IEnumerable entities)
    {
      this.Delete((object) entities);
    }

    /// <summary>
    /// Marks the entity as to be deleted.
    /// 
    /// </summary>
    /// <param name="entity">The entity to be deleted from the database.</param>
    public void Delete(object entity)
    {
      this.GetScope().Remove(entity);
    }

    /// <summary>
    /// Adds new entities to the context
    /// 
    /// </summary>
    /// <param name="entities">Entities to be added.</param>
    public void Add(IEnumerable entities)
    {
      this.Add((object) entities);
    }

    /// <summary>
    /// Adds a new entity to the context
    /// 
    /// </summary>
    /// <param name="entity">Entity to be added.</param>
    public void Add(object entity)
    {
      this.GetScope().Add(entity);
    }

    /// <summary>
    /// Marks a field of an object from this context manually as 'dirty' and to be updated
    ///             in the database context. Must be called before modifications occur.
    /// 
    /// </summary>
    /// <param name="entity">The object that should
    ///             be marked as dirty. It can also be an IEnumerable of objects to mark as dirty.</param><param name="fieldName">The field that should
    ///             be marked as dirty.</param><exception cref="T:Telerik.OpenAccess.Exceptions.TransactionNotActiveException">Transaction is not running (IObjectScope implementation).
    ///             </exception><exception cref="T:Telerik.OpenAccess.Exceptions.ObjectNotEnhancedException">The type of <paramref name="entity"/> is not enhanced.
    ///             </exception>
    public void MakeDirty(object entity, string fieldName)
    {
      this.GetScope().MakeDirty(entity, fieldName);
    }

    /// <summary>
    /// Returns the concurrency control conflicts of the last store operation.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// List of conflicts from last store operation.
    /// </returns>
    public IList<ConcurrencyConflict> GetLastConflicts()
    {
      return ((IExtendedObjectScope) this.GetScope()).GetLastConflicts();
    }

    /// <summary>
    /// Returns the OpenAccessContextBase instance responsible for the given persistence capable object.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// The managing context of the passed object or <c>null</c> when the object is not managed by
    ///              an OpenAccessContextBase.
    /// 
    /// </returns>
    /// <param name="persistentObject">The persistence capable object whose context is to be returned.
    ///              </param><exception cref="T:System.ArgumentNullException">No <paramref name="persistentObject"/> has been given.
    ///              </exception><exception cref="T:Telerik.OpenAccess.Exceptions.ObjectNotEnhancedException">Type of the <paramref name="persistentObject"/> is not declared
    ///              <see cref="T:Telerik.OpenAccess.PersistentAttribute">[Persistent]</see> or not enhanced.
    ///              </exception>
    public static OpenAccessContextBase GetContext(object persistentObject)
    {
      IExtendedObjectScope extendedObjectScope = Database.GetContext(persistentObject) as IExtendedObjectScope;
      if (extendedObjectScope != null)
        return extendedObjectScope.UserContext;
      else
        return (OpenAccessContextBase) null;
    }

    /// <summary>
    /// Returns the state of the persistent object.
    /// 
    /// </summary>
    /// <param name="entity">The object the state should be returned for.</param>
    /// <returns>
    /// An enumeration representing the state of the persistent object.
    /// </returns>
    /// <exception cref="T:Telerik.OpenAccess.Exceptions.InvalidOperationException">Object is not marked as persistent.</exception><exception cref="T:System.ArgumentNullException">No entity passed in.</exception>
    public ObjectState GetState(object entity)
    {
      return this.GetScope().GetState(entity);
    }

    /// <summary>
    /// Returns the state of the named property or field from the persistent object.
    /// 
    /// </summary>
    /// <param name="entity">The object holding the field or property with the respective name.</param><param name="fieldName">The name of the field or property the state should be returned for.</param>
    /// <returns>
    /// An enumeration representing the state of the data hold by the named field or property.
    /// </returns>
    /// <exception cref="T:Telerik.OpenAccess.Exceptions.InvalidOperationException">Object is not marked as persistent.</exception><exception cref="T:System.ArgumentNullException">No entity or field name passed in.</exception>
    public ObjectState GetState(object entity, string fieldName)
    {
      return this.GetScope().GetState(entity, fieldName);
    }

    /// <summary>
    /// Returns the original value of the named property of field from the persistent object.
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the expected return value. Object can be used for untyped access.</typeparam><param name="entity">The object holding the field or property of interest.</param><param name="propertyName">The name of the property or field of interest.</param>
    /// <returns>
    /// The old value if it was available and the property or field was dirty, the actual value otherwise.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">If one of the arguments is null or string.Empty</exception><exception cref="T:System.InvalidOperationException">If the entity is not managed by this context.</exception><exception cref="T:Telerik.OpenAccess.Exceptions.InvalidOperationException">If the no property or field with the specified name can be found.</exception><exception cref="T:System.NotSupportedException">If the original value is not available.</exception>
    public T GetOriginalValue<T>(object entity, string propertyName)
    {
      return this.GetScope().GetOriginalValue<T>(entity, propertyName);
    }

    private OAConnection GetConnection(out bool isNew)
    {
      return ((IExtendedObjectScope) this.GetScope()).GetConnection(out isNew);
    }

    /// <summary>
    /// Creates and returns a detached flat copy of the persistence capable object.
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the persistence capable object.</typeparam><param name="entity">The persistence capable object.</param>
    /// <returns>
    /// A copy of the persistence capable object that is detached.
    /// </returns>
    public T CreateDetachedCopy<T>(T entity)
    {
      if ((object) entity == null)
        throw new ArgumentNullException("entity");
      ArrayList c = new ArrayList(1);
      c.Add((object) entity);
      (this.GetScope() as IExtendedObjectScope).CreateDetachedCopies(c, (FetchPlan) null);
      return (T) c[0];
    }

    /// <summary>
    /// Creates and returns a detached copy of the persistence capable object that includes the specified reference properties.
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the persistence capable object.</typeparam><param name="entity">The persistence capable object.</param><param name="referenceProperties">The reference properties to be included when creating the copy.</param>
    /// <returns>
    /// A copy of the persistence capable object that is detached.
    /// </returns>
    public T CreateDetachedCopy<T>(T entity, params string[] referenceProperties)
    {
      if ((object) entity == null)
        throw new ArgumentNullException("entity");
      if (referenceProperties == null || referenceProperties.Length == 0)
        return this.CreateDetachedCopy<T>(entity);
      Type t = typeof (T);
      List<FetchPlanFragment> list = new List<FetchPlanFragment>(referenceProperties.Length);
      char[] separator = new char[1]
      {
        '.'
      };
      foreach (string str in referenceProperties)
      {
        if (!string.IsNullOrEmpty(str))
        {
          string[] path = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
          FetchPlanFragment fetchPlanFragment = new FetchPlanFragment(t, path, false, (string) null);
          list.Add(fetchPlanFragment);
        }
      }
      IExtendedObjectScope extendedObjectScope = this.GetScope() as IExtendedObjectScope;
      FetchPlan fetch = extendedObjectScope.Database.Adapter.CalculateFetchPlan((ICollection<FetchPlanFragment>) list);
      ArrayList c = new ArrayList(1);
      c.Add((object) entity);
      extendedObjectScope.CreateDetachedCopies(c, fetch);
      return (T) c[0];
    }

    /// <summary>
    /// Attaches a copy of an object graph to the context.
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the persistence capable object.</typeparam><param name="entity">The detached, persistence capable instance.</param>
    /// <returns>
    /// The instance that is managed by the object context which is a copy of the passed <paramref name="entity"/>.
    /// </returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Entity is not a persistence capable class instance.</exception><exception cref="T:System.ArgumentNullException">Entity is not given.</exception>
    public T AttachCopy<T>(T entity)
    {
      if ((object) entity == null)
        throw new ArgumentNullException("entity");
      IExtendedObjectScope extendedObjectScope = this.GetScope() as IExtendedObjectScope;
      ArrayList c = new ArrayList(1);
      c.Add((object) entity);
      extendedObjectScope.AttachCopies(c);
      return (T) c[0];
    }

    /// <summary>
    /// Throws an exception if the context is already disposed.
    /// 
    /// </summary>
    protected void CheckNotDisposed()
    {
      if (this.disposed)
        throw new ObjectDisposedException("Telerik.OpenAccess.OpenAccessContextBase", "The context has already been disposed and it's managed persistent objects can no longer be accessed. The context should be disposed at the end of the life cycle of your business logic instance. This can also be done in the Dispose of your ASP page or MVC controller.");
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources.
    /// 
    /// </summary>
    public void Dispose()
    {
      if (this.disposed)
        return;
      this.disposed = true;
      this.Dispose(true);
    }

    /// <summary>
    /// Overwrite to free additional resources
    /// 
    /// </summary>
    /// <param name="disposing">If true dispose is executed, if false nothing is done.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!disposing || this.objectScope == null)
        return;
      this.objectScope.UserContext = (OpenAccessContextBase) null;
      this.objectScope.Dispose();
      this.objectScope = (IExtendedObjectScope) null;
    }

    /// <summary>
    /// Returns the underlying IObjectScope instance
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// Internal API entry point
    /// </returns>
    protected internal IObjectScope GetScope()
    {
      this.CheckNotDisposed();
      if (this.objectScope == null)
      {
        this.objectScope = (IExtendedObjectScope) this.GetDatabase().GetObjectScope(TransactionProvider.Automatic);
        this.objectScope.UserContext = this;
        if (this.name != null)
          this.objectScope.Correlate(this.name);
      }
      return (IObjectScope) this.objectScope;
    }

    /// <summary>
    /// Returns the underlying Database instance
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// Internal API entry point
    /// </returns>
    internal Database GetDatabase()
    {
      this.CheckNotDisposed();
      if (this.database == null)
        this.database = Database.Get(this.connectionString, this.backendConfiguration, this.metadataContainer, new DBRegistry.OpenNotification(this.PrepareOpen));
      return this.database;
    }

    private object PrepareOpenMeta(string connString, bool opened, object info)
    {
      if (opened)
      {
        this.backendConfiguration.Runtime.OnlyMetadata = (bool) info;
        return (object) null;
      }
      else
      {
        bool onlyMetadata = this.backendConfiguration.Runtime.OnlyMetadata;
        this.backendConfiguration.Runtime.OnlyMetadata = true;
        return (object) (bool) (onlyMetadata ? 1 : 0);
      }
    }

    private object PrepareOpen(string connString, bool opened, object info)
    {
      if (!opened)
      {
        this.backendConfiguration.SetDefaultsForContext();
        this.OnDatabaseOpen(this.backendConfiguration, this.metadataContainer);
      }
      return (object) null;
    }

    /// <summary>
    /// Allows custom processing on a BackendConfiguration and/or MetadataContainer instances exactly prior to opening the database.
    /// 
    /// </summary>
    /// <param name="backendConfiguration">BackendConfiguration instance.</param><param name="metadataContainer">MetadataContainer instance.</param>
    protected virtual void OnDatabaseOpen(BackendConfiguration backendConfiguration, MetadataContainer metadataContainer)
    {
    }

    void ILevelOneCache.Release(object entity)
    {
      if (entity == null)
        throw new ArgumentNullException("entity");
      this.GetScope().Evict(entity);
    }

    void ILevelOneCache.ReleaseAll()
    {
      this.GetScope().Evict((object) null);
    }

    void ILevelOneCache.ReleaseAll(IEnumerable<object> entities)
    {
      if (entities == null)
        throw new ArgumentNullException("entities");
      this.GetScope().Evict((object) entities);
    }

    void ILevelTwoCache.Evict(ObjectKey objectKey)
    {
      this.GetDatabase().Cache.Evict(objectKey);
    }

    void ILevelTwoCache.EvictAll()
    {
      this.GetDatabase().Cache.EvictAll();
    }

    void ILevelTwoCache.EvictAll(Type entityType)
    {
      this.GetDatabase().Cache.EvictAll(entityType, true);
    }

    void ILevelTwoCache.EvictAll(Type entityType, bool includeSubtypes)
    {
      this.GetDatabase().Cache.EvictAll(entityType, includeSubtypes);
    }

    void ILevelTwoCache.EvictAll(IEnumerable<ObjectKey> entityKeyList)
    {
      this.GetDatabase().Cache.EvictAll(entityKeyList);
    }

    void ILevelTwoCache.EvictAll<T>()
    {
      this.GetDatabase().Cache.EvictAll(typeof (T), true);
    }

    void ILevelTwoCache.EvictAll<T>(bool includeSubtypes)
    {
      this.GetDatabase().Cache.EvictAll(typeof (T), includeSubtypes);
    }

    bool ILevelTwoCache.IsCached(ObjectKey objectKey)
    {
      return this.GetDatabase().Cache.IsCached(objectKey);
    }

    /// <summary>
    /// Disposes the underlying database structure.
    /// 
    /// </summary>
    public void DisposeDatabase()
    {
      this.DisposeDatabase("OpenAccessContextBase.DisposeDatabase() called.");
    }

    /// <summary>
    /// Disposes the underlying database structure.
    /// 
    /// </summary>
    /// <param name="reason">The reason why the database is closed.</param>
    public void DisposeDatabase(string reason)
    {
      this.GetDatabase().DatabaseDispose(reason);
    }

    /// <summary>
    /// Returns a 'client side generated System.Guid value that is greater than any Guid previously generated for the connected database.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// An incremental Guid value.
    /// </returns>
    public Guid GetIncrementalGuid()
    {
      return this.GetScope().GetIncrementalGuid();
    }

    /// <summary>
    /// Returns a 'client side generated' System.Guid value that is greater than any Guid previously generated for the connected database.
    /// 
    /// </summary>
    /// <param name="grabSize">The number of incremental Guids that are allocated</param>
    /// <returns>
    /// An incremental Guid value
    /// </returns>
    public Guid GetIncrementalGuid(int grabSize)
    {
      return this.GetScope().GetIncrementalGuid(grabSize);
    }

    /// <summary>
    /// Returns 'client side generated' System.Guid values that are greater than any Guid previously generated for the connected database.
    /// 
    /// </summary>
    /// <param name="count">The number of incremental Guid values that should be .</param>
    /// <returns>
    /// A collection of Guids ordered in an incremental order.
    /// </returns>
    public IEnumerable<Guid> GetIncrementalGuids(int count)
    {
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", (object) count, "The requested number of Guid values should be greater than or equal to 0");
      else
        return this.GetScope().GetIncrementalGuids(count);
    }

    /// <summary>
    /// Returns a unique integer value respecting the settings specified in a UniqueIdGenerator.
    /// 
    /// </summary>
    /// <param name="generator">A UniqueIdGenerator instance that specifies the seed, grab size and sequence name for the unique integer.</param>
    /// <returns>
    /// Unique integer value for a specified sequence.
    /// </returns>
    public int GetUniqueId(UniqueIdGenerator generator)
    {
      if (generator == null)
        throw new ArgumentNullException("generator");
      if (generator.Name == null)
        throw new ArgumentNullException("generator.Name");
      if (generator.Name.Length == 0)
        throw new ArgumentException(string.Format("'{0}' is not valid. An empty string cannot be used to generate a unique id.", (object) generator.Name), "generator.Name");
      else
        return this.GetScope().GetUniqueId(generator);
    }

    /// <summary>
    /// Returns a unique integer value respecting the settings specified in a UniqueIdGenerator.
    /// 
    /// </summary>
    /// <param name="generator">A UniqueIdGenerator instance that specifies the seed, grab size and sequence name for the unique integer.</param><param name="size">Specifies the number of unique id keys that should be fetched from the database.</param>
    /// <returns>
    /// An IEnumerator instance which enumerates values in the range of the grab size.
    /// </returns>
    public IEnumerable<int> GetUniqueIds(UniqueIdGenerator generator, int size)
    {
      if (generator == null)
        throw new ArgumentNullException("generator");
      if (generator.Name == null)
        throw new ArgumentNullException("generator.Name");
      if (generator.Name.Length == 0)
        throw new ArgumentException(string.Format("'{0}' is not valid. An empty string cannot be used to generate unique ids", (object) generator.Name), "generator.Name");
      if (size < 1)
        throw new ArgumentOutOfRangeException("size", (object) size, "The requested number of ids should be greater than 0");
      else
        return this.GetScope().GetUniqueIds(generator, size);
    }
  }
}
