using DocumentFormat.OpenXml;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoucmentManagmentSys.Models
{

    public abstract class DocumentTemplate
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public required ICollection<TemplateElement> TemplateElements { get; set; }


        [NotMapped]
        public string? TemplateFileName { get; set; }

        public abstract void ExtractTemplateElements();

        public abstract void PreProcessTemplateElements();
        public abstract void ProcessTemplateElements();

        public abstract PrimacyDocument GetPrimacyDocument();

        public abstract List<TemplateElement> ExtractingAlgorithm(IEnumerable<OpenXmlElement> TopLevelParagraphs);



    }
}