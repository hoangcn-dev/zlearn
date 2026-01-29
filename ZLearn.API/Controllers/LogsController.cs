using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Logs.DTOs;
using ZLearn.Application.Logs.Queries;

namespace ZLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public LogsController(IConfiguration configuration, IMediator mediator)
        {
            _configuration = configuration;
            _mediator = mediator;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetLogs([FromQuery] string? date)
        {
            var query = new GetLogQuery
            {
                Date = date is not null ? DateTime.Parse(date) : DateTime.Now
            };
            var logs = await _mediator.Send(query);
            return Ok(Result<List<LogListItemDto>>.Success("Get all logs successfully", logs));
        }
    }
}
