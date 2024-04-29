using DocumentManagmentSystem_Demo.Models;
using System.Reflection.Metadata;
using Document = DocumentManagmentSystem_Demo.Models.PrimacyDocument;

namespace DocumentManagmentSystem_Demo.Repo
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
