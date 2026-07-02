using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Categories.DTOs;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Common.Queries
{
    public class SitemapItemDto
    {
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class GetAllCateSlugsQuery : IRequest<IEnumerable<string>>
    {
    }

    public class GetAllQuizSlugsQuery : IRequest<IEnumerable<string>>
    {
    }

    public class GetAllQuestionSlugsQuery : IRequest<IEnumerable<string>>
    {
    }

    public class GetQuizzesForSitemapQuery : IRequest<List<SitemapItemDto>>
    {
    }

    public class GetQuestionsForSitemapQuery : IRequest<List<SitemapItemDto>>
    {
    }

    // HANDLERS
    public class SitemapQueriesHandler : 
        IRequestHandler<GetAllCateSlugsQuery, IEnumerable<string>>,
        IRequestHandler<GetAllQuizSlugsQuery, IEnumerable<string>>,
        IRequestHandler<GetAllQuestionSlugsQuery, IEnumerable<string>>,
        IRequestHandler<GetQuizzesForSitemapQuery, List<SitemapItemDto>>,
        IRequestHandler<GetQuestionsForSitemapQuery, List<SitemapItemDto>>
    {
        private readonly IReadRepo<CategoryDocument> _categoryReadRepo;
        private readonly IReadRepo<QuizDocument> _quizReadRepo;

        public SitemapQueriesHandler(
            IReadRepo<CategoryDocument> categoryReadRepo,
            IReadRepo<QuizDocument> quizReadRepo)
        {
            _categoryReadRepo = categoryReadRepo;
            _quizReadRepo = quizReadRepo;
        }

        public async Task<IEnumerable<string>> Handle(GetAllCateSlugsQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryReadRepo.GetAllAsync();
            return categories.Select(c => c.Slug);
        }

        public async Task<IEnumerable<string>> Handle(GetAllQuizSlugsQuery request, CancellationToken cancellationToken)
        {
            var quizzes = await _quizReadRepo.GetAllAsync(q => q.IsPublic);
            return quizzes.Select(q => q.Slug);
        }

        public async Task<IEnumerable<string>> Handle(GetAllQuestionSlugsQuery request, CancellationToken cancellationToken)
        {
            var quizzes = await _quizReadRepo.GetAllAsync(q => q.IsPublic);
            return quizzes.SelectMany(q => q.Questions.Select(qt => qt.Slug));
        }

        public async Task<List<SitemapItemDto>> Handle(GetQuizzesForSitemapQuery request, CancellationToken cancellationToken)
        {
            var quizzes = await _quizReadRepo.GetAllAsync(q => q.IsPublic);
            return quizzes.Select(q => new SitemapItemDto
            {
                Slug = q.Slug,
                CreatedAt = q.CreatedAt?.UtcDateTime ?? DateTime.UtcNow,
                UpdatedAt = q.LastModifiedAt?.UtcDateTime
            }).ToList();
        }

        public async Task<List<SitemapItemDto>> Handle(GetQuestionsForSitemapQuery request, CancellationToken cancellationToken)
        {
            var quizzes = await _quizReadRepo.GetAllAsync(q => q.IsPublic);
            var items = new List<SitemapItemDto>();
            foreach (var q in quizzes)
            {
                var quizCreatedAt = q.CreatedAt?.UtcDateTime ?? DateTime.UtcNow;
                var quizUpdatedAt = q.LastModifiedAt?.UtcDateTime;
                foreach (var qt in q.Questions)
                {
                    items.Add(new SitemapItemDto
                    {
                        Slug = qt.Slug,
                        CreatedAt = quizCreatedAt,
                        UpdatedAt = quizUpdatedAt
                    });
                }
            }
            return items;
        }
    }
}
