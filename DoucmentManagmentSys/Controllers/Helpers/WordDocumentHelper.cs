
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DoucmentManagmentSys.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Spire.Doc;
using SDocument = Spire.Doc.Document;

namespace DoucmentManagmentSys.Controllers.Helpers
{
    public class WordDocumentHelper
    {
        public WordDocumentHelper() { }

        public static PrimacyDocument InsertToFooter(string Newfooter, PrimacyDocument document) 
        {
            //checked the document is word 
            if (FileTypes.IsFileTypeWord(document.FileName))
            {
                using (MemoryStream ms = new MemoryStream(document.Content))
                {
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, true))
                    {
                        // Get the main document part
                        var mainPart = doc.MainDocumentPart;

                        // Get the footer part
                        FooterPart footerPart = mainPart.FooterParts.FirstOrDefault();
                        if (footerPart != null)
                        {
                            var footer = footerPart.Footer;

                            // Create a new paragraph and run for the text
                            Paragraph paragraph = new Paragraph(new Run(new Text(Newfooter)));

                            // Append the paragraph to the footer
                            footer.Append(paragraph);
                        }

                        // Save the changes
                        mainPart.Document.Save();
                    }
                    //Update Contnent
                     document.Content = ms.ToArray();
                }
              


            }

            return document;


        }
        public static void ConvertDocxStreamToPdfAndUpdateContent(PrimacyDocument document)
        {
            if (FileTypes.IsFileTypeWord(document.FileName))
            {
                using MemoryStream docxStream = new MemoryStream(document.Content);
                List<MemoryStream> splitWordStreams=SplitDocIntoStreams(docxStream);
                
                MemoryStream mergedPdfStream = new MemoryStream();
                foreach (MemoryStream stream in splitWordStreams)
                {
                    using MemoryStream pdfStream = new MemoryStream(ConvertToPdf(stream));
                    pdfStream.CopyTo(mergedPdfStream);
                }
                document.Content = mergedPdfStream.ToArray();
                //FileTypes.ChangeTypeTo(".pdf", document);
            }
        }
        public static List<MemoryStream> SplitDocIntoStreams(MemoryStream wordDocStream)
        {
            List<MemoryStream> splitStreams = new List<MemoryStream>();

            SDocument document = new SDocument();
            document.LoadFromStream(wordDocStream, FileFormat.Docx);

            int currentPageCount = 0;
            int totalPages = document.PageCount;
            int pageIndex = 0;

            while (currentPageCount < totalPages)
            {
                MemoryStream stream = new MemoryStream();
                SDocument newDocument = new SDocument();
                while (currentPageCount < totalPages && newDocument.PageCount < 3)
                {
                    Section section = newDocument.AddSection();
                    foreach (DocumentObject obj in document.Sections[pageIndex].Body.ChildObjects)
                    {
                        section.Body.ChildObjects.Add(obj.Clone());
                    }
                    currentPageCount += newDocument.PageCount;
                    pageIndex++;
                }
                newDocument.SaveToStream(stream, FileFormat.Docx);
                splitStreams.Add(stream);
            }

            return splitStreams;
        }
        private static byte[] ConvertToPdf(Stream s)
        {
            try
            {
                    string savePath =  Path.GetTempPath() + Guid.NewGuid() + ".pdf";
                    SDocument document = new SDocument(s, FileFormat.Auto);
                    document.SaveToFile(savePath, FileFormat.PDF);

                return File.ReadAllBytes(savePath);


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static MemoryStream ConcatenatePdfs(List<MemoryStream> pdfStreams)
        {
            MemoryStream outputPdfStream = new MemoryStream();

            using (PdfDocument outputDocument = new PdfDocument(new PdfWriter(outputPdfStream)))
            {
                foreach (MemoryStream pdfStream in pdfStreams)
                {
                    pdfStream.Seek(0, SeekOrigin.Begin); // Reset the position of the MemoryStream

                    using (PdfDocument inputDocument = new PdfDocument(new PdfReader(pdfStream)))
                    {
                        for (int i = 1; i <= inputDocument.GetNumberOfPages(); i++)
                        {
                            outputDocument.AddPage(inputDocument.GetPage(i).CopyTo(outputDocument));
                        }
                    }
                }
            }

            return outputPdfStream;
        }

        public static string SaveMemoryStreamAsPdf(MemoryStream pdfStream, string outputFilePath)
        {
            File.WriteAllBytes(outputFilePath, pdfStream.ToArray());
            return outputFilePath;
        }
    }
}
