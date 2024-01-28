using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.ComponentModel.DataAnnotations;

namespace DoucmentManagmentSys.Models
{
    [PrimaryKey(nameof(Id), nameof(FileName))]
    public class Document
    {
        // Properties
        [Key]
        public int Id { get; set; }


        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Constructors
        public Document()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        // Methods
        public void UpdateContent(byte[] newContent)
        {
            Content = newContent;
            UpdatedAt = DateTime.Now;
        }
    }
}
