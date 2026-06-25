using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.ExportQuizzes
{
    public class ExportQuizzesQueryHandler : BaseQueryHandler, IRequestHandler<ExportQuizzesQuery, ExportDocumentDto>
    {
        private readonly IQuizRepo _quizRepo;
        private readonly IDocumentExportService _exportService;

        public ExportQuizzesQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IQuizRepo quizRepo,
            IDocumentExportService exportService) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
            _exportService = exportService;
        }

        public async Task<ExportDocumentDto> Handle(ExportQuizzesQuery request, CancellationToken cancellationToken)
        {
            if (request.QuizIds == null || !request.QuizIds.Any())
            {
                throw new ArgumentException("Quiz IDs must be provided.");
            }

            var format = request.Format?.ToLower() == "word" ? "word" : "pdf";

            var filterBuilder = new FilterBuilder<Quiz>();
            filterBuilder.AndCondition(q => request.QuizIds.Contains(q.Id));

            var quizzes = await _quizRepo.GetAll(
                filter: filterBuilder.GetPredicateOrDefault(),
                projector: q => new QuizExportDto
                {
                    Id = q.Id,
                    Name = q.Name,
                    Questions = q.Questions.Select(question => new QuestionExportDto
                    {
                        Id = question.Id,
                        Content = question.StringContent,
                        Order = question.Order,
                        Answers = question.Answers.Select(answer => new AnswerExportDto
                        {
                            Id = answer.Id,
                            Content = answer.StringContent,
                            Key = answer.Key,
                            IsCorrect = answer.IsCorrect
                        }).OrderBy(a => a.Key).ToList()
                    }).OrderBy(question => question.Order).ToList()
                }
            );

            if (!quizzes.Any())
            {
                throw new NotFoundException(nameof(Quiz));
            }

            if (quizzes.Count == 1)
            {
                var quiz = quizzes.First();
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
                foreach (var quiz in quizzes)
                {
                    byte[] content = format == "word" ? _exportService.ExportQuizToWord(quiz) : _exportService.ExportQuizToPdf(quiz);
                    string ext = format == "word" ? "docx" : "pdf";
                    // handle duplicate names
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
