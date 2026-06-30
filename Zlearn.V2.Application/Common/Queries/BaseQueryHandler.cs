using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Interfaces;

namespace Zlearn.V2.Application.Common.Queries
{
    public abstract class BaseQueryHandler
    {
        protected readonly IMapper _mapper;
        protected readonly IMediator _mediator;

        protected BaseQueryHandler(IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }
    }

    public abstract class BaseQueryHandler<TDocument> : BaseQueryHandler where TDocument : class
    {
        protected readonly IReadRepo<TDocument> _readRepo;

        protected BaseQueryHandler(IReadRepo<TDocument> readRepo, IMapper mapper, IMediator mediator)
            : base(mapper, mediator)
        {
            _readRepo = readRepo;
        }
    }
}
