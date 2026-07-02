using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zlearn.V2.Application.Quizzes.Queries.GetQuestionContent;

namespace ZLearn.Web.Controllers
{
    [Route("cau-hoi-trac-nghiem")]
    public class QuestionsController : BaseController
    {
        public QuestionsController(
            ILogger<QuestionsController> logger, 
            IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Index(string slug)
        {
            var query = new GetQuestionContentQuery
            {
                Slug = slug
            };
            var content = await _mediator.Send(query);
            return View(content);
        }

        //[HttpGet("data")]
        //public async Task<IActionResult> GetQuestionData(string id)
        //{
        //    var query = new GetQuestionContentQuery
        //    {
        //        Id = id
        //    };
        //    var content = await _mediator.Send(query);
        //    return Json(content);
        //}

        //[HttpGet("correct-key")]
        //public async Task<IActionResult> GetCorrectKey(string id)
        //{
        //    var query = new GetQuestionAnswerKeyQuery
        //    {
        //        QuestionId = id
        //    };
        //    var key = await _mediator.Send(query);
        //    return Json(key);
        //}
    }
}
