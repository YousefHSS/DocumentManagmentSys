using System.Collections.ObjectModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DoucmentManagmentSys.Models;
using iText.StyledXmlParser.Jsoup.Select;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DoucmentManagmentSys.Helpers.Word
{
    public class WordTemplateHelper
    {
        public void CreateDocumentFromTemplate(string templatePath, string outputPath, Dictionary<string, string> replacements)
        {
            // Make a copy of the template file to work on
            System.IO.File.Copy(templatePath, outputPath, true);

            // Open the document for editing
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(outputPath, true))
            {
                // Get the main document part
                MainDocumentPart mainPart = wordDoc.MainDocumentPart;

                // You may have to process header and footer parts if placeholders are in headers or footers
                // foreach (var headerPart in mainPart.HeaderParts)
                // {
                //     ProcessTemplatePart(headerPart.Header, replacements);
                // }
                // foreach (var footerPart in mainPart.FooterParts)
                // {
                //     ProcessTemplatePart(footerPart.Footer, replacements);
                // }

                // Process the body of the document
                ProcessTemplatePart(mainPart.Document.Body, replacements);

                // Save changes to the main document part
                mainPart.Document.Save();
            }
        }

        private void ProcessTemplatePart(OpenXmlElement element, Dictionary<string, string> replacements)
        {
            // Find all text elements in the document part
            var texts = element.Descendants<Text>();

            foreach (var text in texts)
            {
                // Check if the text contains a placeholder
                foreach (var replacement in replacements)
                {
                    if (text.Text.Contains(replacement.Key))
                    {
                        // Replace the placeholder with actual content
                        text.Text = text.Text.Replace(replacement.Key, replacement.Value);
                    }
                }
            }
        }

        public static AssayMethodValidationProtocolTemplate CreateDocumentTemplate(string Title)
        {
            //search in server folder Templates for template with same name
            //get file name from server without their path
            var templateFiles = Directory.GetFiles("wwwroot/Templates").Select(x => Path.GetFileName(x));
            if (templateFiles.Contains(Title + ".docx"))
            {

                //make a document template with highlighted elements

                var DT = new AssayMethodValidationProtocolTemplate()
                {
                    Title = Title,
                    TemplateElements = new Collection<TemplateElement>()
                };
                DT.ExtractTemplateElements();
                return DT;

            }

            return new AssayMethodValidationProtocolTemplate()
            {
                Title = "Document Template Not Found",
                TemplateElements = new Collection<TemplateElement>()
            };



        }





        public static bool HasBulletPointDescendants(OpenXmlElement element)
        {
            // Check if the element itself is a Paragraph and cast it
            var paragraph = element as Paragraph;

            // If the element is not a Paragraph, look for Paragraph descendants
            var paragraphs = paragraph == null ? element.Descendants<Paragraph>() : new List<Paragraph> { };

            foreach (var para in paragraphs)
            {
                // Get the ParagraphProperties
                ParagraphProperties paragraphProperties = para.ParagraphProperties;

                if (paragraphProperties != null)
                {
                    // Get the NumberingProperties
                    NumberingProperties numberingProperties = paragraphProperties.NumberingProperties;
                    if (numberingProperties != null)
                    {
                        // Extract the NumberingLevelReference and NumberingId
                        NumberingLevelReference numberingLevelRef = numberingProperties.NumberingLevelReference;
                        NumberingId numberingId = numberingProperties.NumberingId;

                        // If both NumberingLevelReference and NumberingId are not null, it could be a bullet point
                        if (numberingLevelRef != null && numberingId != null)
                        {
                            // Additional checks could be added here to determine if it's a bullet
                            // This might involve looking up the NumberingId in the numbering definitions to see if it corresponds to a bullet
                            return true; // Placeholder, as additional checks are needed here
                        }
                    }
                }
            }

            // If no bullet points are found in the descendants
            return false;
        }

    }


}
