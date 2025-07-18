using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ZLearn.Application.Quizzes.Queries.GetListQuiz;
using ZLearn.Application.Quizzes.Queries.GetQuizDetail;

namespace ZLearn.Web.Controllers
{
    [Route("[controller]")]
    public class QuizzesController : BaseController
    {
        public QuizzesController(
            ILogger<QuizzesController> logger,
            IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string category)
        {
            var quizzes = await _mediator.Send(new GetListQuizQuery
            {
                CategoryId = category,
                Name = null,
                PageIndex = 1,
                PageSize = 100
            });
            return View(quizzes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var query = new GetQuizDetailQuery
            {
                Id = id
            };
            var quiz = await _mediator.Send(query);
            return RedirectToAction(
                controllerName: "Questions", 
                actionName: "Index", 
                routeValues: new
                {
                    id = quiz.Questions[0].Id
                });
        }

        [HttpGet("question-map")]
        public async Task<IActionResult> GetQuestionMapAsync(string id)
        {
            var query = new GetQuizDetailQuery
            {
                Id = id
            };
            var quiz = await _mediator.Send(query);
            return Json(quiz);
        }
    }
}
