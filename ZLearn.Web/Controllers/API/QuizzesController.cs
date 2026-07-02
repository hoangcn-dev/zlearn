using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Categories.Commands.CreateCategory;
using Zlearn.V2.Application.Categories.Queries.GetCategoryById;
using Zlearn.V2.Application.Categories.Queries.GetAllCategories;
using Zlearn.V2.Application.Categories.DTOs;
using UpdateCategoryCommand = Zlearn.V2.Application.Categories.Commands.UpdateCategory.UpdateCategoryCommand;
using DeleteCategoryCommand = Zlearn.V2.Application.Categories.Commands.DeleteCategory.DeleteCategoryCommand;
using CategoryDocument = Zlearn.V2.Application.Categories.DTOs.CategoryDocument;
using CreateResponseDto = Zlearn.V2.Application.Common.DTOs.CreateResponseDto;
using GetListQuizQuery = Zlearn.V2.Application.Quizzes.Queries.GetListQuiz.GetListQuizQuery;
using GetMyQuizQuery = Zlearn.V2.Application.Quizzes.Queries.GetMyQuiz.GetMyQuizQuery;
using GetUpdateQuizContentQuery = Zlearn.V2.Application.Quizzes.Queries.GetUpdateQuizContent.GetUpdateQuizContentQuery;
using GetQuizDetailQuery = Zlearn.V2.Application.Quizzes.Queries.GetQuizDetail.GetQuizDetailQuery;
using CreateQuizCommand = Zlearn.V2.Application.Quizzes.Commands.Create.CreateQuizCommand;
using UpdateQuizCommand = Zlearn.V2.Application.Quizzes.Commands.Update.UpdateQuizCommand;
using DeleteQuizCommand = Zlearn.V2.Application.Quizzes.Commands.Delete.DeleteQuizCommand;
using GetAllTagsQuery = Zlearn.V2.Application.Quizzes.Queries.GetAllTags.GetAllTagsQuery;
using UpdateQuizDto = Zlearn.V2.Application.Quizzes.DTOs.UpdateQuizDto;
using QuizDetailDto = Zlearn.V2.Application.Quizzes.DTOs.QuizDetailDto;
using QuizListItemDto = Zlearn.V2.Application.Quizzes.DTOs.QuizListItemDto;
using QuizSearchDto = Zlearn.V2.Application.Quizzes.DTOs.QuizSearchDto;
using UpdateResponseDto = Zlearn.V2.Application.Common.DTOs.UpdateResponseDto;
using Zlearn.V2.Application.Quizzes.Queries.GetQuestionContent;
using QuestionContentDto = Zlearn.V2.Application.Quizzes.DTOs.QuestionContentDto;
using GetQuestionAnswerKeyQuery = Zlearn.V2.Application.Quizzes.Queries.GetQuestionAnswerKey.GetQuestionAnswerKeyQuery;
using CorrectAnswerKeyDto = Zlearn.V2.Application.Quizzes.DTOs.CorrectAnswerKeyDto;


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

        [HttpGet("my-quiz")]
        [Authorize]
        public async Task<IActionResult> GetMyQuizzes([FromQuery] QuizSearchDto data)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = new GetMyQuizQuery
            {
                OwnerId = userId,
                Params = data
            };
            var res = await _mediator.Send(query);
            return Ok(Result<PaginatedDto<QuizListItemDto>>.Success(null, res));
        }

        [HttpPost("generate-questions")]
        public async Task<IActionResult> GenerateQuestions([FromForm] object data)
        {
            throw new System.NotImplementedException("AI question generation is not supported in V2");
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUpdateData(string id)
        {
            var query = new GetUpdateQuizContentQuery
            {
                Id = id,
                OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
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
        [Authorize]
        public async Task<IActionResult> CreateNewQuiz([FromBody] CreateQuizCommand command)
        {
            var res = await _mediator.Send(command);
            return Ok(Result<CreateResponseDto>.Success("Create new quiz successfully.", res));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateQuizDto data)
        {
            data.Id = id;
            var res = await _mediator.Send(new UpdateQuizCommand
            {
                Data = data,
                OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            return Ok(Result<UpdateResponseDto>.Success("Update quiz successfully.", res));
        }

        [HttpPost("delete")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> DeleteQuizzes([FromBody] DeleteRequestDto data)
        {
            var command = new DeleteQuizCommand
            {
                Ids = data.Ids,
                OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            var res = await _mediator.Send(command);
            return Ok(Result<DeleteResponseDto>.Success("Delete quizz successfully.", res));
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetAllTags()
        {
            var query = new GetAllTagsQuery();
            var res = await _mediator.Send(query);
            return Ok(Result<IEnumerable<string>>.Success("Get all tags successfully.", res));
        }

        [HttpPost("export")]
        [Authorize]
        public async Task<IActionResult> ExportQuizzes([FromBody] Zlearn.V2.Application.Quizzes.Queries.ExportQuizzes.ExportQuizzesQuery query)
        {
            var res = await _mediator.Send(query);
            return File(res.Content, res.ContentType, res.FileName);
        }

        [HttpPost("scan")]
        [Authorize]
        public async Task<IActionResult> ScanQuiz([FromForm] object command)
        {
            throw new System.NotImplementedException("Quiz scanning is not supported in V2");
        }

        #region Question
        [HttpGet("questions")]
        [Authorize]
        public async Task<IActionResult> GetQuestionBank([FromQuery] object query)
        {
            throw new System.NotImplementedException("Question bank is not supported in V2");
        }

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
        public async Task<IActionResult> CreateNewCate([FromBody] CreateCategoryCommand command)
        {
            var res = await _mediator.Send(command);
            return Ok(Result<CreateResponseDto>.Success("Create new category successfully.", res));
        }


        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCate()
        {
            var res = await _mediator.Send(new GetAllCategoriesQuery());
            _logger.LogInformation("View all categories V2.");
            return Ok(Result<IEnumerable<CateListItemDto>>.Success("Get all categories successfully.", res));
        }


        [HttpGet("categories/{id}")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> GetCateDetail(string id)
        {
            var res = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
            return Ok(Result<CateDetailDto>.Success("Get category detail information successfully.", res));
        }


        [HttpPut("categories/{id}")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> UpdateCate(string id, [FromBody] UpdateCateDto data)
        {
            var res = await _mediator.Send(new UpdateCategoryCommand(
                id,
                data.Name,
                data.Description,
                data.ThumbnailUrl
            ));
            return Ok(Result<CreateResponseDto>.Success("Update category successfully.", res));
        }


        [HttpPost("categories/delete")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> DeleteCate([FromBody] DeleteRequestDto data)
        {
            var command = new DeleteCategoryCommand(data.Ids);
            var res = await _mediator.Send(command);
            return Ok(Result<bool>.Success("Delete category successfully.", res));
        } 
        #endregion
    }
}
