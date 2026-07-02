using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Infas.Services
{
    public class DocumentExportService : IDocumentExportService
    {
        public DocumentExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] ExportQuizToWord(QuizExportDto quiz)
        {
            using var memoryStream = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Title
                var titlePara = body.AppendChild(new Paragraph());
                var titleRun = titlePara.AppendChild(new Run());
                titleRun.AppendChild(new Text(quiz.Name));
                titleRun.RunProperties = new RunProperties(new Bold(), new FontSize { Val = "48" }); // 24pt

                foreach (var question in quiz.Questions)
                {
                    var qPara = body.AppendChild(new Paragraph());
                    var qRun = qPara.AppendChild(new Run());
                    qRun.AppendChild(new Text($"Câu {question.Order}: {question.Content}"));
                    qRun.RunProperties = new RunProperties(new Bold());

                    int answerChar = 65; // 'A'
                    foreach (var answer in question.Answers)
                    {
                        var aPara = body.AppendChild(new Paragraph());
                        var aRun = aPara.AppendChild(new Run());
                        aRun.AppendChild(new Text($"{(char)answerChar}. {answer.Content}"));
                        
                        if (answer.IsCorrect)
                        {
                            aRun.RunProperties = new RunProperties(new Underline { Val = UnderlineValues.Single });
                        }
                        answerChar++;
                    }
                    
                    body.AppendChild(new Paragraph(new Run(new Text("")))); // Empty line
                }
            }
            return memoryStream.ToArray();
        }

        public byte[] ExportQuizToPdf(QuizExportDto quiz)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(QuestPDF.Helpers.Fonts.Arial));

                    page.Header().Text(quiz.Name).SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Column(col =>
                    {
                        foreach (var question in quiz.Questions)
                        {
                            col.Item().PaddingBottom(5).Text($"Câu {question.Order}: {question.Content}").Bold();
                            
                            int answerChar = 65; // 'A'
                            foreach (var answer in question.Answers)
                            {
                                var textContent = $"{(char)answerChar}. {answer.Content}";
                                if (answer.IsCorrect)
                                {
                                    col.Item().Text(textContent).Underline();
                                }
                                else
                                {
                                    col.Item().Text(textContent);
                                }
                                answerChar++;
                            }
                            col.Item().PaddingBottom(10);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] CreateZipArchive(Dictionary<string, byte[]> files)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(file.Key, CompressionLevel.Fastest);
                    using var entryStream = entry.Open();
                    entryStream.Write(file.Value, 0, file.Value.Length);
                }
            }
            return memoryStream.ToArray();
        }
    }
}
