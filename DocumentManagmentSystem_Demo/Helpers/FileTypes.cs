using DoucmentManagmentSys.Models;
using System.Collections;

namespace DoucmentManagmentSys.Helpers
{
    public class FileTypes
    {
        public static List<string> AllowedFileTypes { get; } = new List<string>()
                {
                    "application/pdf",
                    "text/plain",
                    "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                };


        public static List<string> AllowedFileTypesWord { get; } = new List<string>()
                {
                    "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                };

        public static bool IsFileTypeAllowed(string fileType)
        {
            return AllowedFileTypes.Contains(fileType.ToLower());
        }
        public static bool IsFileTypeWord(string fileType)
        {
            return AllowedFileTypesWord.Contains(GetContentType(fileType.ToLower()));
        }
        //Get Content type from name of file
        public static string GetContentType(string fileName)
        {
            string contentType = "application/octet-stream";
            string ext = Path.GetExtension(fileName).ToLower();
            switch (ext)
            {
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".txt":
                    contentType = "text/plain";
                    break;
            }
            return contentType;
        }

        public static void ChangeTypeTo(string newExt, PrimacyDocument document)
        {
            document.FileExtensiton = newExt;
        }
    }
}
