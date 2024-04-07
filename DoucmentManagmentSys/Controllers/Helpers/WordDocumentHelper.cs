
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DoucmentManagmentSys.Models;


namespace DoucmentManagmentSys.Controllers.Helpers
{
    public class WordDocumentHelper
    {
        public WordDocumentHelper() { }

        public static PrimacyDocument InsertToFooter(string Newfooter, PrimacyDocument document) 
        {
            //checked the document is word 
            if (FileTypes.IsFileTypeWord(document.FileName))
            {
                using (MemoryStream ms = new MemoryStream(document.Content))
                {
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                    {
                        // Get the main document part
                        var mainPart = doc.MainDocumentPart;

                        // Get the footer part
                        FooterPart footerPart = mainPart.FooterParts.FirstOrDefault();
                        if (footerPart != null)
                        {
                            var footer = footerPart.Footer;

                            // Create a new paragraph and run for the text
                            Paragraph paragraph = new Paragraph(new Run(new Text(Newfooter)));

                            // Append the paragraph to the footer
                            footer.Append(paragraph);
                        }

                        // Save the changes
                        mainPart.Document.Save();
                    }
                    //Update Contnent
                     document.Content = ms.ToArray();
                }
              


            }

            return document;


        }
    }
}
