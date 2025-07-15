using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Categories.Commands.CreateCate;
using ZLearn.Application.Categories.Commands.DeleteCate;
using ZLearn.Application.Categories.Commands.UpdateCate;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Categories.Queries.GetCateById;
using ZLearn.Application.Categories.Queries.GetPaginatedCate;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Quizzes.Commands.Create;
using ZLearn.Application.Quizzes.Commands.Delete;
using ZLearn.Application.Quizzes.Commands.Update;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Application.Quizzes.Queries.GetAllTags;
using ZLearn.Application.Quizzes.Queries.GetListQuiz;
using ZLearn.Application.Quizzes.Queries.GetUpdateQuizContent;

namespace ZLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuizzesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuizzes([FromQuery] GetListQuizQuery query)
        {
            var res = await _mediator.Send(query);
            return Ok(Result<PaginatedDto<QuizListItemDto>>.Success("Get all quizzes successfully.", res));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUpdateData(string id)
        {
            var query = new GetUpdateQuizContentQuery
            {
                Id = id
            };
            var res = await _mediator.Send(query);
            return Ok(Result<UpdateQuizDto>.Success("Get quiz content successfully.", res));
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewQuiz([FromBody] CreateQuizCommand command)
        {
            var res = await _mediator.Send(command);
            return Ok(Result<CreateResponseDto>.Success("Create new quiz successfully.", res));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateQuizCommand command)
        {
            command.Data.Id = id;
            var res = await _mediator.Send(command);
            return Ok(Result<UpdateResponseDto>.Success("Update quiz successfully.", res));
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteQuizzes([FromBody] DeleteRequestDto data)
        {
            var command = new DeleteQuizCommand
            {
                Ids = data.Ids
            };
            var res = await _mediator.Send(command);
            return Ok(Result<DeleteResponseDto>.Success("Delete quizzes successfully.", res));
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetAllTags()
        {
            var query = new GetAllTagsQuery();
            var res = await _mediator.Send(query);
            return Ok(Result<IEnumerable<string>>.Success("Get all tags successfully.", res));
        }

        #region Cate
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
            var query = new GetCateByIdQuery
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
        #endregion
    }
}
