
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
                        Document documentt = mainPart.Document;

                        // Get the first section properties
                        SectionProperties sectionProps = documentt.Body.Descendants<SectionProperties>().FirstOrDefault();
                        

                        if (sectionProps != null)
                        {
                            //Don't Delete this line, reason: https://stackoverflow.com/questions/73061394/adding-replacing-header-to-first-page-only-for-existing-word-document-with-openx
                            sectionProps.PrependChild<TitlePage>(new TitlePage());

                            // Get the first footer reference for the first page
                            FooterReference firstPageFooterRef = sectionProps.Descendants<FooterReference>().FirstOrDefault(f => f.Type.HasValue && f.Type == HeaderFooterValues.First);

                            if (firstPageFooterRef != null)
                            {
                                // Get the footer part for the first page
                                FooterPart firstPageFooterPart = mainPart.GetPartById(firstPageFooterRef.Id) as FooterPart;

                                if (firstPageFooterPart != null)
                                {
                                    // Modify the content of the first page footer
                                    foreach (Paragraph para in firstPageFooterPart.Footer.Descendants<Paragraph>())
                                    {
                                        foreach (string content in Newfooter) {
                                            Run run = para.AppendChild(new Run());
                                            run.AppendChild(new Text(content));
                                            run.AppendChild(new Break());

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
                    FooterReference footerRef = new FooterReference() { Type = HeaderFooterValues.First, Id = footerPartId };
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
                FileTypes.ChangeTypeTo(".pdf",primacyDocument);
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
            foreach (var action in LastActions) { 
                footerStrings.Add( action.Action +" by: " + action.UserName);
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
        //public static List<MemoryStream> SplitDocIntoStreams(MemoryStream wordDocStream)
        //{
        //    List<MemoryStream> splitStreams = new List<MemoryStream>();

        //    SDocument document = new SDocument();
        //    document.LoadFromStream(wordDocStream, FileFormat.Docx);

        //    int currentPageCount = 0;
        //    int totalPages = document.PageCount;
        //    int pageIndex = 0;

        //    while (currentPageCount < totalPages)
        //    {
        //        try
        //        {
        //            MemoryStream stream = new MemoryStream();
        //            SDocument newDocument = new SDocument();
        //            while (currentPageCount < totalPages && newDocument.PageCount < 3)
        //            {
        //                Section section = newDocument.AddSection();

        //                foreach (DocumentObject obj in document.Sections[pageIndex].Body.ChildObjects)
        //                {
        //                    if (obj.DocumentObjectType == DocumentObjectType.Paragraph && obj.FirstChild.DocumentObjectType== DocumentObjectType.TextRange)
        //                    {
        //                      var FirstChild = obj.FirstChild as TextRange;
        //                        if (FirstChild.Text == "") {
        //                            //add line break
        //                            section.AddParagraph().AppendBreak(BreakType.LineBreak);
        //                        }

        //                    }



        //                    section.Body.ChildObjects.Add(obj.Clone());


        //                }
        //                currentPageCount += newDocument.PageCount;
        //                pageIndex++;

        //            }

        //            newDocument.SaveToStream(stream, FileFormat.Docx);
        //            splitStreams.Add(stream);
        //        }
        //        catch (ArgumentOutOfRangeException e)
        //        {
        //            break;
        //        }
        //    }


        //    return splitStreams;
        //}
        //private static byte[] ConvertToPdf(Stream s)
        //{
        //    try
        //    {
        //        string savePath = Path.GetTempPath() + Guid.NewGuid() + ".pdf";
        //        SDocument document = new SDocument(s, FileFormat.Auto);
        //        document.SaveToFile(savePath, FileFormat.PDF);

        //        return File.ReadAllBytes(savePath);


        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        //public static MemoryStream ConcatenatePdfs(List<MemoryStream> pdfStreams)
        //{
        //    MemoryStream outputPdfStream = new MemoryStream();

        //    using (PdfDocument outputDocument = new PdfDocument(new PdfWriter(outputPdfStream)))
        //    {
        //        foreach (MemoryStream pdfStream in pdfStreams)
        //        {
        //            pdfStream.Seek(0, SeekOrigin.Begin); // Reset the position of the MemoryStream

        //            using (PdfDocument inputDocument = new PdfDocument(new PdfReader(pdfStream)))
        //            {
        //                for (int i = 1; i <= inputDocument.GetNumberOfPages(); i++)
        //                {
        //                    outputDocument.AddPage(inputDocument.GetPage(i).CopyTo(outputDocument));
        //                }
        //            }
        //        }
        //    }

        //    return outputPdfStream;
        //}

        //public static string SaveMemoryStreamAsPdf(MemoryStream pdfStream, string outputFilePath)
        //{
        //    File.WriteAllBytes(outputFilePath, pdfStream.ToArray());
        //    return outputFilePath;
        //}




    }
}
