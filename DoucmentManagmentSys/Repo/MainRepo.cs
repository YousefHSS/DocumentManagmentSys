using DoucmentManagmentSys.Data;
using DoucmentManagmentSys.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DoucmentManagmentSys.Repo
{
    public class MainRepo<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public MainRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }
        

        public void Delete(T entity)
        {

            _context.Set<T>().Remove(entity);

        }

        public IEnumerable<T> GetAll()
        {
            return _context.Set<T>();
        }

        public IEnumerable<T> GetIncluded<TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath)
        {
            return _context.Set<T>().Include(navigationPropertyPath);
        }

        public T GetById(int id)
        {

            return _context.Set<T>().Find(id);
        }

        public T Find(params object?[]? keyValues)
        {
            return _context.Set<T>().Find(keyValues);
        }

        //GetWhere
        public IEnumerable<T> GetWhere(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }




        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }



        public void UpdateRange(IEnumerable<T> entities)
        {

            _context.Set<T>().UpdateRange(entities);
        }
    }
}
