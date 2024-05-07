using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.StyledXmlParser.Jsoup.Nodes;
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
        public OpenXmlElement Element
        {
            get => DeserializeFromBinary(DefaultData);
            set => DefaultData = SerializeToBinary(value);
        }


        public byte[] SerializeToBinary(OpenXmlElement element)
        {

            using (var memoryStream = new MemoryStream())
            {
                //make a document of the element
                using (var document = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
                {

                    //add element to doc mainpart
                    document.AddMainDocumentPart().Document = new Document(new Body(element.CloneNode(true)));


                    //save the document
                    document.Save();
                    //save to byte array


                   
                }

                return memoryStream.ToArray();

            }
        }


        public OpenXmlElement DeserializeFromBinary(byte[] xmlData)
        {
            OpenXmlElement element ;
            //create file form incoming binary data
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(xmlData, 0, xmlData.Length);
                using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                {
                    //return the element inside the body of MainDocumentPart
                    element= doc.MainDocumentPart.Document.Body.ChildElements.First();
                }
                memoryStream.Close();
            }
            return element;
        }

       

    }
}