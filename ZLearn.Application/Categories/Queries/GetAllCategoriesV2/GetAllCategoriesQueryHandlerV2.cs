using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Queries;

namespace ZLearn.Application.Categories.Queries.GetAllCategoriesV2
{
    public class GetAllCategoriesQueryHandlerV2 : BaseQueryHandler, IRequestHandler<GetAllCategoriesQueryV2, IEnumerable<CategoryDocument>>
    {
        private readonly IReadRepo<CategoryDocument> _readRepo;

        public GetAllCategoriesQueryHandlerV2(
            IMapper mapper, 
            IMediator mediator,
            IReadRepo<CategoryDocument> readRepo) : base(mapper, mediator)
        {
            _readRepo = readRepo;
        }

        public async Task<IEnumerable<CategoryDocument>> Handle(GetAllCategoriesQueryV2 request, CancellationToken cancellationToken)
        {
            return await _readRepo.GetAllAsync(c => true);
        }
    }
}
