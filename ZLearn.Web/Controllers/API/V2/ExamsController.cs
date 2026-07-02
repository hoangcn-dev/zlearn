using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Zlearn.V2.Application.Exams.Commands.ChangeExamStatus;
using Zlearn.V2.Application.Exams.Commands.CreateExam;
using Zlearn.V2.Application.Exams.Commands.JoinExam;
using Zlearn.V2.Application.Exams.Commands.ManageParticipant;
using Zlearn.V2.Application.Exams.Commands.SubmitAnswer;
using Zlearn.V2.Application.Exams.Queries.GetExamDetail;
using Zlearn.V2.Application.Exams.Queries.GetExamScore;
using Zlearn.V2.Application.Exams.Queries.GetExamScoreExcelFileData;
using Zlearn.V2.Application.Exams.Queries.GetOnGoingExam;
using Zlearn.V2.Application.Exams.Queries.GetParticipantStatus;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using CreateResponseDto = Zlearn.V2.Application.Common.DTOs.CreateResponseDto;

namespace ZLearn.Web.Controllers.API.V2
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class ExamsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IExamSessionService _examSessionService;

        public ExamsController(IMediator mediator, IExamSessionService examSessionService)
        {
            _mediator = mediator;
            _examSessionService = examSessionService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDto data)
        {
            var res = await _mediator.Send(new CreateExamCommand
            {
                Data = data,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            });
            return Ok(Result<CreateResponseDto>.Success("Tạo bài kiểm tra thành công (V2).", res));
        }

        [HttpGet("{id}/export-result")]
        public async Task<IActionResult> ExportResult(string id)
        {
            var res = await _mediator.Send(new GetExamScoreExcelFileDataQuery
            {
                ExamId = id,
                UserClaims = User
            });
            res.StreamData.Position = 0;
            return File(res.StreamData, res.MIMEType, res.FileName);
        }

        [HttpGet("participant-info")]
        public async Task<IActionResult> GetParticipantInfo([FromQuery] string alias)
        {
            var userId = await _examSessionService.GetUserIdAsync(HttpContext);
            var token = await _examSessionService.GetSessionTokenAsync(HttpContext);

            var res = await _mediator.Send(new GetParticipantStatusQuery
            {
                Alias = alias,
                UserId = userId
            });
            if (res == null) return Ok(Result<NoData>.Failure());

            if (!string.IsNullOrEmpty(token))
            {
                res.SessionToken = token;
            }

            return Ok(Result<ParticipantWaitingInfoDto>.Success("", res));
        }

        [HttpGet("on-going")]
        [Authorize]
        public async Task<IActionResult> GetOnGoingExam()
        {
            var res = await _mediator.Send(new GetOnGoingExamQuery
            {
                UserClaims = User
            });
            return Ok(Result<List<OnGoingExamListItemDto>>.Success("", res));
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitExamDto data)
        {
            var userId = await _examSessionService.GetUserIdAsync(HttpContext);

            await _mediator.Send(new SubmitAnswerCommand
            {
                ParticipantId = userId,
                Data = data
            });

            await _examSessionService.HandleSubmitSessionAsync(HttpContext, userId, data.ExamId);

            return Ok(Result<NoData>.Success());
        }

        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinExamRequestDto data)
        {
            var result = await _mediator.Send(new JoinExamCommand
            {
                User = User,
                Data = data
            });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null && result is not null)
            {
                await _examSessionService.HandleJoinSessionAsync(HttpContext, userId, data.ExamId, result.ParticipantId, result);
            }

            return Ok(Result<ParticipantWaitingInfoDto>.Success("", result));
        }

        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeExamStatusDto data)
        {
            await _mediator.Send(new ChangeExamStatusCommand
            {
                ExamId = id,
                Data = data,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            });
            return Ok(Result<NoData>.Success());
        }

        [HttpPost("{id}/manage-participant")]
        [Authorize]
        public async Task<IActionResult> ManageParticipant(string id, [FromBody] ManageParticipantDto data)
        {
            var participantId = await _mediator.Send(new ManageParticipantCommand
            {
                Data = data,
                ExamId = id,
                UserClaims = User
            });
            return Ok(Result<object>.Success("", new { participantId }));
        }
    }
}
