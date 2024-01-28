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

        public T GetByName(string Name)
        {

            return _context.Set<T>().First(u => u.GetType().GetProperty("FileName").GetValue(u).ToString() == Name);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public int AddRange(IEnumerable<Document> entities)
        {
            var newDocumentIDs = entities.Select(u => u.FileName).Distinct().ToArray();

            var DocumentInDb = _context.Set<Document>().Where(u => newDocumentIDs.Contains(u.FileName)).Select(u => u.FileName).ToArray();

            var DocumentsNotInDb = entities.Where(u => !DocumentInDb.Contains(u.FileName));
            foreach (Document document in DocumentsNotInDb)
            {
                _context.Add(document);
            }

            return DocumentsNotInDb.Count();
        }

        public void UpdateRange(IEnumerable<T> entities)
        {

            _context.Set<T>().UpdateRange(entities);
        }
    }
}
