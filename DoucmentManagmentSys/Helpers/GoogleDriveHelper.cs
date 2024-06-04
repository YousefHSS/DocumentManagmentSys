using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;

namespace DoucmentManagmentSys.Helpers
{
    public class GoogleDriveHelper
    {
        DriveService service;
        public GoogleDriveHelper()
        {


        }
        public string UploadFileAndGetFileId(string FileName)
        {
            GoogleCredential credential;
            using (var stream = new System.IO.FileStream("bin/Debug/primacypharmadms-152905541119.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.Scope.Drive);

                // Create the service.
                this.service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "PrimacyPharmaDMS",
                });

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = "Your file name.docx",
                    MimeType = "application/vnd.google-apps.document"
                };

                FilesResource.CreateMediaUpload request;
                using (var stream2 = new System.IO.FileStream("wwwroot/Templates/" + FileName + ".docx",
                                        System.IO.FileMode.Open))
                {
                    request = service.Files.Create(fileMetadata, stream2, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                    request.Fields = "id";
                    request.Upload();
                }

                var file = request.ResponseBody;
                Console.WriteLine("File ID: " + file.Id);
                return file.Id;
            }
        }

        public async Task<string> GetFrameLink(string FileName)
        {
            var viewLink = await SetDocumentPermission(UploadFileAndGetFileId(FileName));

            return viewLink;

        }

        public async Task<string> SetDocumentPermission(string fileId)
        {
            var permission = new Permission()
            {
                Type = "anyone",
                Role = "reader"
            };

            var request = service.Permissions.Create(permission, fileId);
            request.Fields = "id";
            var permissionResult = await request.ExecuteAsync();

            // Check if permission is set successfully, then create the view link.
            if (permissionResult != null)
            {
                // Construct the view link.
                string viewLink = $"https://drive.google.com/open?id={fileId}";

                // Use viewLink to embed the document in an iframe or provide it as a direct link.
                return viewLink;
            }
            else
            {
                return "Failed to set permission for the file.";
            }
        }
    }
}
