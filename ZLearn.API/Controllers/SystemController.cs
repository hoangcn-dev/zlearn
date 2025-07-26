using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Tracking.Queries.GetAccessCount;

namespace ZLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SystemController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("access-stat")]
        public async Task<IActionResult> GetAccessCount()
        {
            var result = await _mediator.Send(new GetAccessCountQuery());
            return Ok(result);
        }
    }
}
