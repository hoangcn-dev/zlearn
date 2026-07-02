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
            Expression<Func<QuizDocument, bool>> filter = q => true;

            if (!string.IsNullOrEmpty(request.Name))
            {
                var name = request.Name;
                filter = filter.And(q => q.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(request.CategorySlug))
            {
                var slug = request.CategorySlug;
                filter = filter.And(q => q.CategorySlug == slug);
            }

            if (!string.IsNullOrEmpty(request.ExcludeId))
            {
                var excludeId = request.ExcludeId.ToUpper();
                filter = filter.And(q => q.Id != excludeId);
            }

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

    public static class ExpressionHelper
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);
            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue) return _newValue;
                return base.Visit(node);
            }
        }
    }
}
