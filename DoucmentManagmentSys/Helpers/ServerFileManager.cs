using DoucmentManagmentSys.Models;

namespace DoucmentManagmentSys.Helpers
{
    public class ServerFileManager
    {
        // struct to return the message and the status of the file upload



        public static MessageResult UploadtoServer(IFormFile oFile, string strFolder = "./UploadedFiles/")
        {
            MessageResult result = ValidateFile(oFile);


            if (result.Status)
            {
                string strFileName = oFile.FileName;
                string strFilePath;
                strFileName = Path.GetFileName(strFileName);

                if (!Directory.Exists(strFolder))
                {
                    Directory.CreateDirectory(strFolder);
                }
                // Save the uploaded file to the server.
                strFilePath = strFolder + strFileName;

                using (FileStream fs = File.Create(strFilePath))
                {
                    oFile.CopyTo(fs);
                    fs.Flush();
                }
                result.Message = "File uploaded successfully.";
            }

            return result;
        }


        public static MessageResult ValidateFile(IFormFile oFile2, string strFolder = "./UploadedFiles/")
        {

            MessageResult result;
            result.Status = true;

            string strFilePath;

            result.Message = "Error : ";

            // Retrieve the name of the file that is posted.
            if (oFile2 == null)
            {
                result.Message += "No file selected or file is empty.";
                result.Status = false;
            }
            else
            {
                string strFileName = oFile2.FileName;
                if (!FileTypes.IsFileTypeAllowed(oFile2.ContentType))
                {
                    result.Message += "File type not supported.";
                    result.Status = false;


                }
                if (oFile2.ContentDisposition == "")
                {
                    result.Message += "File is empty.";
                    result.Status = false;

                }
                strFilePath = strFolder + strFileName;
                if (Path.Exists(strFilePath))
                {
                    result.Message += "File Exits Already. Try again";
                    result.Status = false;
                    CleanDirectory(strFolder);

                }
            }

            return result;


        }

        public static void CleanDirectory(string Folder)
        {
            Array.ForEach(Directory.GetFiles(Folder), File.Delete);
        }

        public static async Task<List<PrimacyDocument>> FilesToDocs(string UserID, string strFolder = "./UploadedFiles/")
        {

            List<PrimacyDocument> docs = new List<PrimacyDocument>();

            if (Directory.Exists(strFolder))
            {
                string[] fileNames = Directory.GetFiles(strFolder);

                foreach (string fileName in fileNames)
                {
                    byte[] fileData = await File.ReadAllBytesAsync(fileName);

                    PrimacyDocument document = new PrimacyDocument
                    {
                        FileExtensiton = Path.GetExtension(fileName),
                        FileName = Path.GetFileNameWithoutExtension(fileName),
                        Creator = UserID,
                        Content = fileData
                    };

                    docs.Add(document);

                }

            }
            return docs;

        }
        public static async Task<byte[]?> GetFileContent(string FileName, string strFolder = "./UploadedFiles/")
        {

            byte[]? fileData = null;

            if (Directory.Exists(strFolder))
            {
                // Get the file content
                fileData = await File.ReadAllBytesAsync(strFolder + FileName);
            }
            return fileData;

        }


    }
}
