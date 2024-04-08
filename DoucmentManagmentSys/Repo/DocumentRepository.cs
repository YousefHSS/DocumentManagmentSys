using DocumentFormat.OpenXml.Drawing.Charts;
using DoucmentManagmentSys.Controllers.Helpers;
using DoucmentManagmentSys.Data;
using DoucmentManagmentSys.Models;
using NPOI.SS.Formula.Functions;

namespace DoucmentManagmentSys.Repo
{
    public class DocumentRepository : MainRepo<PrimacyDocument>
    {
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public MessageResult AddRange(IEnumerable<PrimacyDocument> entities)
        {
            MessageResult Result = new MessageResult();
            Result.Status = true;
            Result.Message = "Documents added successfully";
            var newDocumentIDs = entities.Select(u => u.FileName).Distinct().ToArray();

            var DocumentInDb = _context.Set<PrimacyDocument>().Where(u => newDocumentIDs.Contains(u.FileName)).Select(u => u.FileName).ToArray();

            var DocumentsNotInDb = entities.Where(u => !DocumentInDb.Contains(u.FileName));
            foreach (PrimacyDocument document in DocumentsNotInDb)
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

        public MessageResult Update(int id, string NewName)
        {
            var Cleansed = ClearFileName(NewName);
            MessageResult Result = new MessageResult();
            Result.Status = true;
            Result.Message = "Document updated successfully";
            //Get file content from Uploaded files

            var DocumentInDb = _context.Set<PrimacyDocument>().Where(u => (u.Id == id && u.FileName == Cleansed)).FirstOrDefault();
            if (DocumentInDb != null)
            {
                DocumentInDb.FileName = Cleansed;
                DocumentInDb.UpdatedAt = DateTime.Now;
                DocumentInDb.Content = ServerFileManager.GetFileContent(NewName).Result ?? DocumentInDb.Content;
                _context.Update(DocumentInDb);
            }
            else
            {
                Result.Status = false;
                Result.Message = "Document not found or names are not matching";
            }


            return Result;
        }

        public IEnumerable<PrimacyDocument> Search(string search)
        {
            var DocumentInDb = _context.Set<PrimacyDocument>().Where(u => u.FileName.Contains(search)).ToList();

            return DocumentInDb;
        }

        public PrimacyDocument Find(params object?[]? keyValues) {
            //check if object in array of keys is a string
            foreach (var item in keyValues)
            {
                if (item == null) continue;

                if (item.GetType() == typeof(string))
                {
                    keyValues[Array.IndexOf(keyValues, item)] = ClearFileName((string)item!);

                }

            }
            //then continue the function normallly as in the base class
            return base.Find(keyValues)!;
                        

        }

        public string ClearFileName(string fileName) {
            //then check if the name has file extension
            if (System.IO.Path.HasExtension(fileName.ToString()!))
            {
                //remove it and assign to original index object
                 return System.IO.Path.GetFileNameWithoutExtension(fileName.ToString()!);

            }
            return fileName;
        }



    }
}
