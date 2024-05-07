using System.Reflection.Metadata;
using Document = DoucmentManagmentSys.Models.PrimacyDocument;

namespace DoucmentManagmentSys.Repo
{
    public interface IRepository<T> where T : class
    {
        // Create
        void Add(T entity);



        // Read
        T GetById(int id);

        T Find(params object?[]? keyValues);

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
