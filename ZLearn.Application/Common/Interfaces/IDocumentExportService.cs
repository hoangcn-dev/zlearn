using System.Collections.Generic;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Common.Interfaces
{
    public interface IDocumentExportService
    {
        byte[] ExportQuizToWord(QuizExportDto quiz);
        byte[] ExportQuizToPdf(QuizExportDto quiz);
        byte[] CreateZipArchive(Dictionary<string, byte[]> files);
    }
}
