using AutoMapper;
using MediatR;

namespace Zlearn.V2.Application.Common.Commands
{
    public abstract class BaseCommandHandler
    {
        protected readonly IMapper _mapper;
        protected readonly IMediator _mediator;

        protected BaseCommandHandler(IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }
    }
}
