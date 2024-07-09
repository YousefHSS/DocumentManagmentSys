using DoucmentManagmentSys.Data;
using DoucmentManagmentSys.Helpers;
using DoucmentManagmentSys.Models;

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
                //check if extension is same as old one if the old one is pdf then check for docx
                if (DocumentInDb.FileExtensiton == ".pdf")
                {
                    if (System.IO.Path.GetExtension(NewName) != ".docx")
                    {
                        Result.Status = false;
                        Result.Message = "The uploaded Document must be in .docx format";
                        return Result;
                    }

                }
                else if (System.IO.Path.GetExtension(NewName) != DocumentInDb.FileExtensiton)
                {
                    Result.Status = false;
                    Result.Message = "The uploaded Document must be in " + DocumentInDb.FileExtensiton + " format";
                    return Result;
                }


                DocumentInDb.FileName = Cleansed;
                DocumentInDb.UpdatedAt = DateTime.Now;
                DocumentInDb.Content = ServerFileManager.GetFileContent(NewName).Result ?? DocumentInDb.Content;
                FileTypes.ChangeTypeTo(".docx", DocumentInDb);
                _context.Update(DocumentInDb);
                ServerFileManager.RemoveFileFromFolder(NewName);
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

        //DN = Document Name, VR = Version, CA = Created At, UA = Updated At, SS = Status, UP = Updated By Me, DD = Downloaded BY Me
        public IEnumerable<PrimacyDocument> Search(string? search, string? DN, string? VR, string? CA, string? UA, string[]? SS)
        {
            List<PrimacyDocument> DocumentInDb = _context.Set<PrimacyDocument>().ToList();
            if (search != null)
            {
                DocumentInDb = _context.Set<PrimacyDocument>().Where(u => u.FileName.Contains(search)).ToList();
            }
            if (DN != null)
            {
                //order by document name descending
                DocumentInDb = DN == "Descending" ? DocumentInDb.OrderByDescending(u => u.FileName).ToList() : DocumentInDb.OrderBy(u => u.FileName).ToList();

            }
            if (VR != null)
            {
                DocumentInDb = VR == "Descending" ? DocumentInDb.OrderByDescending(u => u.Version).ToList() : DocumentInDb.OrderBy(u => u.Version).ToList();

            }
            if (CA != null)
            {
                DocumentInDb = CA == "Descending" ? DocumentInDb.OrderByDescending(u => u.CreatedAt).ToList() : DocumentInDb.OrderBy(u => u.CreatedAt).ToList();

            }
            if (UA != null)
            {
                DocumentInDb = UA == "Descending" ? DocumentInDb.OrderByDescending(u => u.UpdatedAt).ToList() : DocumentInDb.OrderBy(u => u.UpdatedAt).ToList();

            }
            if (SS != null && SS[0] != "1")
            {
                //SS is array of status , get only the ones that are in the array
                DocumentInDb = SS.Any() ? DocumentInDb.Where(u => SS.Contains(u.status.ToString())).ToList() : DocumentInDb;

            }





            return DocumentInDb;



        }

        public PrimacyDocument Find(params object?[]? keyValues)
        {
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

        public string ClearFileName(string fileName)
        {
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
