using DocumentFormat.OpenXml;

using DocumentFormat.OpenXml.Wordprocessing;
using System.Net;
using System.Text.RegularExpressions;

namespace DoucmentManagmentSys.Helpers.Word
{
    public class CKXMLHelper
    {
        public static List<OpenXmlElement> FromCKToXML(string CK)
        {
            //the purpose of this function is to convert from CK HTML to OpenXMl ELements
            var Result = new List<OpenXmlElement>();
            //this function should take the first CKELEMENT only
            var CKTopLevelElement = ExtractFirstTag(CK);
            //id = timestamp
            int timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            
            //now we have parse each tag top-down
            if (WordTemplateHelper.ContainsHtmlTags(CKTopLevelElement, "ul"))
            {

                var newText = WordTemplateHelper.RemoveHtmlTags(CK, "ul");
                //we have to parse each <li> </li>
                foreach (var item in newText.Split("<li>"))
                {
                    if (item != "")
                    {
                        var MidLevelElement = item.Replace("</li>", "");
                        //the result paragraph must be have a bullet point property
                        var LowerLevelElement = ConstructLowLevelElement(MidLevelElement);
                        //create a new paragraph with bullet point property
                        var paragraph = new Paragraph();
                        //create a numbering level
                        ParagraphProperties paragraphProperties = new ParagraphProperties(
                            new ParagraphStyleId() { Val = "ListParagraph" }
                            ,
                            new NumberingProperties(
                                new NumberingLevelReference() { Val = 0 },
                                new NumberingId() { Val = 23 }
                            ),
                            // Set the indentation for the paragraph (e.g., 720 twips for 0.5 inch)
                            new Indentation() { Left = "360" }
                        );
                        paragraph.ParagraphProperties= paragraphProperties;
                        
                        paragraph.Append(LowerLevelElement);
                        Result.Add(paragraph);
                    }

                }

            }
            else if(WordTemplateHelper.ContainsHtmlTags(CKTopLevelElement, "p"))
            {
                var MidLevelElement = WordTemplateHelper.RemoveHtmlTags(CKTopLevelElement, "p");
                var LowerLevelElement = ConstructLowLevelElement(MidLevelElement);
                Result.AddRange(LowerLevelElement);

            }
            else
            {
                throw new Exception("Unconvertable CK Element:" + CKTopLevelElement);
            }
            return Result;
        }
        public static string ExtractFirstTag(string HTML)
        {
            var match = Regex.Match(HTML, @"<(.*?)>(.*?)<\/\1>", RegexOptions.Singleline);
            return match.Success ? match.Value : HTML;
        }
        public static List<Run> ConstructLowLevelElement(string HTMLs)
        {
            //we have to check if there is a strong tag then the Run must have a bold property
            //if there is i tag then the Run must have an italics property
            var RunsResults = new List<Run>();
            foreach (var span in WordTemplateHelper.SplitByTag(HTMLs,"span"))
            {

                var HTML = span;


                var run = new Run();
                var runProperties = new RunProperties();
                if (WordTemplateHelper.HasAttributeWithValue(HTML, "contenteditable", "false"))
                {
                    //add darkMagenta HighLight
                    runProperties.Highlight= new Highlight() { Val = HighlightColorValues.DarkMagenta };
                }
                if (WordTemplateHelper.HasAttributeWithValue(HTML, "variable"))
                {
                    //add darkMagenta HighLight
                    OpenXmlAttribute openXmlAttribute = new OpenXmlAttribute("Variable", "http://DMSNamespace", WordTemplateHelper.GetAttributeValue(HTML, "Variable"));
                    run.SetAttribute(openXmlAttribute);
                }
                if (WordTemplateHelper.TagStyleContainsAttribute(HTML, "font-weight", "bold"))
                {
                    

                    runProperties.Bold=new Bold();
                
                    
                }
                if (WordTemplateHelper.TagStyleContainsAttribute(HTML, "font-style", "italic"))
                {
                   

                    runProperties.Italic = new Italic(); 
                
                    
                
                }
                if(WordTemplateHelper.TagStyleContainsAttribute(HTML, "text-decoration", "underline"))
                {
                   

                    runProperties.Underline = new Underline() { Val = UnderlineValues.Single };
                }
                if (WordTemplateHelper.ContainsHtmlTags(HTML, "sub"))
                {
                    runProperties.VerticalTextAlignment = new VerticalTextAlignment() { Val = VerticalPositionValues.Subscript };

                    HTML = WordTemplateHelper.RemoveHtmlTags(HTML, "sub");
                }
                if (WordTemplateHelper.ContainsHtmlTags(HTML, "sup"))
                {
                  
                    runProperties.VerticalTextAlignment = new VerticalTextAlignment() { Val = VerticalPositionValues.Superscript };
                    HTML = WordTemplateHelper.RemoveHtmlTags(HTML, "sup");
                }

                runProperties.RunFonts= new RunFonts() { Ascii = "Times New Roman" };


                string startSeq = "font-size:";
                string endSeq = "px";

                string pattern = $"{Regex.Escape(startSeq)}(.*?){Regex.Escape(endSeq)}";

                Match match = Regex.Match(HTML, pattern);
                string extractedString = "24";
                if (match.Success)
                {
                    extractedString = match.Groups[1].Value;
                    int adjustedFontSize = int.Parse(extractedString);
                    extractedString = adjustedFontSize.ToString();
                        
                }


                
                runProperties.FontSize = new FontSize() { Val = extractedString };
                HTML = WordTemplateHelper.RemoveHtmlTags(HTML, "span");
                run.Append(runProperties);
                run.Append(new Text(WebUtility.HtmlDecode(HTML)) { Space = SpaceProcessingModeValues.Preserve });
                RunsResults.Add(run);
            }

            
            return RunsResults;
        }
    }
}
