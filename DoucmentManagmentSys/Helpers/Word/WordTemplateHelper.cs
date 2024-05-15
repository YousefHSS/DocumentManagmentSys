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
            using (WordprocessingDocument Document = WordprocessingDocument.Open("wwwroot/Templates/" + Title + ".docx", true))
            {
                // Retrieve all Run elements with a Highlight child element
                IEnumerable<Run> runs = Document.MainDocumentPart.Document.Body.Descendants<Run>().Where(r => r.Descendants<Highlight>().Any());
                //get direct child elements of body that have runs from the retrieved runs
                IEnumerable<OpenXmlElement> TopLevelParagraphs = Document.MainDocumentPart.Document.Body.ChildElements.Where(x => x.ChildElements.Count > 0 && x.Descendants<Run>().Any(y => runs.Contains(y)));


                highlightedRuns = PackHiglightedElements(TopLevelParagraphs);

                Document.Save();
                return highlightedRuns;
            }










        }

        private static List<TemplateElement> PackHiglightedElements(IEnumerable<OpenXmlElement> TopLevelParagraphs)
        {
            List<TemplateElement> highlightedRuns = new List<TemplateElement>();
            OpenXmlElement? BeforePrevSibling = null;
            foreach (var TopLevelParagraph in TopLevelParagraphs)
            {

                if (TopLevelParagraph.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Yellow))
                {
                    //get pervious sibling if it has a decendant that has a green highlight
                    var PrevSibling = TopLevelParagraph.ElementsBefore().LastOrDefault(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Green));


                    if (BeforePrevSibling != null && BeforePrevSibling == PrevSibling)
                    {
                        //add the current element to the last element in the list
                        highlightedRuns.Last().AddToElements(TopLevelParagraph);
                        
                    }
                    else
                    {
                        //if the runs inside the yellow hulight are bullet points then add each bullet point to the list separately
                        if (HasBulletPointDescendants(TopLevelParagraph))
                        {
                            //get each run inside an element and addd them to the same Template Element
                            var runsToAdd = TopLevelParagraph.Descendants<Paragraph>().ToList();
                            //runsToAdd.Prepend(TopLevelParagraph);
                            highlightedRuns.Add(new TemplateElement
                            {
                                FixedTitle = PrevSibling?.InnerText ?? "This is a test Text",
                                Elements = runsToAdd.Cast<OpenXmlElement>().ToList()
                            });
                            continue;
                        }
                        
                        highlightedRuns.Add(new TemplateElement
                        {
                            FixedTitle = PrevSibling?.InnerText ?? "This is a test Text",
                            Elements = [TopLevelParagraph]
                        });
                    }

                    BeforePrevSibling = PrevSibling;
                }
                if (TopLevelParagraph.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Black))
                {
                    //GET rUNS THAT HAVE A BLACK HIGHLIGHT
                    var runsbLACK = TopLevelParagraph.Descendants<Run>().Where(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Black));
                    foreach (var item in runsbLACK)
                    {
                        highlightedRuns.Add(new TemplateElement
                        {
                            FixedTitle = "Substance",
                            Elements = [item]
                        });

                    }

                }
                if (TopLevelParagraph.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Cyan))
                {
                    //GET rUNS THAT HAVE A BLACK HIGHLIGHT
                    var runsCyan = TopLevelParagraph.Descendants<Run>().Where(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Cyan));
                    var ElementsToBeAdded = new List<OpenXmlElement>();
                    foreach (var item in runsCyan)
                    {
                        ElementsToBeAdded.Add(item);

                    }
                    highlightedRuns.Add(new TemplateElement
                    {
                        FixedTitle = "Strength",
                        Elements = ElementsToBeAdded
                    });

                }
                if (TopLevelParagraph.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Magenta))
                {
                    //get pervious sibling if it has a decendant that has a green highlight
                    var PrevSibling = TopLevelParagraph.ElementsBefore().LastOrDefault(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Green));
                    // This run has a highlight that isn't "None", so it's considered highlighted
                    highlightedRuns.Add(new TemplateElement
                    {
                        FixedTitle = PrevSibling?.InnerText ?? "This is a test Text",
                        Elements = [TopLevelParagraph]
                    });

                }
            }
            return highlightedRuns;
        }

        public static bool HasBulletPointDescendants(OpenXmlElement element)
        {
            // Check if the element itself is a Paragraph and cast it
            var paragraph = element as Paragraph;

            // If the element is not a Paragraph, look for Paragraph descendants
            var paragraphs = paragraph == null ? element.Descendants<Paragraph>() : new List<Paragraph> {  };

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
