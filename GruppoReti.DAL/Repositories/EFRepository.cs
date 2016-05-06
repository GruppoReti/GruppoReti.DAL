using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace SAIPEM.YARPO.DAL.Repositories
{
    public class EFRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private DbContext _context { get; set; }
        private DbSet<TEntity> _dbSet { get; set; }

        public EFRepository() 
        {
            //this.Context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

        /// <summary>
        /// specifiy a connection timeout in seconds
        /// </summary>
        /// <param name="Timeout"></param>
        public EFRepository(int Timeout)
        {
            this.Context.Database.CommandTimeout = Timeout;
        }
        
        protected DbContext Context
        {
            get
            {
                if (_context == null)
                    _context = ((EFUnitOfWork)GlobalUnitOfWork.Current).Context;
                return _context;
            }
        }

        protected DbSet<TEntity> DbSet
        {
            get
            {
                if (_dbSet == null)
                    _dbSet = this.Context.Set<TEntity>();
                return this._dbSet;
            }
        }


        public IQueryable<TEntity> GetQuery()
        {
            return DbSet;
        }

        /// <summary>
        /// Get all records of type T
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TEntity> GetAll()
        {
            return DbSet.ToList();
        }

        /// <summary>
        /// Get records of type T filtered based on a predicate
        /// </summary>
        /// <param name="where"></param>
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> where)
        {
            return DbSet.Where<TEntity>(where);
        }

        /// <summary>
        /// Gets the only element of type T, or a default value if emtpy. Throws an exception if there is more than one element
        /// </summary>
        /// <param name="where">Function used to filters records</param>
        /// <returns></returns>
        public TEntity Single(Expression<Func<TEntity, bool>> where)
        {
            return DbSet.Where<TEntity>(where).SingleOrDefault();
        }

        public TEntity First(Expression<Func<TEntity, bool>> where)
        {
            return DbSet.Where<TEntity>(where).First();
        }

        public void Delete(TEntity entity)
        {
            if (Context.Entry(entity).State == System.Data.Entity.EntityState.Detached)
            {
                DbSet.Attach(entity);
            }
            DbSet.Remove(entity);
        }

        public void DeleteAll(List<TEntity> listToDelete)
        {
            foreach (TEntity ent in listToDelete)
            {
                if (Context.Entry(ent).State == System.Data.Entity.EntityState.Detached)
                {
                    DbSet.Attach(ent);
                }
                DbSet.Remove(ent);
            }
        }

        public virtual void Add(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public virtual void AddAll(IEnumerable<TEntity> entities)
        {
            DbSet.AddRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Attach(entity);
            Context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }

        public virtual void UpdateAll(List<TEntity> entities)
        {
            foreach (TEntity entity in entities)
            {
                DbSet.Attach(entity);
                Context.Entry(entity).State = System.Data.Entity.EntityState.Modified; 
            }
        }

        public IQueryable<TEntity> IncludeMultiple(params Expression<Func<TEntity, object>>[] includeExpressions)
        {
           
            return includeExpressions.Aggregate<Expression<Func<TEntity, object>>, IQueryable<TEntity>>
                (DbSet, (current, expression) => current.Include(expression));
        }
    }
}
