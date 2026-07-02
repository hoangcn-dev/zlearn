using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.ExportQuizzes
{
    public class ExportQuizzesQuery : IRequest<ExportDocumentDto>
    {
        public List<string> QuizIds { get; set; } = new List<string>();
        public string Format { get; set; } = null!;
    }

    public class ExportQuizzesQueryHandler : IRequestHandler<ExportQuizzesQuery, ExportDocumentDto>
    {
        private readonly IReadRepo<QuizDocument> _quizReadRepo;
        private readonly IWriteRepo<Zlearn.V2.Domain.CatalogContext.Quizzes.Quiz> _quizWriteRepo;
        private readonly IDocumentExportService _exportService;

        public ExportQuizzesQueryHandler(
            IReadRepo<QuizDocument> quizReadRepo, 
            IWriteRepo<Zlearn.V2.Domain.CatalogContext.Quizzes.Quiz> quizWriteRepo,
            IDocumentExportService exportService)
        {
            _quizReadRepo = quizReadRepo;
            _quizWriteRepo = quizWriteRepo;
            _exportService = exportService;
        }

        public async Task<ExportDocumentDto> Handle(ExportQuizzesQuery request, CancellationToken cancellationToken)
        {
            if (request.QuizIds == null || !request.QuizIds.Any())
            {
                throw new ArgumentException("Quiz IDs must be provided.");
            }

            var format = request.Format?.ToLower() == "word" ? "word" : "pdf";

            // Increment download count for each exported quiz
            foreach (var quizId in request.QuizIds)
            {
                var quizEntity = await _quizWriteRepo.GetByIdAsync(quizId);
                if (quizEntity != null)
                {
                    quizEntity.IncDownloadCount();
                    await _quizWriteRepo.SaveChangesAsync(cancellationToken);
                }
            }

            var quizzes = await _quizReadRepo.GetAllAsync(q => request.QuizIds.Contains(q.Id));

            if (!quizzes.Any())
            {
                throw new Exception("Quizzes not found");
            }

            var exportQuizzes = quizzes.Select(q => new QuizExportDto
            {
                Id = q.Id,
                Name = q.Name,
                Questions = q.Questions.Select(question => new QuestionExportDto
                {
                    Id = question.Id,
                    Content = question.StringContent ?? string.Empty,
                    Order = question.Order,
                    Answers = question.Answers.Select(answer => new AnswerExportDto
                    {
                        Id = answer.Id,
                        Content = answer.StringContent ?? string.Empty,
                        Key = answer.Key,
                        IsCorrect = answer.IsCorrect
                    }).OrderBy(a => a.Key).ToList()
                }).OrderBy(question => question.Order).ToList()
            }).ToList();

            if (exportQuizzes.Count == 1)
            {
                var quiz = exportQuizzes.First();
                byte[] content = format == "word" ? _exportService.ExportQuizToWord(quiz) : _exportService.ExportQuizToPdf(quiz);
                string ext = format == "word" ? "docx" : "pdf";
                string mime = format == "word" ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" : "application/pdf";
                
                return new ExportDocumentDto
                {
                    Content = content,
                    ContentType = mime,
                    FileName = $"{quiz.Name}.{ext}"
                };
            }
            else
            {
                var dict = new Dictionary<string, byte[]>();
                int index = 1;
                foreach (var quiz in exportQuizzes)
                {
                    byte[] content = format == "word" ? _exportService.ExportQuizToWord(quiz) : _exportService.ExportQuizToPdf(quiz);
                    string ext = format == "word" ? "docx" : "pdf";
                    string name = $"{quiz.Name}.{ext}";
                    if (dict.ContainsKey(name))
                    {
                        name = $"{quiz.Name} ({index}).{ext}";
                    }
                    dict[name] = content;
                    index++;
                }

                var zipContent = _exportService.CreateZipArchive(dict);
                return new ExportDocumentDto
                {
                    Content = zipContent,
                    ContentType = "application/zip",
                    FileName = "Quizzes.zip"
                };
            }
        }
    }
}
