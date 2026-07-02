using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Queries;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetAllTags
{
    public class GetAllTagsQueryHandler : BaseQueryHandler<QuizDocument>, IRequestHandler<GetAllTagsQuery, IEnumerable<string>>
    {
        public GetAllTagsQueryHandler(
            IReadRepo<QuizDocument> readRepo,
            IMapper mapper,
            IMediator mediator) : base(readRepo, mapper, mediator)
        {
        }

        public async Task<IEnumerable<string>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
        {
            var quizzes = await _readRepo.GetAllAsync();
            return quizzes
                .SelectMany(q => q.Tags ?? new List<string>())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}

