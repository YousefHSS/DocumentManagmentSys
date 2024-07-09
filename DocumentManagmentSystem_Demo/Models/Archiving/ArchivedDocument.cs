using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DoucmentManagmentSys.Models
{
    public class ArchivedDocument
    {
        // Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //filename
        //[unique]

        public string FileName { get; set; }

        //extension

        public string Extension { get; set; }


        private ICollection<ArchivedVersion> _versions;

        private ILazyLoader LazyLoader { get; set; }


        public ICollection<ArchivedVersion> Versions
        {
            get => LazyLoader.Load(this, ref _versions);
            set => _versions = value;
        }

    }
}
