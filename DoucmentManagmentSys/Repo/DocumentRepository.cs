using DoucmentManagmentSys.Data;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Models.Static;

namespace DoucmentManagmentSys.Repo
{
    public class DocumentRepository : MainRepo<Document>
    {
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public MessageResult AddRange(IEnumerable<Document> entities)
        {
            MessageResult Result = new MessageResult();
            Result.Status = true;
            Result.Message = "Documents added successfully";
            var newDocumentIDs = entities.Select(u => u.FileName).Distinct().ToArray();

            var DocumentInDb = _context.Set<Document>().Where(u => newDocumentIDs.Contains(u.FileName)).Select(u => u.FileName).ToArray();

            var DocumentsNotInDb = entities.Where(u => !DocumentInDb.Contains(u.FileName));
            foreach (Document document in DocumentsNotInDb)
            {
                _context.Add(document);
            }
            if (DocumentsNotInDb.Count() == 0)
            {
                Result.Status = false;
                Result.Message = "The document(s) Uploaded Before";
            }

            return Result;
        }

        public MessageResult Update(int id, string newName)
        {
            MessageResult Result = new MessageResult();
            Result.Status = true;
            Result.Message = "Document updated successfully";
            var DocumentInDb = _context.Set<Document>().Where(u => (u.Id == id && u.FileName == newName)).FirstOrDefault();
            if (DocumentInDb != null)
            {
                DocumentInDb.FileName = newName;
                DocumentInDb.UpdatedAt = DateTime.Now;
                DocumentInDb.UpdateVersion();
                _context.Update(DocumentInDb);
            }
            else
            {
                Result.Status = false;
                Result.Message = "Document not found or names are not matching";
            }


            return Result;
        }



    }
}
