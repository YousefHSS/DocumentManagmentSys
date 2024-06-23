using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Xml;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Helpers.Word;
using DocumentFormat.OpenXml.Packaging;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using RunProperties = DocumentFormat.OpenXml.Wordprocessing.RunProperties;
using System.Collections.Generic;
using Humanizer;
using iText.StyledXmlParser.Jsoup.Nodes;
using NPOI.SS.Formula.Functions;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using OpenXmlPowerTools;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace DoucmentManagmentSys.Models
{
    public class AssayMethodValidationProtocolTemplate : DocumentTemplate
    {
        public AssayMethodValidationProtocolTemplate()
        {
            TemplateFileName = "Assay Method Validation Protocol";
        }
        
        public override List<TemplateElement> ExtractingAlgorithm(IEnumerable<OpenXmlElement> TopLevelParagraphs)
        {
            int id = 0;
            OpenXmlAttribute ReplaceMeAttribute = new OpenXmlAttribute("ReplaceMe", "http://DMSNamespace", id.ToString());
            List<TemplateElement> highlightedRuns = new List<TemplateElement>();
            OpenXmlElement? BeforePrevSibling = null;
            foreach (var TopLevelParagraph in TopLevelParagraphs)
            {
                var TopLevelHighlights = TopLevelParagraph.Descendants<Highlight>().ToList();
                

                if (TopLevelHighlights.Any(h => h.Val != null && (h.Val == HighlightColorValues.Yellow || h.Val == HighlightColorValues.DarkMagenta)) && TopLevelHighlights.All(h=>(h.Val?? HighlightColorValues.DarkYellow) !=HighlightColorValues.Magenta))
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
                        if (WordTemplateHelper.HasBulletPointDescendants(TopLevelParagraph))
                        {
                            //get each run inside an element and addd them to the same Template Element
                            var ParagraphsToAdd = TopLevelParagraph.Descendants<Paragraph>().ToList();
                            ParagraphsToAdd.ForEach(x => SetMarker(ref ReplaceMeAttribute, x));
                            //runsToAdd.Prepend(TopLevelParagraph);
                            highlightedRuns.Add(new TemplateElement
                            {
                                FixedTitle = PrevSibling?.InnerText ?? "This is a test Text",
                                Elements = ParagraphsToAdd.Cast<OpenXmlElement>().ToList()

                            });
                            continue;
                        }
                        SetMarker(ref ReplaceMeAttribute, TopLevelParagraph);
                        highlightedRuns.Add(new TemplateElement
                        {
                            FixedTitle = PrevSibling?.InnerText ?? "This is a test Text",
                            Elements = [TopLevelParagraph]
                        });
                    }

                    BeforePrevSibling = PrevSibling;
                }
                if (TopLevelHighlights.Any(h => h.Val != null && h.Val == HighlightColorValues.Black))
                {
                    //GET rUNS THAT HAVE A BLACK HIGHLIGHT
                    var runsbLACK = TopLevelParagraph.Descendants<Run>().Where(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Black));
                    foreach (var item in runsbLACK)
                    {
                        SetMarker(ref ReplaceMeAttribute, item);
                        highlightedRuns.Add(new TemplateElement
                        {
                            FixedTitle = "Substance",
                            Elements = [item]
                        });

                    }

                }
                if (TopLevelHighlights.Any(h => h.Val != null && h.Val == HighlightColorValues.Cyan))
                {
                    //GET rUNS THAT HAVE A BLACK HIGHLIGHT
                    var runsCyan = TopLevelParagraph.Descendants<Run>().Where(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Cyan));
                    var ElementsToBeAdded = new List<OpenXmlElement>();
                    foreach (var item in runsCyan)
                    {
                        SetMarker(ref ReplaceMeAttribute, item);
                        ElementsToBeAdded.Add(item);

                    }
                    highlightedRuns.Add(new TemplateElement
                    {
                        FixedTitle = "Strength",
                        Elements = ElementsToBeAdded
                    });

                }
                if (TopLevelHighlights.Any(h => h.Val != null && h.Val == HighlightColorValues.Magenta))
                {
                    //get pervious sibling if it has a decendant that has a green highlight
                    var PrevSibling = TopLevelParagraph.ElementsBefore().LastOrDefault(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Green));
                    // This run has a highlight that isn't "None", so it's considered highlighted
                    SetMarker(ref ReplaceMeAttribute, TopLevelParagraph);
                    highlightedRuns.Add(new TemplateElement
                    {
                        FixedTitle = PrevSibling?.InnerText ?? "This is a test Text",
                        Elements = [TopLevelParagraph]
                    });

                }
                
            }



            return highlightedRuns;
        }

        public override void ExtractTemplateElements()
        {
            List<TemplateElement> TemplateElements = new List<TemplateElement>();
            List<string> ColoredVariables = new List<string>();
            using (WordprocessingDocument Document = WordprocessingDocument.Open("wwwroot/Templates/" + TemplateFileName + ".docx", true))
            {
                // Retrieve all Run elements with a Highlight child element
                IEnumerable<Run> runs = Document.MainDocumentPart.Document.Body.Descendants<Run>().Where(r => r.Descendants<Highlight>().Any());
                //get direct child elements of body that have runs from the retrieved runs
                IEnumerable<OpenXmlElement> TopLevelParagraphs = Document.MainDocumentPart.Document.Body.ChildElements.Where(x => x.ChildElements.Count > 0 && x.Descendants<Run>().Any(y => runs.Contains(y)));

                
                TemplateElements = ExtractingAlgorithm(TopLevelParagraphs);
                Document.Save();

                this.TemplateElements = TemplateElements;
                this.TemplateElements = ExtractingVariablesAlgorithm(TopLevelParagraphs);

                Document.Save();

                

            }
        }

        private static void ImportingAlgorithm(List<OpenXmlElement> TopLevelParagraphs, List<TemplateElement> UpdatedTemplates)
        {
            int id = 0;
            OpenXmlAttribute ReplaceMeAttribute = new OpenXmlAttribute("ReplaceMe", "http://DMSNamespace", id.ToString());

            List<List<OpenXmlElement>> ElementsReplacedFromDoc = new List<List<OpenXmlElement>>();
            OpenXmlElement? BeforePrevSibling = null;
            foreach (var TopLevelParagraph in TopLevelParagraphs)
            {

                var TopLevelHighlights = TopLevelParagraph.Descendants<Highlight>().ToList();
                

                if (TopLevelHighlights.Any(h => h.Val != null && (h.Val == HighlightColorValues.Yellow)))
                { 
                    //get pervious sibling if it has a decendant that has a green highlight
                    var PrevSibling = TopLevelParagraph.ElementsBefore().LastOrDefault(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Green));


                    if (BeforePrevSibling != null && BeforePrevSibling == PrevSibling)
                    {
                        //add the current element to the last element in the list
                        ElementsReplacedFromDoc.Last().Add(TopLevelParagraph);


                    }
                    else
                    {
                        //if the runs inside the yellow hulight are bullet points then add each bullet point to the list separately
                        if (WordTemplateHelper.HasBulletPointDescendants(TopLevelParagraph))
                        {
                            //get each run inside an element and addd them to the same Template Element
                            var runsToAdd = TopLevelParagraph.Descendants<Paragraph>().ToList();
                            //runsToAdd.Prepend(TopLevelParagraph);
                            runsToAdd.ForEach(x => SetMarker(ref ReplaceMeAttribute, x));
                            //add attribute then increse id then set it to attribute
                            ElementsReplacedFromDoc.Add(runsToAdd.Cast<OpenXmlElement>().ToList());
                            continue;
                        }

                        SetMarker(ref ReplaceMeAttribute, TopLevelParagraph);
                        ElementsReplacedFromDoc.Add([TopLevelParagraph]);
                    }

                    BeforePrevSibling = PrevSibling;
                }
                if (TopLevelHighlights.Any(h => h.Val != null && h.Val == HighlightColorValues.Black))
                {
                    //GET rUNS THAT HAVE A BLACK HIGHLIGHT
                    var runsbLACK = TopLevelParagraph.Descendants<Run>().Where(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Black));
                    foreach (var item in runsbLACK)
                    {
                        SetMarker(ref ReplaceMeAttribute, item);
                        ElementsReplacedFromDoc.Add([item]);

                    }

                }
                if (TopLevelHighlights.Any(h => h.Val != null && h.Val == HighlightColorValues.Cyan))
                {
                    //GET rUNS THAT HAVE A BLACK HIGHLIGHT
                    var runsCyan = TopLevelParagraph.Descendants<Run>().Where(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Cyan));
                    var ElementsToBeAdded = new List<OpenXmlElement>();
                    foreach (var item in runsCyan)
                    {
                        SetMarker(ref ReplaceMeAttribute, item);
                        ElementsToBeAdded.Add(item);

                    }

                    ElementsReplacedFromDoc.Add(ElementsToBeAdded);

                }
                if (TopLevelHighlights.Any(h => h.Val != null && h.Val == HighlightColorValues.Magenta))
                {
                    //get pervious sibling if it has a decendant that has a green highlight
                    var PrevSibling = TopLevelParagraph.ElementsBefore().LastOrDefault(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Green));
                    // This run has a highlight that isn't "None", so it's considered highlighted
                    SetMarker(ref ReplaceMeAttribute, TopLevelParagraph);
                    ElementsReplacedFromDoc.Add([TopLevelParagraph]);

                }
            }
            ReplaceAllTopLevel(ElementsReplacedFromDoc, UpdatedTemplates);

        }

        public static void ImportTemplateElements(List<TemplateElement> TemplateElements)
        {
            //turn collection to list

            CloneDocument("wwwroot/Templates/" + "Assay Method Validation Protocol" + ".docx", "wwwroot/Templates/" + "Assay Method Validation Protocol3" + ".docx");
            using (WordprocessingDocument DocClone = WordprocessingDocument.Open("wwwroot/Templates/" + "Assay Method Validation Protocol3" + ".docx", true))
            {
                IEnumerable<Run> runs = DocClone.MainDocumentPart.Document.Body.Descendants<Run>().Where(r => r.Descendants<Highlight>().Any());
                //get direct child elements of body that have runs from the retrieved runs
                List<OpenXmlElement> TopLevelParagraphs = DocClone.MainDocumentPart.Document.Body.ChildElements.Where(x => x.ChildElements.Count > 0 && x.Descendants<Run>().Any(y => runs.Contains(y))).ToList();
                // Create a new XML file

                
                ImportingAlgorithm(TopLevelParagraphs, TemplateElements);
                ReplaceVariablesAlgorithm(TopLevelParagraphs, TemplateElements);
                
                RemoveAllHighlights(DocClone);
                DocClone.Save();
                using (StreamWriter sw = new StreamWriter("wwwroot/Templates/" + "Assay Method Validation Protocol6" + ".xml"))
                {
                    using (XmlWriter xw = XmlWriter.Create(sw))
                    {
                        DocClone.MainDocumentPart.Document.WriteTo(xw);
                    }
                }
            }









        }

        public static void RemoveAllHighlights(WordprocessingDocument document)
        {
            // Remove highlights from the main document part
            RemoveHighlightsFromElement(document.MainDocumentPart.Document);

            // Remove highlights from headers
            foreach (var headerPart in document.MainDocumentPart.HeaderParts)
            {
                RemoveHighlightsFromElement(headerPart.Header);
            }

            // Remove highlights from footers
            foreach (var footerPart in document.MainDocumentPart.FooterParts)
            {
                RemoveHighlightsFromElement(footerPart.Footer);
            }

            // Remove highlights from text boxes in the main document part
            foreach (var textBox in document.MainDocumentPart.Document.Body.Descendants<TextBoxContent>())
            {
                RemoveHighlightsFromElement(textBox);
            }

            // If there are any other document parts where text can exist, handle them similarly
            if (document.MainDocumentPart.NumberingDefinitionsPart != null)
            {
                var numberingPart = document.MainDocumentPart.NumberingDefinitionsPart;
                var abstractNums = numberingPart.Numbering.Elements<AbstractNum>();

                foreach (var abstractNum in abstractNums)
                {
                    foreach (var lvl in abstractNum.Elements<Level>())
                    {
                        var lvlRunProperties = lvl.GetFirstChild<RunProperties>();
                        if (lvlRunProperties != null)
                        {
                            var highlight = lvlRunProperties.Highlight;
                            if (highlight != null)
                            {
                                highlight.Remove();
                            }
                        }
                    }
                }
            }

            // Save changes to the numbering definitions part
            document.MainDocumentPart.NumberingDefinitionsPart.Numbering.Save();
        }

        private static void RemoveHighlightsFromElement(OpenXmlElement element)
        {
            // Find all RunProperties elements that contain Highlight children
            var runPropertiesWithHighlights = element
                .Descendants<RunProperties>()
                .Where(rp => rp.Highlight != null)
                .ToList();

            var paragraphPropertiesWithHighlights = element
                .Descendants<ParagraphProperties>()
                .Where(rp => rp.Descendants().Any(d=> d.LocalName=="rPr") && rp.Descendants().All(d => d is not Run))
                .ToList();

            // Remove the Highlight element from those RunProperties
            foreach (var runProperties in runPropertiesWithHighlights)
            {
                var highlight = runProperties.Highlight;
                if (highlight != null)
                {
                    highlight.Val = null;
                    // Remove the Highlight element
                    highlight.Remove();
                }
            }

            foreach (var paragraphProperties in paragraphPropertiesWithHighlights)
            {
                var highlight = paragraphProperties.Descendants<Highlight>().FirstOrDefault();
                if (highlight != null)
                {
                    highlight.Val = null;
                    // Remove the Highlight element
                    highlight.Remove();
                }
            }


        }

        private ICollection<TemplateElement> ExtractingVariablesAlgorithm(IEnumerable<OpenXmlElement> topLevelParagraphs)
        {
            var TECopy = TemplateElements;

            foreach (var TopLevel in topLevelParagraphs)
            {
                if (TopLevel.Descendants<RunProperties>().Any(p => p.Color != null && p.Color.Val != "000000"))
                {

                    var Runs = TopLevel.Descendants<RunProperties>().Where(p => p.Color != null && p.Color.Val != "000000").Select(x => x.Parent as Run);
                    

                    foreach (var run in Runs)
                    {
                        if(run!=null && run.InnerText.Trim() != "" && run.RunProperties!=null && run.RunProperties.Color != null && run.RunProperties.Color.Val != "000000")
                        {
                            var FetchedTuble = GetFirstElement<Run>(TECopy, x => x.InnerText.Trim() !="" && x.RunProperties != null && x.RunProperties.Color != null && x.RunProperties.Color.Val != "000000" && x.RunProperties.Color.Val == run.RunProperties.Color.Val);
                            var FetchedFirstElement = FetchedTuble?.Item3;
                            if (FetchedFirstElement != null)
                            {
                                //check if it has the attribute
                                if(HasAttribute("Variable", "http://DMSNamespace", FetchedFirstElement))
                                {
                                    continue;
                                }
                                else
                                {
                                    FetchedFirstElement.SetAttribute(new OpenXmlAttribute("Variable", "http://DMSNamespace", run.RunProperties.Color.Val));
                                    //update template elements
                                    FetchedTuble.Item1.ReplaceFromElements(FetchedFirstElement.Ancestors().Last(), FetchedTuble.Item2);

                                    break;
                                
                                }


                            }
                            else
                            {
                                OpenXmlAttribute openXmlAttribute = new OpenXmlAttribute("Variable", "http://DMSNamespace", run.RunProperties.Color.Val);
                                //check if run exits in template elements
                                var clonedRun = run.CloneNode(true);
                                clonedRun.SetAttribute(openXmlAttribute);
                                TECopy.Add(
                                new TemplateElement()
                                {
                                    FixedTitle = "Colored Variable",
                                    Elements = new List<OpenXmlElement>() { clonedRun }
                                });
                            }
                          
                        }
                       
                        
                    }
                }


            }

            return TECopy;
           
        }

        public static bool HasAttribute(OpenXmlAttribute attribute, OpenXmlElement element)
        {
            try
            {
                element.GetAttribute(attribute.LocalName, attribute.NamespaceUri);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool HasAttribute(string LocalName, string NamespaceUri, OpenXmlElement element)
        {
            try
            {
                element.GetAttribute(LocalName, NamespaceUri);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Tuple<TemplateElement,int, TSource> GetFirstElement<TSource>(IEnumerable<TemplateElement> elements, Func<TSource, bool> predicate) where TSource : OpenXmlElement
        {
            foreach (var templateElement in elements)
            {
                int i = 0;
                foreach (var elementList in templateElement.Elements)
                {
                    foreach (TSource element in elementList.Descendants<TSource>())
                    {
                        
                        if (predicate(element))
                        {
                            return  new Tuple<TemplateElement, int, TSource>(templateElement, i, element); // Returning a reference to the matching element
                        }
                        
                    }
                    i++;
                }
            }

           return null;
        }

        private static IEnumerable<Run>  MainColoredRuns(ICollection<TemplateElement> templateElements)
        {
            //extract all runs that have a color
            var Runs= new List<Run>();
            foreach (var run in templateElements.SelectMany(x => x.Elements).SelectMany(x => x.Descendants<Run>()).Where(x => x.RunProperties != null))
            {
                OpenXmlAttribute openXmlAttribute = new OpenXmlAttribute("Variable", "http://DMSNamespace", "000000");
                try
                {
                    if (run.InnerText.Trim() != "" && run.GetAttribute(openXmlAttribute.LocalName, openXmlAttribute.NamespaceUri) != null)
                    {
                        Runs.Add(run);
                    }
                }
                catch(KeyNotFoundException ex)
                {
                    continue;
                }
            }

            return Runs;
        }
            
        public override PrimacyDocument GetPrimacyDocument()
        {
            throw new NotImplementedException();
        }

        private static void ReplaceVariablesAlgorithm(List<OpenXmlElement> topLevelParagraphs, List<TemplateElement> templateElements)
        {
            foreach (var topLevel in topLevelParagraphs)
            {
                topLevel.Descendants<Run>().Where(x=>x.InnerText.Trim()=="").ToList().ForEach(x => x.RunProperties.Color = null); 
            }
            var Runs = MainColoredRuns(templateElements);
            foreach (var templateRun in Runs)
            {

                string replacementText = templateRun.InnerText;

                // Iterate through each Run in topLevelParagraphs and replace text if colors match
                foreach (var topLevel in topLevelParagraphs)
                {
                    var ComparisonAttribute = templateRun.GetAttribute("Variable", "http://DMSNamespace");
                    foreach (var run in topLevel.Descendants<Run>().Where(
                        run=> run.InnerText != "" &&
                            run.RunProperties != null &&
                            run.RunProperties.Color != null &&
                            run.RunProperties.Color.Val.Value.Equals(ComparisonAttribute.Value, StringComparison.OrdinalIgnoreCase)))
                    {
                            

                        
                        
                            // Replace the run text with the text from the matching template run
                            var text = run.Descendants<Text>().FirstOrDefault();
                            if (text != null)
                            {
                                text.Text = replacementText;
                                run.RunProperties.Color = null;
                            }
                        
                    }
                }
            }
        }

        public static void SaveClonedDocument(WordprocessingDocument clonedDocument, string newFilePath)
        {
            // Save the cloned WordprocessingDocument to a new file
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Clone the parts of the document necessary to preserve styles and formatting
                clonedDocument.Clone(memoryStream);


                // Write the content from the memory stream to the new file
                File.WriteAllBytes(newFilePath, memoryStream.ToArray());
            }
        }

        public static void CloneDocument(string sourceFilePath, string newFilePath)
        {
            using (WordprocessingDocument originalDoc = WordprocessingDocument.Open(sourceFilePath, true))
            {
                // The using statement ensures that the cloned document is disposed of properly
                using (WordprocessingDocument clonedDoc = (WordprocessingDocument)originalDoc.Clone())
                {
                    SaveClonedDocument(clonedDoc, newFilePath);
                }
            }
        }
        
        public override void PreProcessTemplateElements()
        {
            throw new NotImplementedException();
        }

        public override void ProcessTemplateElements()
        {
            throw new NotImplementedException();
        }

        private static void SetMarker(ref OpenXmlAttribute attrib, OpenXmlElement element)
        {
            element.SetAttribute(attrib);
            IncreaseId(ref attrib);
        }
        
        private static void IncreaseId(ref OpenXmlAttribute attrib)
        {
            if (attrib.Value == null)
            {
                attrib = new OpenXmlAttribute(attrib.LocalName, attrib.NamespaceUri, "1");
                return;
            }
            var val = int.Parse(attrib.Value);
            val++;
            attrib = new OpenXmlAttribute(attrib.LocalName, attrib.NamespaceUri, val.ToString());
        }
        
        private static int ReplaceAllTopLevel(List<List<OpenXmlElement>> TopLevelParagraphs, List<TemplateElement> UpdatedTemplates)
        {
            //replace each item marked with ReplaceMe attribute from TopLevelParagraphs with it's corresponding child with replace me attribute from UpdatedTemplates 
            // the value of the attribute must be similar to the attribute in UpdatedTemplates

            int id = 0;
            OpenXmlAttribute ReplaceMeAttribute = new OpenXmlAttribute("ReplaceMe", "http://DMSNamespace", id.ToString());
            while (true)
            {
                var firstElm = FindMarkedElementInList(TopLevelParagraphs, ReplaceMeAttribute);
                var secondElm = FindMarkedElementInList(UpdatedTemplates, ReplaceMeAttribute);
                
               
                if (firstElm == null || secondElm == null)
                {
                    break;
                }
                CopyProperties(secondElm, firstElm);
                if (firstElm.Parent == null)
                {
                    //replcae the children of the first element with the second element
                    ReplaceChildren(firstElm, secondElm);

                }
                else
                {
                    firstElm.Parent.InsertAfter(secondElm.CloneNode(true), firstElm);
                    firstElm.Remove();
                }


                IncreaseId(ref ReplaceMeAttribute);

            }

            return 0;
        }

        public static void ReplaceChildren(OpenXmlElement firstElm, OpenXmlElement secondElm)
        {
            //replce the children of the first element with the second element
            firstElm.RemoveAllChildren();
            foreach (var child in secondElm.ChildElements)
            {
                firstElm.Append(child.CloneNode(true));
            }

        }

        private static OpenXmlElement? FindMarkedElementInList(List<List<OpenXmlElement>> openXmlElements, OpenXmlAttribute replaceMeAttribute)
        {
            //this function should keep traversing inside the list until it finds the first element that has the replaceMe attribute

            for (var i = 0; i < openXmlElements.Count; i++)
            {
                var items = openXmlElements[i];
                for (var j = 0; j < items.Count; j++)
                {
                    var item = items[j];
                    if (item.GetAttributes().Any(y => y == replaceMeAttribute))
                    {
                        return item;
                    }
                }
            }
            return null;

        }
        
        private static OpenXmlElement? FindMarkedElementInList(List<TemplateElement> templateElements, OpenXmlAttribute replaceMeAttribute)
        {
            //this function should keep traversing inside the list until it finds the first element that has the replaceMe attribute

            for (var i = 0; i < templateElements.Count; i++)
            {
                var items = templateElements[i].Elements.ToList();
                for (var j = 0; j < items.Count; j++)
                {
                    var item = items[j];
                    if (item.GetAttributes().Any(y => y.LocalName == replaceMeAttribute.LocalName && y.Value == replaceMeAttribute.Value))
                    {
                        return item;
                    }
                    if (item.Descendants().Any(x => x.GetAttributes().Any(y => y.LocalName == replaceMeAttribute.LocalName && y.Value == replaceMeAttribute.Value)))
                    {
                        return item.Descendants().FirstOrDefault(x => x.GetAttributes().Any(y => y.LocalName == replaceMeAttribute.LocalName && y.Value == replaceMeAttribute.Value));
                    }
                }
            }
            return null;


        }

        private static void CopyProperties(OpenXmlElement from, OpenXmlElement to)
        {
            //check if to doesn't have properties already
            if (!(to.Descendants().Any(x => x.LocalName.Contains("Pr"))))
            {
                //check if from and to are the same type
                if (from.GetType() == to.GetType())
                {
                    //now copy the properties from to to
                    // Use LocalName to match the run properties element
                    var runProperties = from.Descendants<RunProperties>().FirstOrDefault();
                    if (runProperties != null)
                    {
                        to.PrependChild(runProperties.CloneNode(true));
                    }
                }
            }
        }
    }
}
