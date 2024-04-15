﻿
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Repo;
/*
using Humanizer;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Spire.Doc;
using Spire.Doc.Documents;
using SParagraph = Spire.Doc.Documents.Paragraph;
using SDocument = Spire.Doc.Document;
using Spire.Doc.Fields;
*/
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using System.Diagnostics;
using OpenXmlPowerTools;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using NPOI.SS.Formula.Functions;
using DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using RunProperties = DocumentFormat.OpenXml.Wordprocessing.RunProperties;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Path = System.IO.Path;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using ParagraphProperties = DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties;
using Position = DocumentFormat.OpenXml.Wordprocessing.Position;

namespace DoucmentManagmentSys.Controllers.Helpers
{
    public class WordDocumentHelper
    {
        public WordDocumentHelper() { }

        private static string GetLibreOfficePath() => @"C:\LibreOfficePortable\App\libreoffice\program\swriter.exe";

        public class LibreOfficeFailedException : Exception
        {
            public LibreOfficeFailedException(int exitCode) : base($"LibreOffice has failed with {exitCode}") { }
        }

        public static void InsertToFooter(List<string> Newfooter, PrimacyDocument document)
        {
            //checked the document is word 
            if (FileTypes.IsFileTypeWord(document.FileExtensiton))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    //open memory stream
                    ms.Write(document.Content, 0, document.Content.Length);
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                    {
                        // Get the main document part
                        var mainPart = doc.MainDocumentPart;
                        CreateFooterIfDoesntExist(mainPart);
                        CreateHeaderIfDoesntExist(mainPart);
                        MakeMarginsNarrow(mainPart);
                        Document documentt = mainPart.Document;
                        // Get the first section properties
                        SectionProperties sectionProps = documentt.Body.Descendants<SectionProperties>().FirstOrDefault();



                        if (sectionProps != null)
                        {
                            //Don't Delete this line, reason: https://stackoverflow.com/questions/73061394/adding-replacing-header-to-first-page-only-for-existing-word-document-with-openx
                            sectionProps.PrependChild<TitlePage>(new TitlePage());
                            AddImageToHeader(doc);


                            // Get the first footer reference for the first page
                            FooterReference firstPageFooterRef = sectionProps.Descendants<FooterReference>().FirstOrDefault(f => f.Type.HasValue && f.Type == HeaderFooterValues.Default);

                            if (firstPageFooterRef != null)
                            {
                                // Get the footer part for the first page
                                List<FooterPart> firstPageFooterPart = mainPart.FooterParts.ToList();

                                if (firstPageFooterPart != null)
                                {
                                    foreach (FooterPart footerPart in firstPageFooterPart) { 
                                    // Modify the content of the first page footer
                                    foreach (Paragraph para in footerPart.Footer.Descendants<Paragraph>())
                                    {
                                        
                                        
                                        foreach (string content in Newfooter)
                                        {
                                            Run run = para.AppendChild(new Run());
                                            run.AppendChild(new Text(content) { Space = SpaceProcessingModeValues.Preserve });


                                        }
                                        
                                    }
}
                                }



                            }

                        }





                        // Save the changes
                        mainPart.Document.Save();



                    }

                    //Update Contnent
                    document.Content = ms.ToArray();

                    //close memory stream
                    ms.Close();
                }



            }


        }

        private static void CreateHeaderIfDoesntExist(MainDocumentPart? mainPart)
        {
            // Check if the document already has a footer part
            HeaderPart existingHeaderPart = mainPart.GetPartsOfType<HeaderPart>().FirstOrDefault();

            if (existingHeaderPart == null)
            {
                // Create a new footer part
                HeaderPart newHeaderPart = mainPart.AddNewPart<HeaderPart>();
                string footerPartId = mainPart.GetIdOfPart(newHeaderPart);

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
                    HeaderReference headerRef = new HeaderReference() { Type = HeaderFooterValues.First, Id = footerPartId };
                    sectionProps.Append(headerRef);
                }
            }
            
        }

        private static void AddImageToHeader(WordprocessingDocument doc)
        {
            HeaderPart headerPart = doc.MainDocumentPart.HeaderParts.FirstOrDefault();

            foreach (Paragraph para in headerPart.Header.Descendants<Paragraph>()) {

                ImagePart imagePart = PreAddImage(headerPart);
                var relationshipId = headerPart.GetIdOfPart(imagePart);
                var wrapSquare = new WrapSquare()
                {
                    WrapText = WrapTextValues.BothSides // This sets the wrapping style to 'square'.
                };
                Run run = new Run();
                Drawing drawing = new Drawing(

               new DW.Inline(
                     new DW.SimplePosition() { X = 0, Y = 0 },
                     new DW.HorizontalPosition(
                         new DW.PositionOffset("689900") // Use appropriate position offset
                     )
                     { RelativeFrom = DW.HorizontalRelativePositionValues.Page },
                     new DW.VerticalPosition(
                         new DW.PositionOffset("0") // Use appropriate position offset
                     )
                     { RelativeFrom = DW.VerticalRelativePositionValues.BottomMargin },
                     new DW.Extent() { Cx = 512000L * 2, Cy = 512000L }, // Set the size of the image (Cx = width, Cy = height)
                     new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                     wrapSquare,
                     new DW.DocProperties() { Id = (UInt32Value)1U, Name = "Picture 1" },
                     new DW.NonVisualGraphicFrameDrawingProperties(
                         new GraphicFrameLocks() { NoChangeAspect = true }),
                     new Graphic(
                         new GraphicData(
                             new PIC.Picture(
                                 new PIC.NonVisualPictureProperties(
                                     new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "New Image" },
                                     new PIC.NonVisualPictureDrawingProperties()),
                                 new PIC.BlipFill(
                                     new Blip() { Embed = relationshipId },
                                     new Stretch(new FillRectangle())),
                                 new PIC.ShapeProperties(
                                     new Transform2D(
                                         new A.Offset() { X = 0L, Y = 0L },
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
        }

        private static void CreateFooterIfDoesntExist(MainDocumentPart? mainPart)
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
        internal static void AddFinalFooter(PrimacyDocument doc, MainRepo<HistoryAction> _HistoryActionRepo, MainRepo<HistoryLog> _HistoryLogRepo)
        {
            List<HistoryAction> LastActions = new List<HistoryAction>();
            //from the aditlog helper get every last action of each action type of this document
            LastActions = AuditLogHelper.GetLatestActionsOfDocument(doc, _HistoryActionRepo, _HistoryLogRepo);
            //footer string is a list of strings of what should be written
            List<string> footerStrings = new List<string>();
            foreach (var action in LastActions)
            {
                footerStrings.Add(action.Action + " by: " + action.UserName + " ");
            }

            //update footer
            InsertToFooter(footerStrings, doc);

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

        public static void MakeMarginsNarrow(MainDocumentPart mainPart) {
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

        public static ImagePart PreAddImage(HeaderPart HeaderPart) {
            ImagePart imagePart = HeaderPart.AddImagePart(ImagePartType.Jpeg);
            using (FileStream stream = new FileStream("F:\\Documents\\DoucmentManagmentSys\\DoucmentManagmentSys\\wwwroot\\logo.jpg", FileMode.Open))
            {
                imagePart.FeedData(stream);
            }
            return  imagePart;

        }





    }
}
