using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DoucmentManagmentSys.Repo;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using System.Diagnostics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;

using A = DocumentFormat.OpenXml.Drawing;

using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Path = System.IO.Path;

using DocumentFormat.OpenXml.Drawing.Wordprocessing;

using ParagraphProperties = DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties;
using DoucmentManagmentSys.Models;

using DoucmentManagmentSys.Models;

using System.Text;

using WP = DocumentFormat.OpenXml.Drawing.Pictures;

using V = DocumentFormat.OpenXml.Vml;
using Picture = DocumentFormat.OpenXml.Wordprocessing.Picture;
using System.Text.RegularExpressions;

namespace DoucmentManagmentSys.Helpers.Word
{
    public class WordDocumentHelper
    {
        private static string imagePart1Data = "";

        private PrimacyDocument document;
        private MainDocumentPart? mainPart;
        private MemoryStream ms;

        private static readonly List<string> CodePatterns = new List<string>
        {
            @"RMS-A\d{3}-\d{0,3}",
            @"RMS-E\d{3}-\d{0,3}",
            @"RMS-V\d{3}-\d{0,3}",
            @"ARS-\d{3}-\d{0,3}",
            @"ASS-\d{3}-\d{0,3}",
            @"PMS-\d{3}-\d{0,3}"
        };

        public static bool IsValidCode(string input)
        {
            foreach (var pattern in CodePatterns)
            {
                if (Regex.IsMatch(input, pattern))
                {
                    return true;
                }
            }
            return false;
        }

        public WordDocumentHelper(PrimacyDocument document)
        {
            this.document = document;

            ms = new MemoryStream();

            //open memory stream

            ms.Write(document.Content, 0, document.Content.Length);
        }

        public static string ConvertDocxStreamToHtmlString(string file, string outputDirectory)
        {
            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(outputDirectory)) throw new Exception("Invalid parameters passed to convert word function.");

            if (!File.Exists(file)) throw new FileNotFoundException($"The file passed to the convert word process ({file}) could not be found.");

            if (!Directory.Exists(outputDirectory)) throw new DirectoryNotFoundException($"The output folder passed to the convert word process ({outputDirectory}) does not exist.");

            if (outputDirectory.EndsWith(@"\")) outputDirectory = outputDirectory[..^1];

            var fileInfo = new FileInfo(file);

            if (fileInfo.Extension.ToLower() == ".doc" && fileInfo.Extension.ToLower() == ".docx") throw new ArgumentOutOfRangeException($"The file type passed to the convert word process is an invalid type ({fileInfo.Extension}).");

            var outputFile = outputDirectory + @"\" + Path.GetFileNameWithoutExtension(fileInfo.Name) + ".pdf";

            if (File.Exists(outputFile)) File.Delete(outputFile);

            var libreOfficePath = GetLibreOfficePath();

            if (!File.Exists(libreOfficePath)) throw new FileNotFoundException("It seems that LibreOffice is not where it should be, please ensure the path exists.");

            var procStartInfo = new ProcessStartInfo(libreOfficePath, $@"--headless --convert-to html:HTML ""{file}"" --outdir ""{outputDirectory}""")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory
            };

            Process process = new() { StartInfo = procStartInfo };

            process.Start();

            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new LibreOfficeFailedException(process.ExitCode);

            if (!File.Exists(outputFile)) throw new FileNotFoundException("The convert to word process has failed to convert the file!");

            return outputFile;
        }

        public static byte[] ConvertToPdfAndReturnOutput(PrimacyDocument primacyDocument)
        {
            //if (Path.Exists($"./TempView/{primacyDocument.FileName}.pdf"))
            //{
            //    return $"./TempView/{primacyDocument.FileName}.pdf";
            //}
            if (FileTypes.IsFileTypeWord(primacyDocument.FileExtensiton))
            {
                //write primacyDocument.content to file in uploadedfiles

                var filePath = $"./TempView/{primacyDocument.FileName}{primacyDocument.FileExtensiton}";

                File.WriteAllBytes(filePath, primacyDocument.Content);

                //convert file to pdf
                var pdfFilePath = ConvertWordFile(filePath, "./TempView");
                //replace content with new file content
                File.Delete(filePath);
                var pdfFileBytes = File.ReadAllBytes(pdfFilePath);
                File.Delete(pdfFilePath);
                return pdfFileBytes;
            }
            else if (FileTypes.IsFileTypePdf(primacyDocument.FileExtensiton))
            {
                return primacyDocument.Content;
            }
            else
            {
                return null;
            }
        }

