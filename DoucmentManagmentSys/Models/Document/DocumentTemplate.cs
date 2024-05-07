using DocumentFormat.OpenXml;
using System.Xml;

namespace DoucmentManagmentSys.Models
{
    public class DocumentTemplate
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public required ICollection<TemplateElement> FixedElements { get; set; }

    }
}
