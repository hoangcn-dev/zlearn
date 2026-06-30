using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ZLearn.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Queries;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetListQuiz
{
    public class GetListQuizQueryHandler : BaseQueryHandler<QuizDocument>, IRequestHandler<GetListQuizQuery, PaginatedDto<QuizListItemDto>>
    {
        public GetListQuizQueryHandler(
            IReadRepo<QuizDocument> readRepo,
            IMapper mapper,
            IMediator mediator) : base(readRepo, mapper, mediator)
        {
        }

        public async Task<PaginatedDto<QuizListItemDto>> Handle(GetListQuizQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<QuizDocument, bool>> filter = q => 
                (string.IsNullOrEmpty(request.Name) || q.Name.Contains(request.Name)) &&
                (string.IsNullOrEmpty(request.CategorySlug) || q.CategorySlug == request.CategorySlug) &&
                (string.IsNullOrEmpty(request.ExcludeId) || q.Id != request.ExcludeId.ToUpper());

            var pagingResult = await _readRepo.GetPagingAsync(request.PageIndex, request.PageSize, filter);

            var listItems = pagingResult.Items.Select(q => new QuizListItemDto
            {
                Id = q.Id,
                Slug = q.Slug,
                Name = q.Name,
                AttemptCount = q.AttemptCount,
                CategoryId = q.CategoryId,
                CategoryName = q.CategoryName,
                CategorySLug = q.CategorySlug,
                DownloadCount = q.DownloadCount,
                QuestionCount = q.QuestionCount
            }).ToList();

            return new PaginatedDto<QuizListItemDto>
            {
                Items = listItems,
                TotalItems = pagingResult.TotalItems,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };
        }
    }
}
