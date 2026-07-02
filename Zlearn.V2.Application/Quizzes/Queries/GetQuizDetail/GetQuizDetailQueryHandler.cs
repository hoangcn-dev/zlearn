using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Queries;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetQuizDetail
{
    public class GetQuizDetailQueryHandler : BaseQueryHandler<QuizDocument>, IRequestHandler<GetQuizDetailQuery, QuizDetailDto>
    {
        public GetQuizDetailQueryHandler(
            IReadRepo<QuizDocument> readRepo,
            IMapper mapper,
            IMediator mediator) : base(readRepo, mapper, mediator)
        {
        }

        public async Task<QuizDetailDto> Handle(GetQuizDetailQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Slug) && string.IsNullOrEmpty(request.Id))
            {
                throw new ArgumentException("Quiz ID or Slug must be provided.");
            }

            var quizzes = await _readRepo.GetAllAsync(q => 
                (!string.IsNullOrEmpty(request.Slug) && q.Slug == request.Slug) || 
                (!string.IsNullOrEmpty(request.Id) && q.Id == request.Id.ToUpper())
            );

            var quizDoc = quizzes.FirstOrDefault()
                ?? throw new NotFoundException(nameof(QuizDocument), request.Id ?? request.Slug ?? string.Empty);

            return new QuizDetailDto
            {
                Id = quizDoc.Id,
                Name = quizDoc.Name,
                Slug = quizDoc.Slug,
                AttemptCount = quizDoc.AttemptCount,
                CategoryId = quizDoc.CategoryId,
                CategorySLug = quizDoc.CategorySlug,
                CategoryName = quizDoc.CategoryName,
                QuestionCount = quizDoc.QuestionCount,
                Questions = quizDoc.Questions.Select(q => new QuestionListItemDto
                {
                    Id = q.Id,
                    Slug = q.Slug,
                    Order = q.Order,
                    Content = q.StringContent ?? "Nội dung media",
                }).OrderBy(q => q.Order).ToList()
            };
        }
    }
}

