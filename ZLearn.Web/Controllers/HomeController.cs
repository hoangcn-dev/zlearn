using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ZLearn.Web.Models;

namespace ZLearn.Web.Controllers
{
    [Route("/")]
    public class HomeController : BaseController
    {
        public HomeController(
            ILogger<HomeController> logger, 
            IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", "QuizCate");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("forbidden")]
        public IActionResult Forbidden()
        {
            return View();
        }

        [Route("unauthoried")]
        public IActionResult Unauthorized([FromQuery] string returnUrl)
        {
            return View(model: returnUrl);
        }
    }
}