        public static void ConvertToPdfAndUpdate(PrimacyDocument primacyDocument)
        {
            if (FileTypes.IsFileTypeWord(primacyDocument.FileExtensiton))
            {
                //write primacyDocument.content to file in uploadedfiles

                var filePath = $"./UploadedFiles/{primacyDocument.FileName}{primacyDocument.FileExtensiton}";

                File.WriteAllBytes(filePath, primacyDocument.Content);

                //convert file to pdf
                var pdfFilePath = ConvertWordFile(filePath, "./UploadedFiles");
                //replace content with new file content
                primacyDocument.Content = File.ReadAllBytes(pdfFilePath);
                FileTypes.ChangeTypeTo(".pdf", primacyDocument);
                File.Delete(pdfFilePath);
                File.Delete(filePath);
            }
        }

        public static string ConvertWordFile(string file, string outputDirectory)
        {
            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(outputDirectory)) throw new Exception("Invalid parameters passed to convert word function.");

            if (!File.Exists(file)) throw new FileNotFoundException($"The file passed to the convert word process ({file}) could not be found.");

            if (!Directory.Exists(outputDirectory)) throw new DirectoryNotFoundException($"The output folder passed to the convert word process ({outputDirectory}) does not exist.");

            if (outputDirectory.EndsWith(@"\")) outputDirectory = outputDirectory[..^1];

            var fileInfo = new FileInfo(file);

            if (fileInfo.Extension.ToLower() == ".doc" && fileInfo.Extension.ToLower() == ".docx") throw new ArgumentOutOfRangeException($"The file type passed to the convert word process is an invalid type ({fileInfo.Extension}).");

            var outputFile = outputDirectory + @"\" + Path.GetFileNameWithoutExtension(fileInfo.Name) + ".pdf";

            if (File.Exists(outputFile)) File.Delete(outputFile);

            var libreOfficePath = GetLibreOfficePath();

            if (!File.Exists(libreOfficePath)) throw new FileNotFoundException("It seems that LibreOffice is not where it should be, please ensure the path exists.");

            var procStartInfo = new ProcessStartInfo(libreOfficePath, $@"--headless --convert-to pdf:writer_pdf_Export ""{file}"" --outdir ""{outputDirectory}""")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory
            };

            Process process = new() { StartInfo = procStartInfo };

            process.Start();

            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new LibreOfficeFailedException(process.ExitCode);

            if (!File.Exists(outputFile)) throw new FileNotFoundException("The convert to word process has failed to convert the file!");

            return outputFile;
        }

        public static void MakeMarginsNarrow(MainDocumentPart mainPart)
        {
            // Get the document settings part
            DocumentSettingsPart settingsPart = mainPart.DocumentSettingsPart;
            if (settingsPart == null)
            {
                settingsPart = mainPart.AddNewPart<DocumentSettingsPart>();
                settingsPart.Settings = new Settings();
            }

            // Get or create the section properties for the document
            SectionProperties sectionProperties = settingsPart.Settings.Descendants<SectionProperties>().FirstOrDefault();
            if (sectionProperties == null)
            {
                sectionProperties = new SectionProperties();
                settingsPart.Settings.Append(sectionProperties);
            }

            // Adjust the margins by setting the PageMargin properties
            PageMargin pageMargin = new PageMargin()
            {
                Left = 0,    // Narrow the left margin (default is 1440 = 1 inch)
                Right = 0,   // Narrow the right margin
                Top = 360,     // Narrow the top margin
                Bottom = 360   // Narrow the bottom margin
            };

            // Apply the updated PageMargin to the SectionProperties
            sectionProperties.Append(pageMargin);

            // Save the changes
            settingsPart.Settings.Save();
        }

        public static ImagePart PreAddImage(HeaderPart HeaderPart)
        {
            ImagePart imagePart = HeaderPart.AddImagePart(ImagePartType.Jpeg);
            using (FileStream stream = new FileStream(Path.GetFullPath("wwwroot/images/Watermark.png"), FileMode.Open))
            {
                imagePart.FeedData(stream);
            }
            return imagePart;
        }

