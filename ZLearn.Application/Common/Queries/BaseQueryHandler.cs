namespace ZLearn.Application.Common.Queries
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
}
