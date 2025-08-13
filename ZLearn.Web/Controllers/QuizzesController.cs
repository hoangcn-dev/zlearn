using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Quizzes.Queries.GetQuizDetail;

namespace ZLearn.Web.Controllers
{
    [Route("de-trac-nghiem")]
    public class QuizzesController : BaseController
    {
        public QuizzesController(
            ILogger<QuizzesController> logger,
            IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet("tao-moi")]
        public IActionResult Create()
        {
            return View();
        }


        [HttpGet("de-da-tao")]
        public async Task<IActionResult> MyQuiz()
        {
            return View();
        }


        [HttpGet("cap-nhat-de")]
        public async Task<IActionResult> Update(string id)
        {
            return View();
        }


        [HttpGet("{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            var query = new GetQuizDetailQuery
            {
                Slug = slug
            };
            var quiz = await _mediator.Send(query);
            return RedirectToAction(
                controllerName: "Questions",
                actionName: "Index",
                routeValues: new
                {
                    slug = quiz.Questions[0].Slug
                });
        }
    }
}
