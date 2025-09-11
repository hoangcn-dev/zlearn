using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Common.Events
{
    public class BaseEventHandler
    {
        protected readonly IMapper _mapper;
        protected readonly IMediator _mediator;
        protected readonly ILogger _logger;

        public BaseEventHandler(IMapper mapper, IMediator mediator, ILogger logger)
        {
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }
    }
}
