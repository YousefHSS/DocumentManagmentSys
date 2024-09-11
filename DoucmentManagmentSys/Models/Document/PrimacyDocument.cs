using DocumentFormat.OpenXml.Spreadsheet;
using DoucmentManagmentSys.Helpers;
using DoucmentManagmentSys.Helpers.Word;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace DoucmentManagmentSys.Models
{
    [PrimaryKey(nameof(Id), nameof(FileName))]

    [Index(nameof(Id), IsUnique = true)]
    public class PrimacyDocument
    {
        // Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }
        public string FileExtensiton { get; set; }
        public byte[] Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Version { get; set; }

        [NotMapped]
        public static int VersionOffset = -1;
        public string? Reason { get; set; }
        public Status status { get; set; }


        [ForeignKey(nameof(Id))]
        public string Creator { get; set; }



        // Constructors
        public PrimacyDocument()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            //version is now a 3 digit number
            Version  = (int.Parse("000") + VersionOffset).ToString("000");

            status = Status.Under_Revison;
        }

        // Methods
        public void UpdateContent(byte[] newContent, Status? newStatus = null)
        {
            Content = newContent;
            UpdatedAt = DateTime.Now;
            status = newStatus ?? status;
        }

        public enum Status
        {
            Under_Revison,
            Under_Finalization,
            Approved,
            Rejected

        }
        public void UpdateVersion()
        {
            if (Version == null)
            {
                
                Version = (int.Parse("000") + VersionOffset).ToString("000");
            }
            else
            {
                if (Version.Contains('.'))
                {
                    //turn 0.1 into 001 example
                    Version = Version.Split('.')[1].PadLeft(3, '0');


                }
                Version = (int.Parse(Version) + 1).ToString("000");

            }
        }

        public void Approve(MainRepo<ArchivedDocument> _ArchivedDocsRepo)
        {
            Reason = null;
            status = Status.Approved;

            WordDocumentHelper.ConvertToPdfAndUpdate(this);

            ArchivedDocument? AD = _ArchivedDocsRepo.GetWhere(x => x.FileName == FileName).FirstOrDefault();
            //check if there is an archived document with same Document Id
            if (AD == null)
            {
                ICollection<ArchivedVersion> versions = new Collection<ArchivedVersion>();

                versions.Add(new ArchivedVersion() { Document = AD, Version = Version, Content = Content });
                //add to the archive a new version
                _ArchivedDocsRepo.Add(new ArchivedDocument()
                {
                    FileName = FileName,
                    Extension = FileExtensiton,
                    Versions = versions
                });
            }
            else
            {
                AD.Versions.Add(new ArchivedVersion() { Document = AD, Version = Version, Content = Content });
                _ArchivedDocsRepo.Update(AD);
            }
            UpdateVersion();

        }
        public void Revise()
        {
            Reason = null;
            status = Status.Under_Finalization;
        }

        public MessageResult Reject(string reason, IFormFile? fileWithReason = null)
        {
            MessageResult result = new MessageResult();
            if (fileWithReason != null)
            {
                result = ServerFileManager.UploadtoServer(fileWithReason);
 
            }
            Reason = reason;
            if (status == Status.Under_Revison)
            {
                status = Status.Rejected;
            }
            else if (status == Status.Under_Finalization)
            {
                status = Status.Under_Revison;
            }
            return result;

        }


    }
}
