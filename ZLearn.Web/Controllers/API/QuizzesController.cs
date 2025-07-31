using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Categories.Commands.CreateCate;
using ZLearn.Application.Categories.Commands.DeleteCate;
using ZLearn.Application.Categories.Commands.UpdateCate;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Categories.Queries.GetAllCates;
using ZLearn.Application.Categories.Queries.GetCateById;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Quizzes.Commands.Create;
using ZLearn.Application.Quizzes.Commands.Delete;
using ZLearn.Application.Quizzes.Commands.Update;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Application.Quizzes.Queries.GetAllTags;
using ZLearn.Application.Quizzes.Queries.GetListQuiz;
using ZLearn.Application.Quizzes.Queries.GetQuestionAnswerKey;
using ZLearn.Application.Quizzes.Queries.GetQuestionContent;
using ZLearn.Application.Quizzes.Queries.GetQuizDetail;
using ZLearn.Application.Quizzes.Queries.GetUpdateQuizContent;

namespace ZLearn.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly ILogger<QuizzesController> _logger;
        private readonly IMediator _mediator;

        public QuizzesController(IMediator mediator, ILogger<QuizzesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuizzes([FromQuery] GetListQuizQuery query)
        {
            var res = await _mediator.Send(query);
            return Ok(Result<PaginatedDto<QuizListItemDto>>.Success("Get all quizzes successfully.", res));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> GetUpdateData(string id)
        {
            var query = new GetUpdateQuizContentQuery
            {
                Id = id
            };
            var res = await _mediator.Send(query);
            return Ok(Result<UpdateQuizDto>.Success("Get quiz content successfully.", res));
        }

        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetQuestionMapAsync(string id)
        {
            var query = new GetQuizDetailQuery
            {
                Id = id
            };
            var quiz = await _mediator.Send(query);
            return Ok(Result<QuizDetailDto>.Success("Get quiz detail successfully.", quiz));
        }

        [HttpPost]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> CreateNewQuiz([FromBody] CreateQuizCommand command)
        {
            var res = await _mediator.Send(command);
            return Ok(Result<CreateResponseDto>.Success("Create new quiz successfully.", res));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateQuizCommand command)
        {
            command.Data.Id = id;
            var res = await _mediator.Send(command);
            return Ok(Result<UpdateResponseDto>.Success("Update quiz successfully.", res));
        }

        [HttpPost("delete")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> DeleteQuizzes([FromBody] DeleteRequestDto data)
        {
            var command = new DeleteQuizCommand
            {
                Ids = data.Ids
            };
            var res = await _mediator.Send(command);
            return Ok(Result<DeleteResponseDto>.Success("Delete quizz successfully.", res));
        }

        [HttpGet("tags")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> GetAllTags()
        {
            var query = new GetAllTagsQuery();
            var res = await _mediator.Send(query);
            return Ok(Result<IEnumerable<string>>.Success("Get all tags successfully.", res));
        }

        #region Question
        [HttpGet("questions/{id}")]
        public async Task<IActionResult> GetQuestionData(string id)
        {
            var query = new GetQuestionContentQuery
            {
                Id = id
            };
            var content = await _mediator.Send(query);
            return Ok(Result<QuestionContentDto>.Success("Get question content successfully", content));
        }

        [HttpGet("questions/{id}/correct-key")]
        public async Task<IActionResult> GetQuestionCorrectKey(string id)
        {
            var query = new GetQuestionAnswerKeyQuery
            {
                QuestionId = id
            };
            var key = await _mediator.Send(query);
            return Ok(Result<CorrectAnswerKeyDto>.Success("Get question key successfully", key));
        }
        #endregion

        #region Category
        [HttpPost("categories")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> CreateNewCate([FromBody] CreateCateCommand command)
        {
            var res = await _mediator.Send(command);
            return Ok(Result<CreateResponseDto>.Success("Create new category successfully.", res));
        }


        [HttpGet("categories")]
        //[Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> GetAllCate([FromQuery] GetAllCatesQuery query)
        {
            var res = await _mediator.Send(query);
            _logger.LogInformation("View all categories.");
            return Ok(Result<IEnumerable<CateListItemDto>>.Success("Get all categories successfully.", res));
        }


        [HttpGet("categories/{id}")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> GetCateDetail(string id)
        {
            var user = Request.HttpContext.User;
            var query = new GetCateByIdQuery
            {
                CateId = id
            };
            var res = await _mediator.Send(query);
            return Ok(Result<CateDetailDto>.Success("Get category detail information successfully.", res));
        }


        [HttpPut("categories/{id}")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> UpdateCate(string id, [FromBody] UpdateCateCommand command)
        {
            command.CateId = id;
            var res = await _mediator.Send(command);
            return Ok(Result<UpdateResponseDto>.Success("Update category successfully.", res));
        }


        [HttpPost("categories/delete")]
        [Authorize(Policy = "OnlyAdmin")]
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
