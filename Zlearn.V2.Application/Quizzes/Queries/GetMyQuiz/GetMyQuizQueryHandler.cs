using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Queries;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetMyQuiz
{
    public class GetMyQuizQueryHandler : BaseQueryHandler<QuizDocument>, IRequestHandler<GetMyQuizQuery, PaginatedDto<QuizListItemDto>>
    {
        public GetMyQuizQueryHandler(
            IReadRepo<QuizDocument> readRepo,
            IMapper mapper,
            IMediator mediator) : base(readRepo, mapper, mediator)
        {
        }

        public async Task<PaginatedDto<QuizListItemDto>> Handle(GetMyQuizQuery request, CancellationToken cancellationToken)
        {
            // Build filter predicate for QuizDocument in MongoDB
            Expression<Func<QuizDocument, bool>> filter = q =>
                q.CreatedBy == request.OwnerId &&
                (string.IsNullOrEmpty(request.Params.Name) || q.Name.Contains(request.Params.Name)) &&
                (string.IsNullOrEmpty(request.Params.CategoryId) || q.CategoryId == request.Params.CategoryId) &&
                (string.IsNullOrEmpty(request.Params.Tag) || q.Tags.Contains(request.Params.Tag));

            var pagingResult = await _readRepo.GetPagingAsync(request.Params.PageIndex, request.Params.PageSize, filter);

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

            // Wait, in MongoDB we can sort on listItems in memory if needed, matching V1 sorting:
            if (request.Params.OrderBy == "AttemptCount")
            {
                listItems = listItems.OrderByDescending(q => q.AttemptCount).ToList();
            }
            else
            {
                // Default sorting is CreatedAt descending (handled here or preserved)
            }

            return new PaginatedDto<QuizListItemDto>
            {
                Items = listItems,
                TotalItems = pagingResult.TotalItems,
                PageIndex = request.Params.PageIndex,
                PageSize = request.Params.PageSize
            };
        }
    }
}

