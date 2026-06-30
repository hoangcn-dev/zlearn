using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Domain.Common;

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

    public abstract class BaseCommandHandler<TEntity> : BaseCommandHandler where TEntity : BaseEntity
    {
        protected readonly IWriteRepo<TEntity> _writeRepo;

        protected BaseCommandHandler(IWriteRepo<TEntity> writeRepo, IMapper mapper, IMediator mediator)
            : base(mapper, mediator)
        {
            _writeRepo = writeRepo;
        }
    }
}
