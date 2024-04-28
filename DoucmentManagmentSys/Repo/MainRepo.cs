using DoucmentManagmentSys.Data;
using DoucmentManagmentSys.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

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

        public IEnumerable<T> GetWhere(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query.ToList();
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

        public IEnumerable<T> Search(string search, string property)
        {
            // Use reflection to get the property info for the given property name
            var propertyInfo = typeof(T).GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new ArgumentException($"'{property}' is not a valid property of type '{typeof(T).Name}'");
            }

            // Use the property info to build a dynamic LINQ query
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyExpression = Expression.Property(parameter, propertyInfo);
            var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

            // Call ToLower on both the property and the search value
            var propertyToLowerExpression = Expression.Call(propertyExpression, toLowerMethod);
            var searchToLower = search.ToLower();
            var searchExpression = Expression.Constant(searchToLower);

            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });

            // Use StringComparison.OrdinalIgnoreCase for case insensitive comparison
            var comparisonExpression = Expression.Constant(StringComparison.OrdinalIgnoreCase);
            var containsExpression = Expression.Call(propertyToLowerExpression, containsMethod, searchExpression, comparisonExpression);

            var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

            return _context.Set<T>().Where(lambda.Compile()).ToList();
        }
    }
}