        public static void SetWaterMarkPicture(string file)
        {
            FileStream inFile;
            try
            {
                inFile = new FileStream(file, FileMode.Open, FileAccess.Read);
                byte[] byteArray = new byte[inFile.Length];
                long byteRead = inFile.Read(byteArray, 0, (int)inFile.Length);
                inFile.Close();
                imagePart1Data = Convert.ToBase64String(byteArray, 0, byteArray.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AddWatermark()
        {
            try
            {
                using (WordprocessingDocument package = WordprocessingDocument.Open(this.ms, true))
                {
                    //CreateHeaderIfDoesntExist(package.MainDocumentPart);
                    //AddImageToHeader(package);
                    InsertCustomWatermark(package, @"F:\Documents\DoucmentManagmentSys\DoucmentManagmentSys\wwwroot\images\Watermark.png");
                    //save
                    package.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string? ExtractCode()
        {
            if (FileTypes.IsFileTypeWord(document.FileExtensiton))
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                {
                    MainDocumentPart mainPart = doc.MainDocumentPart ?? doc.AddMainDocumentPart();

                   

                    // Convert the document body to a string
                    string documentText = mainPart.Document.Body.InnerText;

                    // Search for the code in the document text
                    foreach (var pattern in CodePatterns)
                    {
                        var match = Regex.Match(documentText, pattern);
                        if (match.Success)
                        {
                            return match.Value;
                        }
                    }

                    // Return null if no code is found
                    return null;
                }
            }

            return null;
        }

        public void StampFooter(List<string> Newfooter)
        {
            //checked the document is word
            if (FileTypes.IsFileTypeWord(document.FileExtensiton))
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                {
                    MainDocumentPart mainPart = doc.MainDocumentPart ?? doc.AddMainDocumentPart();

                    CreateFooterIfDoesntExist(mainPart);

                    MakeMarginsNarrow(mainPart);
                    Document MainPartDoc = mainPart.Document;
                    // Get the first section properties
                    SectionProperties sectionProps = MainPartDoc.Body.Descendants<SectionProperties>().FirstOrDefault() ?? new SectionProperties();

                    if (sectionProps != null)
                    {
                        //Don't Delete this line, reason: https://stackoverflow.com/questions/73061394/adding-replacing-header-to-first-page-only-for-existing-word-document-with-openx
                        sectionProps.PrependChild(new TitlePage());

                        // Get the first footer reference for the first page
                        //there has to be a footer reference because we called the function CreateFooterIfDoesntExist
                        FooterReference firstPageFooterRef = sectionProps.Descendants<FooterReference>().FirstOrDefault(f => f.Type.HasValue && f.Type == HeaderFooterValues.Default)!;

                        if (firstPageFooterRef != null)
                        {
                            // Get the footer part for the first page
                            List<FooterPart> firstPageFooterPart = mainPart.FooterParts.ToList();

                            if (firstPageFooterPart != null)
                            {
                                foreach (FooterPart footerPart in firstPageFooterPart)
                                {
                                    // Modify the content of the first page footer
                                    Paragraph para = new Paragraph();

                                    //null safety on para

                                    Run run = para.AppendChild(new Run());
                                    run.AppendChild(new Text(string.Join(" ", Newfooter)) { Space = SpaceProcessingModeValues.Preserve });
                                    footerPart.Footer.Append(para);
                                }
                            }
                        }
                    }

                    // Save the changes
                    mainPart.Document.Save();
                }

                //Update Content
                document.Content = ms.ToArray();
            }
        }

        public void StampFooter(string Newfooter)
        {
            //checked the document is word
            if (FileTypes.IsFileTypeWord(document.FileExtensiton))
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                {
                    MainDocumentPart mainPart = doc.MainDocumentPart ?? doc.AddMainDocumentPart();

                    CreateFooterIfDoesntExist(mainPart);

                    MakeMarginsNarrow(mainPart);
                    Document MainPartDoc = mainPart.Document;
                    // Get the first section properties
                    SectionProperties sectionProps = MainPartDoc.Body.Descendants<SectionProperties>().FirstOrDefault() ?? new SectionProperties();

                    if (sectionProps != null)
                    {
                        //Don't Delete this line, reason: https://stackoverflow.com/questions/73061394/adding-replacing-header-to-first-page-only-for-existing-word-document-with-openx
                        //TitlePage titlePage = sectionProps.Descendants<TitlePage>().FirstOrDefault();
                        //if (titlePage == null)
                        //{
                        //    titlePage = new TitlePage();
                        //    sectionProps.PrependChild(titlePage);
                        //}

                        // Get the first footer reference for the first page
                        //there has to be a footer reference because we called the function CreateFooterIfDoesntExist
                        FooterReference firstPageFooterRef = sectionProps.Descendants<FooterReference>().FirstOrDefault(f => f.Type.HasValue && f.Type == HeaderFooterValues.Default)!;

                        if (firstPageFooterRef != null)
                        {
                            // Get the footer part for the first page
                            List<FooterPart> firstPageFooterPart = mainPart.FooterParts.ToList();

                            if (firstPageFooterPart != null)
                            {
                                foreach (FooterPart footerPart in firstPageFooterPart)
                                {
                                    // Modify the content of the first page footer
                                    Paragraph para = new Paragraph();

                                    //null safety on para

                                    Run run = para.AppendChild(new Run());
                                    run.AppendChild(new Text(Newfooter) { Space = SpaceProcessingModeValues.Preserve });
                                    footerPart.Footer.Append(para);
                                }
                            }
                        }
                    }

                    // Save the changes
                    mainPart.Document.Save();
                }

                //Update Content
                document.Content = ms.ToArray();
            }
        }

        internal void StampDocument(MainRepo<HistoryAction> _HistoryActionRepo, MainRepo<HistoryLog> _HistoryLogRepo)
        {
            //List<HistoryAction> LastActions = new List<HistoryAction>();
            ////from the audit log helper get every last action of each action type of this document
            //LastActions = AuditLogHelper.GetLatestActionsOfDocument(document, _HistoryActionRepo, _HistoryLogRepo);
            ////footer string is a list of strings of what should be written
            //List<string> footerStrings = new List<string>();
            //foreach (var action in LastActions)
            //{
            //    footerStrings.Add(action.Action + " by: " + action.UserName + "         ");
            //}

            HistoryAction? approvedBy = AuditLogHelper.GetLatestActionOfType("Approved",document, _HistoryActionRepo, _HistoryLogRepo);
            //update footer and header
            var footerString = "This Document was digitally signed , Approved by " + approvedBy.UserName + " SignCode:" + document.Id + approvedBy.id;
            StampFooter(footerString);
            if (!CheckFooter(footerString))
            {
                StampFooter(footerString);
            }
            //StampHeader();
        }

        private  bool CheckFooter(string CheckString)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, false))
            {
                foreach (FooterPart footerPart in doc.MainDocumentPart.FooterParts)
                {
                    foreach (Paragraph para in footerPart.Footer.Descendants<Paragraph>())
                    {
                        if (para.InnerText.Contains(CheckString))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static void AddImageToHeader(WordprocessingDocument doc)
        {
            HeaderPart headerPart = doc.MainDocumentPart.HeaderParts.FirstOrDefault();

            Paragraph para = headerPart.Header.Descendants<Paragraph>().First();

            ImagePart imagePart = PreAddImage(headerPart);
            var relationshipId = headerPart.GetIdOfPart(imagePart);
            // Assuming the image size is 5486400L x 3200400L (adjust as needed)
            long imageWidth = 798640L;
            long imageHeight = 798640L;

            // A4 page dimensions in EMUs
            long pageWidth = 11906000L;
            long pageHeight = 16838000L;

            // Calculate position to center the image
            long CentreOffsetX = (pageWidth - imageWidth) / 2;
            long CentreOffsetY = (pageHeight - imageHeight) / 2;
            var wrapSquare = new WrapSquare()
            {
                WrapText = WrapTextValues.BothSides // This sets the wrapping style to 'square'.
            };
            Run run = new Run();
            Drawing drawing = new Drawing(

           new DW.Anchor(
                 new SimplePosition() { X = 0, Y = 0 },
                 new HorizontalPosition(
                     new PositionOffset(CentreOffsetX.ToString()) // Use appropriate position offset
                 )
                 { RelativeFrom = HorizontalRelativePositionValues.Page },
                 new VerticalPosition(
                     new PositionOffset(CentreOffsetY.ToString()) // Use appropriate position offset
                 )
                 { RelativeFrom = VerticalRelativePositionValues.Page },
                 new Extent() { Cx = imageWidth, Cy = imageHeight }, // Set the size of the image (Cx = width, Cy = height)
                 new EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                 new DW.WrapNone(),
                 new DocProperties() { Id = (UInt32Value)1U, Name = "Picture 1" },
                 new DW.NonVisualGraphicFrameDrawingProperties(
                     new GraphicFrameLocks() { NoChangeAspect = true }),
                 new Graphic(
                     new GraphicData(
                         new PIC.Picture(
                             new NonVisualPictureProperties(
                                 new NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "New Image" },
                                 new NonVisualPictureDrawingProperties()),
                             new BlipFill(
                                 new AlphaReplace() { Alpha = 2000 },
                                 new Blip()
                                 { Embed = relationshipId },

                                 new Stretch(new FillRectangle())
                                 ),
                             new ShapeProperties(
                                 new Transform2D(
                                     new Offset() { X = 0L, Y = 0L },
                                     new Extents() { Cx = 512000L, Cy = 512000L }),
                                 new PresetGeometry(new AdjustValueList()) { Preset = ShapeTypeValues.Rectangle }))

                         )

                     )
                 { }

            )
           {
               DistanceFromTop = (UInt32Value)0U,
               DistanceFromBottom = (UInt32Value)0U,
               DistanceFromLeft = (UInt32Value)0U,
               DistanceFromRight = (UInt32Value)0U,

               SimplePos = false,
               RelativeHeight = (UInt32Value)0U,
               BehindDoc = true,
               Locked = false,
               LayoutInCell = true,
               AllowOverlap = true
           }

            )
                ;

            run.AppendChild(drawing);

            // Set the paragraph properties
            ParagraphProperties paragraphProperties = new ParagraphProperties();

            // Create the SpacingBetweenLines object
            SpacingBetweenLines spacing = new SpacingBetweenLines();

            // Set the line spacing - the value is defined in twentieths of a point (1/20 pt)
            // Example: 240 is equivalent to 12 points (single spacing)
            spacing.Line = "1080"; // This would set the spacing to double

            // Add the SpacingBetweenLines object to the ParagraphProperties
            paragraphProperties.Append(spacing);

            // Add the ParagraphProperties to the Paragraph
            para.PrependChild(paragraphProperties);
            para.AppendChild(run);
        }

        private static void CreateFooterIfDoesntExist(MainDocumentPart mainPart)
        {
            // Check if the document already has a footer part
            FooterPart existingFooterPart = mainPart.GetPartsOfType<FooterPart>().FirstOrDefault();

            if (existingFooterPart == null)
            {
                // Create a new footer part
                FooterPart newFooterPart = mainPart.AddNewPart<FooterPart>();
                string footerPartId = mainPart.GetIdOfPart(newFooterPart);

                // Create a new footer and add content
                Footer footer = new Footer();
                Paragraph para = new Paragraph();

                footer.Append(para);

                // Save the footer part
                newFooterPart.Footer = footer;

                // Get the first section properties
                SectionProperties sectionProps = mainPart.Document.Body.Descendants<SectionProperties>().FirstOrDefault();

                if (sectionProps != null)
                {
                    // Associate the footer part with the section
                    FooterReference footerRef = new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId };
                    sectionProps.Append(footerRef);
                }
            }
        }

        private static void CreateHeaderIfDoesntExist(MainDocumentPart mainPart)
        {
            // Check if the document already has a footer part
            HeaderPart existingHeaderPart = mainPart.GetPartsOfType<HeaderPart>().FirstOrDefault();

            if (existingHeaderPart == null)
            {
                // Create a new footer part
                HeaderPart newHeaderPart = mainPart.AddNewPart<HeaderPart>();
                string HeaderPartId = mainPart.GetIdOfPart(newHeaderPart);

                // Create a new footer and add content
                Header header = new Header();
                Paragraph para = new Paragraph();

                header.Append(para);

                // Save the footer part
                newHeaderPart.Header = header;

                // Get the first section properties
                SectionProperties sectionProps = mainPart.Document.Body.Descendants<SectionProperties>().FirstOrDefault();

                if (sectionProps != null)
                {
                    // Associate the footer part with the section
                    HeaderReference headerRef = new HeaderReference() { Type = HeaderFooterValues.First, Id = HeaderPartId };
                    if (sectionProps.GetFirstChild<TitlePage>() == null)
                    {
                        sectionProps.PrependChild(new TitlePage());
                    }

                    sectionProps.Append(headerRef);
                }
            }
        }

        private static void GenerateHeaderPart1Content(HeaderPart headerPart1)
        {
            Header header1 = new Header();
            Paragraph paragraph2 = new Paragraph();
            Run run1 = new Run();
            Picture picture1 = new Picture();
            V.Shape shape1 = new V.Shape() { Id = "WordPictureWatermark75517470", Style = "position:absolute;left:0;text-align:left;margin-left:0;margin-top:0;width:415.2pt;height:456.15pt;z-index:-251656192;mso-position-horizontal:center;mso-position-horizontal-relative:margin;mso-position-vertical:center;mso-position-vertical-relative:margin", OptionalString = "_x0000_s2051", AllowInCell = false, Type = "#_x0000_t75" };
            V.ImageData imageData1 = new V.ImageData() { Gain = "19661f", BlackLevel = "22938f", Title = "??", RelationshipId = "rId999" };
            shape1.Append(imageData1);
            picture1.Append(shape1);
            run1.Append(picture1);
            paragraph2.Append(run1);
            header1.Append(paragraph2);
            headerPart1.Header = header1;
        }

        private static void GenerateImagePart1Content(ImagePart imagePart1)
        {
            System.IO.Stream data = GetBinaryDataStream(imagePart1Data);
            imagePart1.FeedData(data);
            data.Close();
        }

        private static System.IO.Stream GetBinaryDataStream(string base64String)
        {
            return new System.IO.MemoryStream(System.Convert.FromBase64String(base64String));
        }

        private static string GetLibreOfficePath() => ConfigurationHelper.GetString("LibreOfficePath");

        private static void InsertCustomWatermark(WordprocessingDocument package, string p)
        {
            SetWaterMarkPicture(p);
            MainDocumentPart mainDocumentPart1 = package.MainDocumentPart;
            if (mainDocumentPart1 != null)
            {
                mainDocumentPart1.DeleteParts(mainDocumentPart1.HeaderParts);
                HeaderPart headPart1 = mainDocumentPart1.AddNewPart<HeaderPart>();
                GenerateHeaderPart1Content(headPart1);
                string rId = mainDocumentPart1.GetIdOfPart(headPart1);
                ImagePart image = headPart1.AddNewPart<ImagePart>("image/png", "rId999");
                GenerateImagePart1Content(image);
                IEnumerable<SectionProperties> sectPrs = mainDocumentPart1.Document.Body.Elements<SectionProperties>();
                foreach (var sectPr in sectPrs)
                {
                    sectPr.RemoveAllChildren<HeaderReference>();
                    sectPr.PrependChild(new HeaderReference() { Id = rId });
                }
            }
        }

        private void StampHeader()
        {
            //checked the document is word
            if (FileTypes.IsFileTypeWord(document.FileExtensiton))
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                {
                    MainDocumentPart mainPart = doc.MainDocumentPart ?? doc.AddMainDocumentPart();

                    CreateHeaderIfDoesntExist(mainPart);
                    Document MainPartDoc = mainPart.Document;
                    // Get the first section properties
                    SectionProperties sectionProps = MainPartDoc.Body.Descendants<SectionProperties>().FirstOrDefault() ?? new SectionProperties();

                    if (sectionProps != null)
                    {
                        //could cause an error
                        //Don't Delete this line, reason: https://stackoverflow.com/questions/73061394/adding-replacing-header-to-first-page-only-for-existing-word-document-with-openx
                        sectionProps.PrependChild(new TitlePage());

                        AddImageToHeader(doc);
                        //AddWatermark();
                    }

                    // Save the changes
                    mainPart.Document.Save();
                }
                //Update Content
                document.Content = ms.ToArray();
            }
        }

        public class LibreOfficeFailedException : Exception
        {
            public LibreOfficeFailedException(int exitCode) : base($"LibreOffice has failed with {exitCode}")
            {
            }
        }
    }
}