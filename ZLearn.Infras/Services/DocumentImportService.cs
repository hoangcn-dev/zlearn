using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using ZLearn.Application.Common.Interfaces;

namespace ZLearn.Infras.Services
{
    public class DocumentImportService : IDocumentImportService
    {
        public async Task<string> ExtractTextFromFileAsync(Stream fileStream, string contentType)
        {
            string rawText = string.Empty;

            if (contentType == "application/pdf" || contentType.EndsWith("pdf", StringComparison.OrdinalIgnoreCase))
            {
                rawText = ExtractFromPdf(fileStream);
            }
            else if (contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" || 
                     contentType.EndsWith("docx", StringComparison.OrdinalIgnoreCase) || 
                     contentType.EndsWith("msword", StringComparison.OrdinalIgnoreCase))
            {
                rawText = ExtractFromWord(fileStream);
            }
            else
            {
                throw new InvalidOperationException("Unsupported file format.");
            }

            return CleanText(rawText);
        }

        private string ExtractFromPdf(Stream stream)
        {
            var sb = new StringBuilder();
            using (var pdfDocument = PdfDocument.Open(stream))
            {
                foreach (var page in pdfDocument.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
            }
            return sb.ToString();
        }

        private string ExtractFromWord(Stream stream)
        {
            var sb = new StringBuilder();
            using (var wordDocument = WordprocessingDocument.Open(stream, false))
            {
                var body = wordDocument.MainDocumentPart?.Document.Body;
                if (body != null)
                {
                    foreach (var paragraph in body.Elements<Paragraph>())
                    {
                        sb.AppendLine(paragraph.InnerText);
                    }
                }
            }
            return sb.ToString();
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Remove multiple blank lines, replace with single newline
            text = Regex.Replace(text, @"(\r\n|\n|\r)+", "\n");
            
            // Remove excessive whitespace between words
            text = Regex.Replace(text, @"[ \t]+", " ");

            // Trim each line
            var lines = text.Split('\n');
            var cleanedLines = new StringBuilder();
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    cleanedLines.AppendLine(trimmed);
                }
            }

            return cleanedLines.ToString().Trim();
        }
    }
}
