using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Categories.Commands.CreateCate;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Categories.Queries.GetPaginatedCate;
using ZLearn.Application.Common.Model;

namespace ZLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewCate([FromBody] CreateCateCommand command)
        {
            var res = await _mediator.Send(command);
            return Ok(Result<CreateResponseDto>.Success("Create new category successfully.", res));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCate([FromQuery] GetAllCatesQuery query)
        {
            var res = await _mediator.Send(query);
            return Ok(Result<IEnumerable<CateListItemDto>>.Success(null, res));
        }
    }
}
