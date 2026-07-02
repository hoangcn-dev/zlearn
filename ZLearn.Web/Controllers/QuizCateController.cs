using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zlearn.V2.Application.Categories.Queries.GetAllCategories;
using Zlearn.V2.Application.Categories.Queries.GetCategoryById;
using Zlearn.V2.Application.Quizzes.Queries.GetListQuiz;

namespace ZLearn.Web.Controllers
{
    [Route("danh-muc-trac-nghiem")]
    public class QuizCateController : BaseController
    {
        public QuizCateController(ILogger<QuizCateController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _mediator.Send(new GetAllCategoriesQuery());
            return View(categories);
        }

        [HttpGet("{categorySlug}")]
        public async Task<IActionResult> Detail(string categorySlug)
        {
            var quizzes = await _mediator.Send(new GetListQuizQuery
            {
                CategorySlug = categorySlug,
                Name = null,
                PageIndex = 1,
                PageSize = 100
            });
            var cateDetail = await _mediator.Send(new GetCategoryByIdQuery
            {
                Slug = categorySlug
            });
            return View((cateDetail, quizzes));
        }
    }
}
