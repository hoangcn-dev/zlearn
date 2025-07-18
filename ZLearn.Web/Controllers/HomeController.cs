using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ZLearn.Application.Categories.Queries.GetAllCates;
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
            var categories = await _mediator.Send(new GetAllCatesQuery());
            return View(categories);
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
