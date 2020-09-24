using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Stat;
using NHibernate.Type;

#pragma warning disable 618

namespace GS.DecoupleIt.Contextual.UnitOfWork.NHibernate5
{
    /// <summary>
    ///     Wrapper for <see cref="ISession" /> implementing <see cref="IUnitOfWork" />.
    /// </summary>
    [PublicAPI]
    public class NHibernateSessionUnitOfWorkWrapper : ISession, IUnitOfWork
    {
        /// <inheritdoc />
        public CacheMode CacheMode
        {
            get => _session.CacheMode;
            set => _session.CacheMode = value;
        }

        /// <inheritdoc />
        public DbConnection Connection => _session.Connection;

        /// <inheritdoc />
        public bool DefaultReadOnly
        {
            get => _session.DefaultReadOnly;
            set => _session.DefaultReadOnly = value;
        }

        /// <inheritdoc />
        public FlushMode FlushMode
        {
            get => _session.FlushMode;
            set => _session.FlushMode = value;
        }

        /// <inheritdoc />
        public bool IsConnected => _session.IsConnected;

        /// <inheritdoc />
        public bool IsOpen => _session.IsOpen;

        /// <inheritdoc />
        public ISessionFactory SessionFactory => _session.SessionFactory;

        /// <inheritdoc />
        public ISessionStatistics Statistics => _session.Statistics;

        /// <inheritdoc />
        public ITransaction Transaction => _session.Transaction;

        /// <inheritdoc />
        public event Action<IUnitOfWork> Disposed;

        /// <summary>
        ///     Creates an instance of <see cref="NHibernateSessionUnitOfWorkWrapper" />.
        /// </summary>
        /// <param name="session">NHibernate session.</param>
        public NHibernateSessionUnitOfWorkWrapper([NotNull] ISession session)
        {
            _session = session;
        }

        /// <inheritdoc />
        public ITransaction BeginTransaction()
        {
            return _session.BeginTransaction();
        }

        /// <inheritdoc />
        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return _session.BeginTransaction(isolationLevel);
        }

        /// <inheritdoc />
        public void CancelQuery()
        {
            _session.CancelQuery();
        }

        /// <inheritdoc />
        public void Clear()
        {
            _session.Clear();
        }

        /// <inheritdoc />
        public DbConnection Close()
        {
            return _session.Close();
        }

        /// <inheritdoc />
        public bool Contains(object obj)
        {
            return _session.Contains(obj);
        }

        /// <inheritdoc />
        public ICriteria CreateCriteria<T>()
            where T : class
        {
            return _session.CreateCriteria<T>();
        }

        /// <inheritdoc />
        public ICriteria CreateCriteria<T>(string alias)
            where T : class
        {
            return _session.CreateCriteria<T>(alias);
        }

        /// <inheritdoc />
        public ICriteria CreateCriteria(Type persistentClass)
        {
            return _session.CreateCriteria(persistentClass);
        }

        /// <inheritdoc />
        public ICriteria CreateCriteria(Type persistentClass, string alias)
        {
            return _session.CreateCriteria(persistentClass, alias);
        }

        /// <inheritdoc />
        public ICriteria CreateCriteria(string entityName)
        {
            return _session.CreateCriteria(entityName);
        }

        /// <inheritdoc />
        public ICriteria CreateCriteria(string entityName, string alias)
        {
            return _session.CreateCriteria(entityName, alias);
        }

        /// <inheritdoc />
        public IQuery CreateFilter(object collection, string queryString)
        {
            return _session.CreateFilter(collection, queryString);
        }

        /// <inheritdoc />
        public Task<IQuery> CreateFilterAsync(object collection, string queryString, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.CreateFilterAsync(collection, queryString, cancellationToken);
        }

        /// <inheritdoc />
        public IMultiCriteria CreateMultiCriteria()
        {
            return _session.CreateMultiCriteria();
        }

        /// <inheritdoc />
        public IMultiQuery CreateMultiQuery()
        {
            return _session.CreateMultiQuery();
        }

        /// <inheritdoc />
        public IQuery CreateQuery(string queryString)
        {
            return _session.CreateQuery(queryString);
        }

        /// <inheritdoc />
        public ISQLQuery CreateSQLQuery(string queryString)
        {
            return _session.CreateSQLQuery(queryString);
        }

        /// <inheritdoc />
        public void Delete(object obj)
        {
            _session.Delete(obj);
        }

        /// <inheritdoc />
        public void Delete(string entityName, object obj)
        {
            _session.Delete(entityName, obj);
        }

        /// <inheritdoc />
        public int Delete(string query)
        {
            return _session.Delete(query);
        }

        /// <inheritdoc />
        public int Delete(string query, object value, IType type)
        {
            return _session.Delete(query, value, type);
        }

