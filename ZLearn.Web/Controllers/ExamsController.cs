using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZLearn.Application.Exams.Queries.GetAllExams;
using ZLearn.Application.Exams.Queries.GetExamContent;
using ZLearn.Application.Exams.Queries.GetExamDetail;
using ZLearn.Application.Exams.Queries.GetParticipantResult;
using ZLearn.Application.Exams.Queries.GetParticipantStatus;
using ZLearn.Application.Exams.Queries.GetWaitExamInfo;
using ZLearn.Domain.Enums;

namespace ZLearn.Web.Controllers
{
    [Route("bai-kiem-tra")]
    public class ExamsController : BaseController
    {
        private readonly IConfiguration _configuration;

        public ExamsController(
            ILogger<ExamsController> logger,
            IMediator mediator,
            IConfiguration configuration) : base(logger, mediator)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var exams = await _mediator.Send(new GetAllExamsQuery());
            return View(exams);
        }


        [HttpGet("tao-moi")]
        public IActionResult Create()
        {
            return View();
        }


        [HttpGet("content")]
        [Authorize]
        public async Task<IActionResult> Content([FromQuery] string alias)
        {
            var exam = await _mediator.Send(new GetExamContentQuery 
            { 
                Alias = alias,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            return View(exam);
        }


        [HttpGet("result")]
        public async Task<IActionResult> Result([FromQuery] string alias)
        {
            var query = new GetParticipantResultQuery
            {
                Alias = alias,
                ParticipantId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            var result = await _mediator.Send(query);
            return View(result);
        }


        [HttpGet("detail-answer")]
        public async Task<IActionResult> DetailAnswer([FromQuery] string alias)
        {
            var query = new GetParticipantResultQuery
            {
                Alias = alias,
                ParticipantId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            var result = await _mediator.Send(query);
            return View(result);
        }


        [HttpGet("wait")]
        [Authorize]
        public async Task<IActionResult> Wait([FromQuery] string alias)
        {
            var waitData = await _mediator.Send(new GetWaitExamInfoQuery 
            { 
                Alias = alias,
                ParticipantId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            return View(waitData);
        }


        [HttpGet("join")]
        [Authorize]
        public async Task<IActionResult> Join([FromQuery] string alias)
        {
            var status = await _mediator.Send(new GetParticipantStatusQuery
            {
                Alias = alias,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            if (status is null || status.Status == ParticipantStatus.WaitingForExamStart)
            {
                return RedirectToAction("Wait", new { alias });
            }
            else if (status.Status == ParticipantStatus.Completed)
            {
                return RedirectToAction("Result", new { alias });
            }
            else
            {
                return RedirectToAction("Content", new { alias });
            }
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Detail(string id)
        {
            var exam = await _mediator.Send(new GetExamDetailQuery
            {
                Id = id,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            return View(exam);
        }
    }
}
