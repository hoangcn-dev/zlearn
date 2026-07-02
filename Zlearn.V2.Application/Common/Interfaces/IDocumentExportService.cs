using System.Collections.Generic;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Common.Interfaces
{
    public interface IDocumentExportService
    {
        byte[] ExportQuizToWord(QuizExportDto quiz);
        byte[] ExportQuizToPdf(QuizExportDto quiz);
        byte[] CreateZipArchive(Dictionary<string, byte[]> files);
    }
}