        /// <inheritdoc />
        public int Delete(string query, object[] values, IType[] types)
        {
            return _session.Delete(query, values, types);
        }

        /// <inheritdoc />
        public Task DeleteAsync(object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.DeleteAsync(obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteAsync(string entityName, object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.DeleteAsync(entityName, obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> DeleteAsync(string query, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.DeleteAsync(query, cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> DeleteAsync(
            string query,
            object value,
            IType type,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.DeleteAsync(query,
                                        value,
                                        type,
                                        cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> DeleteAsync(
            string query,
            object[] values,
            IType[] types,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.DeleteAsync(query,
                                        values,
                                        types,
                                        cancellationToken);
        }

        /// <inheritdoc />
        public void DisableFilter(string filterName)
        {
            _session.DisableFilter(filterName);
        }

        /// <inheritdoc />
        public DbConnection Disconnect()
        {
            return _session.Disconnect();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(this))
                return;

            _session.Dispose();

            Disposed?.Invoke(this);
        }

#if !(NETSTANDARD2_0 || NETCOREAPP2_2)
        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            Dispose();

            return new ValueTask(Task.CompletedTask);
        }
#endif

        /// <inheritdoc />
        public IFilter EnableFilter(string filterName)
        {
            return _session.EnableFilter(filterName);
        }

        /// <inheritdoc />
        public void Evict(object obj)
        {
            _session.Evict(obj);
        }

        /// <inheritdoc />
        public Task EvictAsync(object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.EvictAsync(obj, cancellationToken);
        }

        /// <inheritdoc />
        public void Flush()
        {
            _session.Flush();
        }

        /// <inheritdoc />
        public Task FlushAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.FlushAsync(cancellationToken);
        }

        /// <inheritdoc />
        public object Get(Type clazz, object id)
        {
            return _session.Get(clazz, id);
        }

        /// <inheritdoc />
        public object Get(Type clazz, object id, LockMode lockMode)
        {
            return _session.Get(clazz, id, lockMode);
        }

        /// <inheritdoc />
        public object Get(string entityName, object id)
        {
            return _session.Get(entityName, id);
        }

        /// <inheritdoc />
        public T Get<T>(object id)
        {
            return _session.Get<T>(id);
        }

        /// <inheritdoc />
        public T Get<T>(object id, LockMode lockMode)
        {
            return _session.Get<T>(id, lockMode);
        }

        /// <inheritdoc />
        public Task<object> GetAsync(Type clazz, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.GetAsync(clazz, id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<object> GetAsync(
            Type clazz,
            object id,
            LockMode lockMode,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.GetAsync(clazz,
                                     id,
                                     lockMode,
                                     cancellationToken);
        }

        /// <inheritdoc />
        public Task<object> GetAsync(string entityName, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.GetAsync(entityName, id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> GetAsync<T>(object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.GetAsync<T>(id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> GetAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.GetAsync<T>(id, lockMode, cancellationToken);
        }

        /// <inheritdoc />
        public LockMode GetCurrentLockMode(object obj)
        {
            return _session.GetCurrentLockMode(obj);
        }

        /// <inheritdoc />
        public IFilter GetEnabledFilter(string filterName)
        {
            return _session.GetEnabledFilter(filterName);
        }

        /// <inheritdoc />
        public string GetEntityName(object obj)
        {
            return _session.GetEntityName(obj);
        }

        /// <inheritdoc />
        public Task<string> GetEntityNameAsync(object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.GetEntityNameAsync(obj, cancellationToken);
        }

        /// <inheritdoc />
        public object GetIdentifier(object obj)
        {
            return _session.GetIdentifier(obj);
        }

        /// <inheritdoc />
        public IQuery GetNamedQuery(string queryName)
        {
            return _session.GetNamedQuery(queryName);
        }

        /// <inheritdoc />
        public ISession GetSession(EntityMode entityMode)
        {
            return _session.GetSession(entityMode);
        }

        /// <inheritdoc />
        public ISessionImplementor GetSessionImplementation()
        {
            return _session.GetSessionImplementation();
        }

        /// <inheritdoc />
        public bool IsDirty()
        {
            return _session.IsDirty();
        }

        /// <inheritdoc />
        public Task<bool> IsDirtyAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.IsDirtyAsync(cancellationToken);
        }

        /// <inheritdoc />
        public bool IsReadOnly(object entityOrProxy)
        {
            return _session.IsReadOnly(entityOrProxy);
        }

        /// <inheritdoc />
        public void JoinTransaction()
        {
            _session.JoinTransaction();
        }

        /// <inheritdoc />
        public object Load(Type theType, object id, LockMode lockMode)
        {
            return _session.Load(theType, id, lockMode);
        }

        /// <inheritdoc />
        public object Load(string entityName, object id, LockMode lockMode)
        {
            return _session.Load(entityName, id, lockMode);
        }

        /// <inheritdoc />
        public object Load(Type theType, object id)
        {
            return _session.Load(theType, id);
        }

        /// <inheritdoc />
        public T Load<T>(object id, LockMode lockMode)
        {
            return _session.Load<T>(id, lockMode);
        }

        /// <inheritdoc />
        public T Load<T>(object id)
        {
            return _session.Load<T>(id);
        }

        /// <inheritdoc />
        public object Load(string entityName, object id)
        {
            return _session.Load(entityName, id);
        }

        /// <inheritdoc />
        public void Load(object obj, object id)
        {
            _session.Load(obj, id);
        }

        /// <inheritdoc />
        public Task<object> LoadAsync(
            Type theType,
            object id,
            LockMode lockMode,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.LoadAsync(theType,
                                      id,
                                      lockMode,
                                      cancellationToken);
        }

        /// <inheritdoc />
        public Task<object> LoadAsync(
            string entityName,
            object id,
            LockMode lockMode,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.LoadAsync(entityName,
                                      id,
                                      lockMode,
                                      cancellationToken);
        }

        /// <inheritdoc />
        public Task<object> LoadAsync(Type theType, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.LoadAsync(theType, id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> LoadAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.LoadAsync<T>(id, lockMode, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> LoadAsync<T>(object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.LoadAsync<T>(id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<object> LoadAsync(string entityName, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.LoadAsync(entityName, id, cancellationToken);
        }

        /// <inheritdoc />
        public Task LoadAsync(object obj, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.LoadAsync(obj, id, cancellationToken);
        }

        /// <inheritdoc />
        public void Lock(object obj, LockMode lockMode)
        {
            _session.Lock(obj, lockMode);
        }

        /// <inheritdoc />
        public void Lock(string entityName, object obj, LockMode lockMode)
        {
            _session.Lock(entityName, obj, lockMode);
        }

        /// <inheritdoc />
        public Task LockAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.LockAsync(obj, lockMode, cancellationToken);
        }

        /// <inheritdoc />
        public Task LockAsync(
            string entityName,
            object obj,
            LockMode lockMode,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.LockAsync(entityName,
                                      obj,
                                      lockMode,
                                      cancellationToken);
        }

        /// <inheritdoc />
        public object Merge(object obj)
        {
            return _session.Merge(obj);
        }

        /// <inheritdoc />
        public object Merge(string entityName, object obj)
        {
            return _session.Merge(entityName, obj);
        }

        /// <inheritdoc />
        public T Merge<T>(T entity)
            where T : class
        {
            return _session.Merge(entity);
        }

        /// <inheritdoc />
        public T Merge<T>(string entityName, T entity)
            where T : class
        {
            return _session.Merge(entityName, entity);
        }

        /// <inheritdoc />
        public Task<object> MergeAsync(object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.MergeAsync(obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task<object> MergeAsync(string entityName, object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.MergeAsync(entityName, obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> MergeAsync<T>(T entity, CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            return _session.MergeAsync(entity, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> MergeAsync<T>(string entityName, T entity, CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            return _session.MergeAsync(entityName, entity, cancellationToken);
        }

        /// <inheritdoc />
        public void Persist(object obj)
        {
            _session.Persist(obj);
        }

        /// <inheritdoc />
        public void Persist(string entityName, object obj)
        {
            _session.Persist(entityName, obj);
        }

        /// <inheritdoc />
        public Task PersistAsync(object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.PersistAsync(obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task PersistAsync(string entityName, object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.PersistAsync(entityName, obj, cancellationToken);
        }

        /// <inheritdoc />
        public IQueryable<T> Query<T>()
        {
            return _session.Query<T>();
        }

        /// <inheritdoc />
        public IQueryable<T> Query<T>(string entityName)
        {
            return _session.Query<T>(entityName);
        }

        /// <inheritdoc />
        public IQueryOver<T, T> QueryOver<T>()
            where T : class
        {
            return _session.QueryOver<T>();
        }

        /// <inheritdoc />
        public IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias)
            where T : class
        {
            return _session.QueryOver(alias);
        }

        /// <inheritdoc />
        public IQueryOver<T, T> QueryOver<T>(string entityName)
            where T : class
        {
            return _session.QueryOver<T>(entityName);
        }

        /// <inheritdoc />
        public IQueryOver<T, T> QueryOver<T>(string entityName, Expression<Func<T>> alias)
            where T : class
        {
            return _session.QueryOver(entityName, alias);
        }

        /// <inheritdoc />
        public void Reconnect()
        {
            _session.Reconnect();
        }

        /// <inheritdoc />
        public void Reconnect(DbConnection connection)
        {
            _session.Reconnect(connection);
        }

        /// <inheritdoc />
        public void Refresh(object obj)
        {
            _session.Refresh(obj);
        }

        /// <inheritdoc />
        public void Refresh(object obj, LockMode lockMode)
        {
            _session.Refresh(obj, lockMode);
        }

        /// <inheritdoc />
        public Task RefreshAsync(object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.RefreshAsync(obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task RefreshAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.RefreshAsync(obj, lockMode, cancellationToken);
        }

        /// <inheritdoc />
        public void Replicate(object obj, ReplicationMode replicationMode)
        {
            _session.Replicate(obj, replicationMode);
        }

        /// <inheritdoc />
        public void Replicate(string entityName, object obj, ReplicationMode replicationMode)
        {
            _session.Replicate(entityName, obj, replicationMode);
        }

        /// <inheritdoc />
        public Task ReplicateAsync(object obj, ReplicationMode replicationMode, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.ReplicateAsync(obj, replicationMode, cancellationToken);
        }

        /// <inheritdoc />
        public Task ReplicateAsync(
            string entityName,
            object obj,
            ReplicationMode replicationMode,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.ReplicateAsync(entityName,
                                           obj,
                                           replicationMode,
                                           cancellationToken);
        }

        /// <inheritdoc />
        public object Save(object obj)
        {
            return _session.Save(obj);
        }

        /// <inheritdoc />
        public void Save(object obj, object id)
        {
            _session.Save(obj, id);
        }

        /// <inheritdoc />
        public object Save(string entityName, object obj)
        {
            return _session.Save(entityName, obj);
        }

        /// <inheritdoc />
        public void Save(string entityName, object obj, object id)
        {
            _session.Save(entityName, obj, id);
        }

        /// <inheritdoc />
        public Task<object> SaveAsync(object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.SaveAsync(obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task SaveAsync(object obj, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.SaveAsync(obj, id, cancellationToken);
        }

        /// <inheritdoc />
        public Task<object> SaveAsync(string entityName, object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.SaveAsync(entityName, obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task SaveAsync(
            string entityName,
            object obj,
            object id,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.SaveAsync(entityName,
                                      obj,
                                      id,
                                      cancellationToken);
        }

        /// <inheritdoc />
        public void SaveChanges()
        {
            Flush();
        }

        /// <inheritdoc />
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return FlushAsync(cancellationToken)
                .AsNotNull();
        }

        /// <inheritdoc />
        public void SaveOrUpdate(object obj)
        {
            _session.SaveOrUpdate(obj);
        }

        /// <inheritdoc />
        public void SaveOrUpdate(string entityName, object obj)
        {
            _session.SaveOrUpdate(entityName, obj);
        }

        /// <inheritdoc />
        public void SaveOrUpdate(string entityName, object obj, object id)
        {
            _session.SaveOrUpdate(entityName, obj, id);
        }

        /// <inheritdoc />
        public Task SaveOrUpdateAsync(object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.SaveOrUpdateAsync(obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task SaveOrUpdateAsync(string entityName, object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.SaveOrUpdateAsync(entityName, obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task SaveOrUpdateAsync(
            string entityName,
            object obj,
            object id,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.SaveOrUpdateAsync(entityName,
                                              obj,
                                              id,
                                              cancellationToken);
        }

        /// <inheritdoc />
        public ISharedSessionBuilder SessionWithOptions()
        {
            return _session.SessionWithOptions();
        }

        /// <inheritdoc />
        public ISession SetBatchSize(int batchSize)
        {
            return _session.SetBatchSize(batchSize);
        }

        /// <inheritdoc />
        public void SetReadOnly(object entityOrProxy, bool readOnly)
        {
            _session.SetReadOnly(entityOrProxy, readOnly);
        }

        /// <inheritdoc />
        public void Update(object obj)
        {
            _session.Update(obj);
        }

        /// <inheritdoc />
        public void Update(object obj, object id)
        {
            _session.Update(obj, id);
        }

        /// <inheritdoc />
        public void Update(string entityName, object obj)
        {
            _session.Update(entityName, obj);
        }

        /// <inheritdoc />
        public void Update(string entityName, object obj, object id)
        {
            _session.Update(entityName, obj, id);
        }

        /// <inheritdoc />
        public Task UpdateAsync(object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.UpdateAsync(obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(object obj, object id, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.UpdateAsync(obj, id, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(string entityName, object obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.UpdateAsync(entityName, obj, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(
            string entityName,
            object obj,
            object id,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _session.UpdateAsync(entityName,
                                        obj,
                                        id,
                                        cancellationToken);
        }

        [NotNull]
        private readonly ISession _session;
    }
}
