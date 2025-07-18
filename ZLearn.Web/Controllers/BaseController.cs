using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ZLearn.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly IMediator _mediator;

        protected BaseController(ILogger logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
    }
}
