using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Xml;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Helpers.Word;
using DocumentFormat.OpenXml.Packaging;

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
    }
}
