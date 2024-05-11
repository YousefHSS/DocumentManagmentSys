using System.Collections.ObjectModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DoucmentManagmentSys.Models;
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

        public static DocumentTemplate CreateDocumentTemplate(string Title)
        {
            //search in server folder Templates for template with same name
            //get file name from server without their path
            var templateFiles = Directory.GetFiles("wwwroot/Templates").Select(x => Path.GetFileName(x));
            if (templateFiles.Contains(Title + ".docx"))
            {
                //if found then fetch the highlighted OpenXmlElements
                List<TemplateElement> highlightedElements = GetHighlitedElements(Title);
                //make a document template with highlighted elements

                return new DocumentTemplate()
                {
                    Title = Title,
                    TemplateElements = highlightedElements
                };

            }
            
            return new DocumentTemplate()
            {
                Title = "Document Template Not Found",
                TemplateElements = new Collection<TemplateElement>()
            };



        }



        private static List<TemplateElement> GetHighlitedElements(string Title)
        {

            List<TemplateElement> highlightedRuns = new List<TemplateElement>();
            using (WordprocessingDocument Document = WordprocessingDocument.Open("wwwroot/Templates/" + Title+".docx", true))
            {
                // Retrieve all Run elements with a Highlight child element
                IEnumerable<Run> runs = Document.MainDocumentPart.Document.Body.Descendants<Run>().Where(r=> r.Descendants<Highlight>().Any());
                //get direct child elements of body that have runs from the retrieved runs
               var TopLevelParagraphs = Document.MainDocumentPart.Document.Body.ChildElements.Where(x => x.ChildElements.Count > 0 && x.Descendants<Run>().Any(y => runs.Contains(y)));
                foreach (var Parent in TopLevelParagraphs)
                {
                    if (Parent.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Yellow))
                    {
                        //get pervious sibling if it has a decendant that has a green highlight
                        var PrevSibling = Parent.ElementsBefore().LastOrDefault(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Green));
                        // This run has a highlight that isn't "None", so it's considered highlighted
                        highlightedRuns.Add(new TemplateElement
                        {
                            FixedTitle = PrevSibling?.InnerText ?? "This is a test Text",
                            Element = Parent
                        });
                    }
                    if (Parent.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Black))
                    {
                        //Black For the Substance
                        highlightedRuns.Add(new TemplateElement
                        {
                            FixedTitle = "Substance",
                            Element = Parent
                        });

                    }
                    if (Parent.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Cyan))
                    {
                        //Cyan For the Strength
                        highlightedRuns.Add(new TemplateElement
                        {
                            FixedTitle = "Strength",
                            Element = Parent
                        });

                    }

                }
                Document.Save();
                return highlightedRuns;
            }

           
            







        }

    }
}
