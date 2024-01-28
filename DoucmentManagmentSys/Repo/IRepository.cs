using DoucmentManagmentSys.Models;
using System.Reflection.Metadata;
using Document = DoucmentManagmentSys.Models.Document;

namespace DoucmentManagmentSys.Repo
{
    public interface IRepository<T> where T : class
    {
        // Create
        void Add(T entity);

        int AddRange(IEnumerable<Document> entities);

        // Read
        T GetById(int id);

        T Find(params object?[]? keyValues);

        T GetByName(string Name);
        IEnumerable<T> GetAll();

        // Update
        void Update(T entity);

        void UpdateRange(IEnumerable<T> entities);

        //void UpdateRange(IEnumerable<Document> entities);

        void SaveChanges();



        // Delete
        void Delete(T entity);
    }

}
