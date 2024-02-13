using DoucmentManagmentSys.Data;
using DoucmentManagmentSys.Models;

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

        public void Delete(T entity)
        {

            _context.Set<T>().Remove(entity);

        }

        public IEnumerable<T> GetAll()
        {
            return _context.Set<T>();
        }

        public T GetById(int id)
        {

            return _context.Set<T>().Find(id);
        }

        public T Find(params object?[]? keyValues)
        {
            return _context.Set<T>().Find(keyValues);
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
