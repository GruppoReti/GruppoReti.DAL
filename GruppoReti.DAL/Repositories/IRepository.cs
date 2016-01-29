using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GruppoReti.DAL.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetQuery();

        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> where);
        TEntity Single(Expression<Func<TEntity, bool>> where);
        TEntity First(Expression<Func<TEntity, bool>> where);

        void Delete(TEntity entity);
        void Add(TEntity entity);
        void Update(TEntity entity);
    }
}
