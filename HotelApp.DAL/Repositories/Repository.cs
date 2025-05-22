using HotelApp.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HotelApp.DAL.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext Context;
        protected readonly DbSet<T> DbSet;

        public Repository(DbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public IQueryable<T> GetAll() => DbSet.AsQueryable();
        public IQueryable<T> Get(Expression<Func<T, bool>> predicate) => DbSet.Where(predicate);
        public T GetById(int id) => DbSet.Find(id);
        public void Add(T entity) => DbSet.Add(entity);
        public void Update(T entity)
        {
            var existingEntry = Context.ChangeTracker
                .Entries<T>()
                .FirstOrDefault(e => e.Entity.Equals(entity));

            if (existingEntry != null)
            {
                existingEntry.CurrentValues.SetValues(entity);
            }
            else
            {
                DbSet.Attach(entity);
                Context.Entry(entity).State = EntityState.Modified;
            }
        }
        public void Delete(T entity) => DbSet.Remove(entity);
        public void Delete(int id) => Delete(GetById(id));
    }
}