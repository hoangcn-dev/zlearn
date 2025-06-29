using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Categories.Commands.CreateCate;
using ZLearn.Application.Categories.Commands.DeleteCate;
using ZLearn.Application.Categories.Commands.UpdateCate;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Categories.Queries.GetCateDetail;
using ZLearn.Application.Categories.Queries.GetPaginatedCate;
using ZLearn.Application.Common.Model;

namespace ZLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizziesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuizziesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateNewCate([FromBody] CreateCateCommand command)
        {
            var res = await _mediator.Send(command);
            return Ok(Result<CreateResponseDto>.Success("Create new category successfully.", res));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCate([FromQuery] GetAllCatesQuery query)
        {
            var res = await _mediator.Send(query);
            return Ok(Result<IEnumerable<CateListItemDto>>.Success("Get all categories successfully.", res));
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCateDetail(string id)
        {
            var query = new GetCateDetailQuery
            {
                CateId = id
            };
            var res = await _mediator.Send(query);
            return Ok(Result<CateDetailDto>.Success("Get category detail information successfully.", res));
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCate(string id, [FromBody] UpdateCateCommand command)
        {
            command.CateId = id;
            var res = await _mediator.Send(command);
            return Ok(Result<UpdateResponseDto>.Success("Update category successfully.", res));
        }

        [HttpPost("categories/delete")]
        public async Task<IActionResult> DeleteCate([FromBody] DeleteRequestDto data)
        {
            var command = new DeleteCateCommand
            {
                CateIds = data.Ids
            };
            var res = await _mediator.Send(command);
            return Ok(Result<DeleteResponseDto>.Success("Delete category successfully.", res));
        }
    }
}
