using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace DoucmentManagmentSys.Models
{
    [PrimaryKey(nameof(Id), nameof(FileName))]

    [Index(nameof(Id), IsUnique = true)]
    public class Document
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

        public Status status { get; set; }



        // Constructors
        public Document()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Version = new System.Version("0.1").ToString();
            status = Status.Under_Revison;
        }

        // Methods
        public void UpdateContent(byte[] newContent, Status newStatus)
        {
            Content = newContent;
            UpdatedAt = DateTime.Now;
            status = newStatus;
        }

        public enum Status
        {
            Under_Revison,
            Under_Finlization,
            Approved

        }
        public void UpdateVersion(bool major = true, bool minor = false)
        {
            Version VersionVal = new Version(Version);
            int MajorVal = VersionVal.Major;
            int MinorVal = VersionVal.Minor;



            if (major)
            {
                MajorVal++;

            }
            if (minor)
            {
                MinorVal++;
            }

            Version = new Version(MajorVal, MinorVal).ToString();
        }
    }
}
