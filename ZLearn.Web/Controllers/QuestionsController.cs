using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Quizzes.Queries.GetQuestionAnswerKey;
using ZLearn.Application.Quizzes.Queries.GetQuestionContent;

namespace ZLearn.Web.Controllers
{
    [Route("[controller]")]
    public class QuestionsController : BaseController
    {
        public QuestionsController(
            ILogger<QuestionsController> logger, 
            IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            var query = new GetQuestionContentQuery
            {
                Id = id
            };
            var content = await _mediator.Send(query);
            return View(content);
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetQuestionData(string id)
        {
            var query = new GetQuestionContentQuery
            {
                Id = id
            };
            var content = await _mediator.Send(query);
            return Json(content);
        }

        [HttpGet("correct-key")]
        public async Task<IActionResult> GetCorrectKey(string id)
        {
            var query = new GetQuestionAnswerKeyQuery
            {
                QuestionId = id
            };
            var key = await _mediator.Send(query);
            return Json(key);
        }
    }
}
