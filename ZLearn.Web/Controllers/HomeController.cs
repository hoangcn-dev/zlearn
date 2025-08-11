using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZLearn.Application.Categories.Queries.GetAllCateSlugs;
using ZLearn.Application.Quizzes.Queries.GetAllQuestionSlugs;
using ZLearn.Application.Quizzes.Queries.GetAllQuizSlugs;
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

        [Route("/sitemap.xml")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> Sitemap()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var baseSlugs = new List<(string Slug, double Priority)>
            { 
                (Slug: baseUrl, Priority: 1), 
                (Slug: $"{baseUrl}/danh-muc-trac-nghiem", Priority: 1), 
            };
            var cateSlugs = (await _mediator.Send(new GetAllCateSlugsQuery()))
                .Select(s => (Slug: $"{baseUrl}/danh-muc-trac-nghiem/{s}", Priority: 0.8));
            var quizSlugs = (await _mediator.Send(new GetAllQuizSlugsQuery()))
                .Select(s => (Slug: $"{baseUrl}/de-trac-nghiem/{s}", Priority: 0.8));
            var questionSlugs = (await _mediator.Send(new GetAllQuestionSlugsQuery()))
                .Select(s => (Slug: $"{baseUrl}/cau-hoi-trac-nghiem/{s}", Priority: 0.8));
            var urls = baseSlugs
                .Concat(cateSlugs)
                .Concat(quizSlugs)
                .Concat(questionSlugs);


            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urlset = new XElement(ns + "urlset",
                urls.Select(item =>
                    new XElement(ns + "url",
                        new XElement(ns + "loc", $"{baseUrl}/{item.Slug}"),
                        new XElement(ns + "lastmod", DateTime.UtcNow.ToString("yyyy-MM-dd")),
                        new XElement(ns + "changefreq", "weekly"),
                        new XElement(ns + "priority", item.Priority.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture))
                    )
                )
            );
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
            return Content(xml.ToString(), "application/xml");
        }
    }
}
