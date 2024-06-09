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
            //now we have parse each tag top-down
            if (CKTopLevelElement.Contains("ul"))
            {
                var newText = WordTemplateHelper.RemoveHtmlTags(CK, "ul");
                //we have to parse each <li> </li>
                foreach (var item in newText.Split("<li>"))
                {
                    if (item != "")
                    {
                        var MidLevelElement = item.Replace("</li>", "");
                        //the result paragraph must be have a bullet point property
                        var LowerLevelElement = 
                    }

                }

            }
            else if(CKTopLevelElement.Contains("p"))
            {
                
            }
            else
            {
                throw new Exception("Unconvertable CK Element:" + CKTopLevelElement);
            }

        }
        public static string ExtractFirstTag(string HTML)
        {
            var match = Regex.Match(HTML, @"<(.*?)>(.*?)<\/\1>", RegexOptions.Singleline);
            return match.Success ? match.Value : string.Empty;
        }
        public static OpenXmlElement ConstructLowLevelElement(string HTML)
        {
            //we have to check if there is a strong tag then the Run must have a bold property
            //if there is i tag then the Run must have an italics property
            var run = new Run();
            var runProperties = new RunProperties();
            if (HTML.Contains("strong"))
            {
                 // Create new run properties
                var bold = new Bold(); // Create a new bold property

                runProperties.Append(bold); // Add the bold property to the run properties
                
                HTML = WordTemplateHelper.RemoveHtmlTags(HTML, "strong");
            }
            if (HTML.Contains("i"))
            {
                // Create new run properties
                var italics = new Italic(); // Create a new italics property

                runProperties.Append(italics); // Add the italics property to the run properties
                
                HTML = WordTemplateHelper.RemoveHtmlTags(HTML, "i");
                
            }
            run.Append(runProperties);
            run.Append(new Text(HTML));
            return run;
        }
    }
}
