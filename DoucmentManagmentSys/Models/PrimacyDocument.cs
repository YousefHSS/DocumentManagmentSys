using DocumentFormat.OpenXml.Spreadsheet;
using DoucmentManagmentSys.Helpers;
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
            Version =  "000";
            status = Status.Under_Revison;
        }

        // Methods
        public void UpdateContent(byte[] newContent, Status? newStatus=null)
        {
            Content = newContent;
            UpdatedAt = DateTime.Now;
            status = newStatus?? this.status;
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
                Version = "000";
            }
            else
            {
                 Version = (int.Parse(Version) + 1).ToString("000");
            }
        }

        public void Approve(MainRepo<ArchivedDocument> _ArchivedDocsRepo)
        {
            Reason = null;
            status = Status.Approved;

            WordDocumentHelper.ConvertToPdfAndUpdate(this);
            ArchivedDocument? AD = _ArchivedDocsRepo.GetWhere(x => x.FileName == this.FileName).FirstOrDefault();
            //check if there is an archived document with same Document Id
            if (AD == null)
            {
                ICollection<ArchivedVersion> versions = new Collection<ArchivedVersion>();

                versions.Add(new ArchivedVersion() { Document = AD, Version = this.Version, Content = this.Content });
                //add to the archive a new version
                _ArchivedDocsRepo.Add(new ArchivedDocument()
                {
                    FileName = this.FileName,
                    Extension = this.FileExtensiton,
                    Versions = versions
                });
            }
            else
            {
                AD.Versions.Add(new ArchivedVersion() { Document = AD, Version = this.Version, Content = this.Content });
                _ArchivedDocsRepo.Update(AD);
            }
            UpdateVersion();

        }
        public void Revise()
        {
            Reason = null;
            status = Status.Under_Finalization;
        }

        public void Reject(string reason)
        {
            Reason = reason;
            if (status == Status.Under_Revison)
            {
                status = Status.Rejected;
            }
            else if (status == Status.Under_Finalization)
            {
                status = Status.Under_Revison;
            }


        }


    }
}
