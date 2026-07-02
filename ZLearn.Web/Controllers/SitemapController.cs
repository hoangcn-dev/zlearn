using System.Text;
using System.Xml.Linq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zlearn.V2.Application.Common.Queries;

namespace ZLearn.Web.Controllers
{
    [Route("sitemap.xml")]
    public class SitemapController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SitemapController> _logger;

        public SitemapController(IMediator mediator, ILogger<SitemapController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [ResponseCache(Duration = 3600)] // Cache for 1 hour
        public async Task<IActionResult> Index()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                
                // Create a sitemap index that points to individual sitemaps
                var sitemapIndex = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement(XName.Get("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                        CreateSitemapElement(baseUrl, "/sitemap-main.xml", DateTime.UtcNow),
                        CreateSitemapElement(baseUrl, "/sitemap-quizzes.xml", DateTime.UtcNow),
                        CreateSitemapElement(baseUrl, "/sitemap-questions.xml", DateTime.UtcNow)
                    )
                );

                return Content(sitemapIndex.ToString(), "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sitemap index");
                return StatusCode(500);
            }
        }

        [HttpGet("/sitemap-main.xml")]
        [ResponseCache(Duration = 3600)]
        public IActionResult MainSitemap()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                
                var sitemap = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement(XName.Get("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                        // Homepage
                        CreateUrlElement(baseUrl, "/", DateTime.UtcNow, "1.0", "daily"),
                        
                        // Static pages
                        CreateUrlElement(baseUrl, "/de-trac-nghiem", DateTime.UtcNow, "0.9", "daily"),
                        CreateUrlElement(baseUrl, "/de-trac-nghiem/de-da-tao", DateTime.UtcNow, "0.5", "weekly")
                    )
                );

                return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating main sitemap");
                return StatusCode(500);
            }
        }

        [HttpGet("/sitemap-quizzes.xml")]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> QuizzesSitemap()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                
                // Get all quizzes - you'll need to create this query
                var quizzes = await GetQuizzesForSitemap();
                
                var elements = new List<XElement>();
                
                foreach (var quiz in quizzes)
                {
                    elements.Add(CreateUrlElement(
                        baseUrl, 
                        $"/de-trac-nghiem/{quiz.Slug}", 
                        quiz.UpdatedAt ?? quiz.CreatedAt, 
                        "0.8", 
                        "weekly"
                    ));
                }

                var sitemap = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement(XName.Get("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                        elements
                    )
                );

                return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quizzes sitemap");
                return StatusCode(500);
            }
        }

        [HttpGet("/sitemap-questions.xml")]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> QuestionsSitemap()
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                
                // Get all questions - you'll need to create this query
                var questions = await GetQuestionsForSitemap();
                
                var elements = new List<XElement>();
                
                foreach (var question in questions)
                {
                    elements.Add(CreateUrlElement(
                        baseUrl, 
                        $"/cau-hoi-trac-nghiem/{question.Slug}", 
                        question.UpdatedAt ?? question.CreatedAt, 
                        "0.7", 
                        "monthly"
                    ));
                }

                var sitemap = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement(XName.Get("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                        elements
                    )
                );

                return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating questions sitemap");
                return StatusCode(500);
            }
        }

        private XElement CreateUrlElement(string baseUrl, string path, DateTime lastMod, string priority, string changeFreq)
        {
            return new XElement("url",
                new XElement("loc", $"{baseUrl}{path}"),
                new XElement("lastmod", lastMod.ToString("yyyy-MM-dd")),
                new XElement("changefreq", changeFreq),
                new XElement("priority", priority)
            );
        }

        private XElement CreateSitemapElement(string baseUrl, string path, DateTime lastMod)
        {
            return new XElement("sitemap",
                new XElement("loc", $"{baseUrl}{path}"),
                new XElement("lastmod", lastMod.ToString("yyyy-MM-dd"))
            );
        }

        private async Task<List<SitemapQuiz>> GetQuizzesForSitemap()
        {
            var query = new GetQuizzesForSitemapQuery();
            var quizzes = await _mediator.Send(query);
            
            return quizzes.Select(q => new SitemapQuiz
            {
                Slug = q.Slug,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            }).ToList();
        }

        private async Task<List<SitemapQuestion>> GetQuestionsForSitemap()
        {
            var query = new GetQuestionsForSitemapQuery();
            var questions = await _mediator.Send(query);
            
            return questions.Select(q => new SitemapQuestion
            {
                Slug = q.Slug,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            }).ToList();
        }
    }

    // DTOs for sitemap
    public class SitemapQuiz
    {
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SitemapQuestion
    {
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
