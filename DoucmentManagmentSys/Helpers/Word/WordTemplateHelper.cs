using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DoucmentManagmentSys.Models;
using Google.Apis.Drive.v3.Data;
using iText.StyledXmlParser.Jsoup.Nodes;
using iText.StyledXmlParser.Jsoup.Select;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using NPOI.POIFS.Properties;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

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


        public static DocumentTemplate UpdateDocumentTemplate(DocumentTemplate template, string Values, int? Page)
        {
            

            string[] TrueValues = Values.Split("__SEP__");
            //remove last  element from the list
            TrueValues = TrueValues.Last() == "" ? TrueValues.Take(TrueValues.Length - 1).ToArray() : TrueValues;
            int Iteration = 0;
            var ListedElements = template.TemplateElements;
            if (Page == 0)
            {
                //get the ones with fixed title substance and strength
                ListedElements = template.TemplateElements.Where(x => x.FixedTitle == "Substance" || x.FixedTitle == "Strength").ToList();

            }
            else
            {
                //remove the ones with fixed title substance and strength then get the rest as in pages from 2 onwards
                var TemplateElements2 = template.TemplateElements.Where(TE => TE.FixedTitle != "Substance" && TE.FixedTitle != "Strength").ToList();
                ListedElements = TemplateElements2.Skip((Page.Value) - 1).Take(1).ToList();

            }

            //loop for all template elementss
            foreach (var TemplateElement in ListedElements)
            {
                var newElements = new List<OpenXmlElement>();
                int i =0;
                foreach (var item in TemplateElement.Elements)
                {
                    if (item is Table)
                    {
                        //travese throught the table cells and change only the ones without magenta highlight
                        foreach (var row in @item.Elements<TableRow>())
                        {
                            i += row.Elements<TableCell>().Count();
                            foreach (var cell in row.Elements<TableCell>())
                            {
                                var Decen = cell.Descendants<Highlight>();
                                if (Decen.IsNullOrEmpty() || Decen.Any(h => h.Val != HighlightColorValues.Magenta))
                                {
                                    

                                    
                                  
                                    ReplaceElement(cell, TrueValues[Iteration]);
                                    
                                    Iteration++;
                                    
                                }
                            }
                        }
                        newElements.Add(item);
                    }
                    else
                    {
                        ///Change the text inside the item
                        var runElement = item.Descendants<Run>().First();
                        if (runElement == null)
                        {
                            continue;
                        }
                        ReplaceElement(runElement, TrueValues[Iteration]);
                        Iteration++;
                        newElements.Add(item);
                    }

                   


                }

                TemplateElement.Elements = newElements;
            }



            return template;
        }

        private static void ReplaceElement(OpenXmlElement element, string newText)
        {
           
            var NewElement = CKXMLHelper.FromCKToXML(newText);
            //replace all the paragrahps 
            var Parent = element.Parent;
            var ReplaceMe = new OpenXmlAttribute();
            //if the element is table cell 
            //before Removing We must save their ReplaceMe Attributes in a list to set it to the new elements
            ReplaceMe = GetReplaceMarkers(element);
           
            if (element is TableCell && NewElement.Count<=1)
            {

                
                Parent= element.Descendants<Paragraph>().FirstOrDefault();
                Parent.RemoveAllChildren<Run>();
            }
            else if(element is TableCell && NewElement.Count > 1)
            {
                element.RemoveAllChildren<Paragraph>();
                Parent = element;
            }
            else
            {
                Parent.RemoveAllChildren<Run>();
            }
            foreach (var item in NewElement)
            {
                if (ReplaceMe.LocalName != null)
                {
                    item.SetAttribute(ReplaceMe);
                }

                Parent.Append(item.CloneNode(true));
            }

        }

        public static OpenXmlAttribute GetReplaceMarkers(OpenXmlElement item)
        {
            OpenXmlAttribute Result = new OpenXmlAttribute();
           
                //iterate through Ancestors 
            if (item.GetAttributes().Any(y => y.LocalName == "ReplaceMe"))
            {
                Result = item.GetAttributes().Where(x => x.LocalName == "ReplaceMe").FirstOrDefault();
            }
            else if (item.Ancestors().Any(A=> (A is not Body) && A.GetAttributes().Any(y=> y.LocalName=="ReplaceMe")))
            {
                foreach (var ancestor in item.Ancestors())
                {
                    // Skip if the ancestor is a Body element
                    if (ancestor is Body)
                    {
                        break;
                    }

                    // Check if the ancestor has the "ReplaceMe" attribute
                    var replaceMeAttribute = ancestor.GetAttributes().FirstOrDefault(attr => attr.LocalName == "ReplaceMe");
                    if (replaceMeAttribute != null)
                    {
                        Result = replaceMeAttribute;
                        break;
                    }
                }
            }


            return Result;
            
            
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

        public static string RemoveHtmlTags(string input , string? tag = null)
        {
            string pattern = tag == null ? "<.*?>" : $"<{tag}.*?>|</{tag}>";
            return Regex.Replace(input, pattern, string.Empty, RegexOptions.IgnoreCase);
        }

        public static bool ContainsHtmlTags(string input, string? tag = null)
        {
            string pattern = tag == null ? "<.*?>" : $"<{tag}.*?>|</{tag}>";
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }



    }


}
