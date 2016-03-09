using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using GruppoReti.Entities.Entities;

namespace GruppoReti.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        int Commit();
    }

    public static class GlobalUnitOfWork
    {
        private const string CONTEXTKEY = "UnitOfWorkContext";
        private static readonly ThreadLocal<IUnitOfWork> _threadUnitOfWork = new ThreadLocal<IUnitOfWork>(ThreadLocalUnitOfWorkFactory);
        private static readonly ConcurrentDictionary<Thread, WeakReference<IUnitOfWork>> _threads = new ConcurrentDictionary<Thread, WeakReference<IUnitOfWork>>();

        public static void Commit()
        {
            var unitOfWork = GetUnitOfWork();

            if (unitOfWork != null)
            {
                unitOfWork.Commit();
            }
        }

        public static IUnitOfWork Current
        {
            get { return GetOrCreateUnitOfWork(); }
        }

        private static IUnitOfWork GetUnitOfWork()
        {
            if (OperationContext.Current != null && OperationContext.Current.RequestContext != null)
            {
                if (OperationContext.Current.RequestContext.RequestMessage.Properties.ContainsKey(CONTEXTKEY))
                {
                    return (IUnitOfWork)OperationContext.Current.RequestContext.RequestMessage.Properties[CONTEXTKEY];
                }
                return null;
            }
            else if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Items.Contains(CONTEXTKEY))
                    return (IUnitOfWork)HttpContext.Current.Items[CONTEXTKEY];
                return null;
            }
            else
            {
                return _threadUnitOfWork.IsValueCreated ? _threadUnitOfWork.Value : null;
            }
        }

        private static IUnitOfWork GetOrCreateUnitOfWork()
        {
            IUnitOfWork uow;

            if (OperationContext.Current != null && OperationContext.Current.RequestContext != null)
            {
                if (OperationContext.Current.RequestContext.RequestMessage.Properties.ContainsKey(CONTEXTKEY))
                {
                    uow = (IUnitOfWork)OperationContext.Current.RequestContext.RequestMessage.Properties[CONTEXTKEY];
                }
                else
                {
                    uow = CreateUnitOfWork();
                    OperationContext.Current.RequestContext.RequestMessage.Properties.Add(CONTEXTKEY, uow);
                }
            }
            else if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Items.Contains(CONTEXTKEY))
                    uow = (IUnitOfWork)HttpContext.Current.Items[CONTEXTKEY];
                else
                {
                    uow = CreateUnitOfWork();
                    HttpContext.Current.Items.Add(CONTEXTKEY, uow);
                }
            }
            else
            {
                if (!_threadUnitOfWork.IsValueCreated)
                {
                    // We CleanupDead() only before creating a new UnitOfWork to avoid calling the cleanup procedure multiple times when it's not actually needed.
                    CleanupDead();
                }
                uow = _threadUnitOfWork.Value;
            }

            return uow;
        }

      
        private static void CleanupDead()
        {
            var toCleanup = _threads.Keys.Where(x => !x.IsAlive).ToList();
            foreach (var key in toCleanup)
            {
                WeakReference<IUnitOfWork> uowRef;
                IUnitOfWork uow;
                if (_threads.TryRemove(key, out uowRef) && uowRef != null && uowRef.TryGetTarget(out uow) && uow != null)
                {
                    uow.Dispose();
                }
            }
        }

        private static IUnitOfWork CreateUnitOfWork()
        {
            return new EFUnitOfWork(new NORTHWNDEntities());
        }

        private static IUnitOfWork ThreadLocalUnitOfWorkFactory()
        {
            var uow = CreateUnitOfWork();
            _threads[Thread.CurrentThread] = new WeakReference<IUnitOfWork>(uow);
            return uow;
        }
    }

    public class EFUnitOfWork : IUnitOfWork, IDisposable
    {
        public EFUnitOfWork(DbContext context)
        {
            Context = context;
            //context.Configuration.LazyLoadingEnabled = true;
        }

        public DbContext Context { get; private set; }

        public int Commit()
        {
            return Context.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Context == null means already disposed. Consider adding explicit variable if more disposable resources are added.
            if (Context == null)
                return;

            if (disposing)
            {
                // Dispose managed resources.
                Context.Dispose();
            }

            // Dispose unmanaged resources (none here)
            // ...

            // Mark as disposed.
            Context = null;
        }
    }
}
