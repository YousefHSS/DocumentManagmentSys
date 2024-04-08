using DoucmentManagmentSys.Controllers.Helpers;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
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


        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Version { get; set; }

        public string? Reason { get; set; }

        public Status status { get; set; }



        // Constructors
        public PrimacyDocument()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Version = new System.Version("0.1").ToString();
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
                Version = "0.1";
            }
            else
            {
                string[] versionParts = Version.Split('.');
                int majorVersion = int.Parse(versionParts[0]);
                int minorVersion = int.Parse(versionParts[1]);

                if (minorVersion < 9)
                {
                    minorVersion++;
                }
                else
                {
                    majorVersion++;
                    minorVersion = 0;
                }

                Version = $"{majorVersion}.{minorVersion}";
            }
        }

        public void Approve()
        {
            Reason = null;
            if (status == Status.Under_Revison)
            {
                status = Status.Under_Finalization;
            }
            else if (status == Status.Under_Finalization)
            {
                status = Status.Approved;
                WordDocumentHelper.ConvertDocxStreamToPdfAndUpdateContent(this);
                UpdateVersion();
            }

            



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
