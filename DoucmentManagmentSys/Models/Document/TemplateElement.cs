using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.StyledXmlParser.Jsoup.Nodes;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Document = DocumentFormat.OpenXml.Wordprocessing.Document;

namespace DoucmentManagmentSys.Models
{
    public class TemplateElement
    {

        [Key]
        public int Id { get; set; }

        // This is the non-mapped property that you'll interact with in your c

        public string FixedTitle { get; set; }


        public byte[] DefaultData { get; set; }

        [NotMapped]
        public ICollection<OpenXmlElement> Elements
        {
            get => DeserializeFromBinary(DefaultData);
            set => DefaultData = SerializeToBinary(value);
        }


        public byte[] SerializeToBinary(ICollection<OpenXmlElement> elements)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var document = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
                {
                    // Create a new body with the elements
                    var body = new Body();
                    foreach (var element in elements)
                    {
                        body.AppendChild(element.CloneNode(true));
                    }

                    // Add the body to the main document part
                    document.AddMainDocumentPart().Document = new Document(body);

                    // Save the document
                    document.Save();
                }
                return memoryStream.ToArray();
            }
        }


        public ICollection<OpenXmlElement> DeserializeFromBinary(byte[] xmlData)
        {
            var elements = new List<OpenXmlElement>();
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(xmlData, 0, xmlData.Length);
                using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                {
                    // Get all elements inside the body of MainDocumentPart
                    foreach (var element in doc.MainDocumentPart.Document.Body.ChildElements)
                    {
                        elements.Add(element.CloneNode(true));
                    }
                }
                memoryStream.Close();
            }
            return elements;
        }

        public void AddToElements(OpenXmlElement NewElement)
        {
            //make a new list from the current list
            var NewElements = Elements;
            //add the new element
            NewElements.Add(NewElement);
            //set the new list
            Elements = NewElements;
        }



    }

   
}