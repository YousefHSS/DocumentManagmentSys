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

namespace DoucmentManagmentSys.Models
{
    public class AssayMethodValidationProtocolTemplate : DocumentTemplate
    {
        public AssayMethodValidationProtocolTemplate()
        {
            TemplateFileName= "Assay Method Validation Protocol";
        }
        public override List<TemplateElement> ExtractingAlgorithm(IEnumerable<OpenXmlElement> TopLevelParagraphs)
        {
            int Counter = 0;
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
                        if (WordTemplateHelper.HasBulletPointDescendants(TopLevelParagraph))
                        {
                            //get each run inside an element and addd them to the same Template Element
                            var runsToAdd = TopLevelParagraph.Descendants<Paragraph>().ToList();
                            //runsToAdd.Prepend(TopLevelParagraph);
                            highlightedRuns.Add(new TemplateElement
                            {
                                FixedTitle = PrevSibling?.InnerText ?? "This is a test Text",
                                Elements = runsToAdd.Cast<OpenXmlElement>().ToList(),
                                PlaceId=Counter++
                            });
                            continue;
                        }

                        highlightedRuns.Add(new TemplateElement
                        {
                            FixedTitle = PrevSibling?.InnerText ?? "This is a test Text",
                            Elements = [TopLevelParagraph],
                            PlaceId = Counter++
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
                            Elements = [item],
                            PlaceId = Counter++
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
                        Elements = ElementsToBeAdded,
                        PlaceId = Counter++
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
                        Elements = [TopLevelParagraph],
                        PlaceId = Counter++
                    });

                }
            }
            return highlightedRuns;
        }

        public override void ExtractTemplateElements()
        {
            List<TemplateElement> TemplateElements = new List<TemplateElement>();
            using (WordprocessingDocument Document = WordprocessingDocument.Open("wwwroot/Templates/" + TemplateFileName + ".docx", true))
            {
                // Retrieve all Run elements with a Highlight child element
                IEnumerable<Run> runs = Document.MainDocumentPart.Document.Body.Descendants<Run>().Where(r => r.Descendants<Highlight>().Any());
                //get direct child elements of body that have runs from the retrieved runs
                IEnumerable<OpenXmlElement> TopLevelParagraphs = Document.MainDocumentPart.Document.Body.ChildElements.Where(x => x.ChildElements.Count > 0 && x.Descendants<Run>().Any(y => runs.Contains(y)));


                TemplateElements = ExtractingAlgorithm(TopLevelParagraphs);

                Document.Save();

                this.TemplateElements = TemplateElements;
                
            }
        }

        public override PrimacyDocument GetPrimacyDocument()
        {
            throw new NotImplementedException();
        }

        public static void ImportTemplateElements(List<TemplateElement> TemplateElements)
        {
            //turn collection to list
            
            CloneDocument( "wwwroot/Templates/" + "Assay Method Validation Protocol" + ".docx" ,"wwwroot/Templates/" + "Assay Method Validation Protocol3" + ".docx");
            using (WordprocessingDocument DocClone = WordprocessingDocument.Open("wwwroot/Templates/" + "Assay Method Validation Protocol3" + ".docx", true))
            {
                IEnumerable<Run> runs = DocClone.MainDocumentPart.Document.Body.Descendants<Run>().Where(r => r.Descendants<Highlight>().Any());
                //get direct child elements of body that have runs from the retrieved runs
                List<OpenXmlElement> TopLevelParagraphs = DocClone.MainDocumentPart.Document.Body.ChildElements.Where(x => x.ChildElements.Count > 0 && x.Descendants<Run>().Any(y => runs.Contains(y))).ToList();


                ImportingAlgorithm(TopLevelParagraphs, TemplateElements);
                DocClone.Save();

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
            //main Difference is that this function callled after the first page argument in the view is edited
            
            //get the element that hadFixedTitle substance
            TemplateElement substance = TemplateElements.Where(x => x.FixedTitle == "Substance").FirstOrDefault();
            //get all the grey text colored runs
            IEnumerable<Run> runs = substance.Elements.OfType<Run>().Where(x => x.Descendants<RunProperties>().Any(y => y.Color == new Color { Val = "7F7F7F" }));
            //replace all the colored grey texts with the substance inner text
            foreach (var item in runs)
            {
                //Replace the Text element with the substance inner text
                var Text = item.Descendants<Text>().FirstOrDefault();
                if(Text != null)
                {
                    Text.Text = substance.FixedTitle;
                }
               
                
            }



        }

        public override void ProcessTemplateElements()
        {
            throw new NotImplementedException();
        }

        private static void ImportingAlgorithm(List<OpenXmlElement> TopLevelParagraphs, List<TemplateElement> UpdatedTemplates)
        {
            int id = 0;
            OpenXmlAttribute ReplaceMeAttribute = new OpenXmlAttribute("ReplaceMe", "http://DMSNamespace", id.ToString());

            List<List<OpenXmlElement>> ElementsReplacedFromDoc = new List<List<OpenXmlElement>>();
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
                            runsToAdd.ForEach(x => SetMarker(ReplaceMeAttribute,x));
                            //add attribute then increse id then set it to attribute
                            ElementsReplacedFromDoc.Add(runsToAdd.Cast<OpenXmlElement>().ToList());
                            continue;
                        }

                        ElementsReplacedFromDoc.Add([TopLevelParagraph]);
                    }

                    BeforePrevSibling = PrevSibling;
                }
                if (TopLevelParagraph.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Black))
                {
                    //GET rUNS THAT HAVE A BLACK HIGHLIGHT
                    var runsbLACK = TopLevelParagraph.Descendants<Run>().Where(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Black));
                    foreach (var item in runsbLACK)
                    {
                        SetMarker(ReplaceMeAttribute, item);
                        ElementsReplacedFromDoc.Add( [item]);

                    }

                }
                if (TopLevelParagraph.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Cyan))
                {
                    //GET rUNS THAT HAVE A BLACK HIGHLIGHT
                    var runsCyan = TopLevelParagraph.Descendants<Run>().Where(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Cyan));
                    var ElementsToBeAdded = new List<OpenXmlElement>();
                    foreach (var item in runsCyan)
                    {
                        SetMarker(ReplaceMeAttribute, item);
                        ElementsToBeAdded.Add(item);

                    }

                    ElementsReplacedFromDoc.Add(ElementsToBeAdded);

                }
                if (TopLevelParagraph.Descendants<Highlight>().Any(h => h.Val != null && h.Val == HighlightColorValues.Magenta))
                {
                    //get pervious sibling if it has a decendant that has a green highlight
                    var PrevSibling = TopLevelParagraph.ElementsBefore().LastOrDefault(x => x.Descendants<Highlight>().Any(y => y.Val == HighlightColorValues.Green));
                    // This run has a highlight that isn't "None", so it's considered highlighted
                    SetMarker(ReplaceMeAttribute, TopLevelParagraph);
                    ElementsReplacedFromDoc.Add([TopLevelParagraph]);

                }
            }
            ReplaceAllTopLevel(ElementsReplacedFromDoc, UpdatedTemplates);

        }
        private static void SetMarker(OpenXmlAttribute attrib , OpenXmlElement element)
        {
            element.SetAttribute(attrib);
            attrib = new OpenXmlAttribute(attrib.LocalName,attrib.NamespaceUri, (int.Parse(attrib.Value))+1 .ToString());
        }
        private static int ReplaceAllTopLevel(List<List<OpenXmlElement>> TopLevelParagraphs, List<TemplateElement> UpdatedTemplates)
        {


            //for (var i = 0; i < TopLevelParagraphs.Count; i++)
            //{
            //    var items = TopLevelParagraphs[i];
            //    for (var j = 0; j < items.Count; j++)
            //    {
            //        var item = items[j];
                   

            //        var TemplateElement = UpdatedTemplates[i];
            //        foreach (var element in TemplateElement.Elements)
            //        {
            //            if (TemplateElement.FixedTitle=="Substance"|| TemplateElement.FixedTitle == "Strength")
            //            {
                            
            //            }
            //            //children of elment must be cloned 1 by 1
            //            foreach (var child in element.ChildElements)
            //            {
            //                //find the first element inside item that have the replaceMe attrib
            //                var subitem=item.Descendants().Where(x=> x.HasAttributes).FirstOrDefault(x => x.HasAttributes && x.GetAttributes().Any(y => y.LocalName.ToString() == "ReplaceMe"));
            //                if (subitem ==null)
            //                {
            //                    //check if item has the attribute
            //                    if (item.GetAttributes().Any(y => y.LocalName.ToString() == "ReplaceMe"))
            //                    {
            //                        if (item.GetType() == child.GetType())
            //                        {
            //                            //copy the properties from item to child
            //                            CopyProperties(item, child);
            //                            item.Parent.InsertAfter(child.CloneNode(true), item);
            //                            child.Remove();
            //                            //remove attribute from the item 
            //                            item.RemoveAttribute("ReplaceMe", "http://DMSNamespace");

            //                            item.Remove();
            //                        }
            //                        else if(child.Parent!=null)
            //                        {
            //                            //copy the properties from item to child
            //                            CopyProperties(item, child.Parent);
            //                            item.Parent.InsertAfter(child.Parent.CloneNode(true), item);
                                        
            //                            //remove attribute from the item 
            //                            item.RemoveAttribute("ReplaceMe", "http://DMSNamespace");

            //                            item.Remove();
            //                            continue;
            //                        }



            //                    }
                               

            //                }
            //                else
            //                {
            //                    //replce item itsself with the child
            //                    subitem.Parent.InsertAfter(child.CloneNode(true), subitem);
            //                    subitem.Remove();
            //                }


            //            }
            //        }
            //    }
            //}
            return 0;
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
