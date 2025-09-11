
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Exams.Commands.ChangeExamStatus;
using ZLearn.Application.Exams.Commands.CreateExam;
using ZLearn.Application.Exams.Commands.JoinExam;
using ZLearn.Application.Exams.Commands.ManageParticipant;
using ZLearn.Application.Exams.Commands.SubmitAnswer;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Application.Exams.Queries.GetParticipantStatus;
using ZLearn.Domain.Enums;

namespace ZLearn.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : BaseController
    {
        public ExamsController(
            ILogger<ExamsController> logger, 
            IMediator mediator) : base(logger, mediator)
        {
        }

        [HttpGet("participant-info")]
        public async Task<IActionResult> GetParticipantInfo([FromQuery] string alias)
        {
            var res = await _mediator.Send(new GetParticipantStatusQuery
            {
                Alias = alias,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            if (res == null) return Ok(Result<NoData>.Failure());
            return Ok(Result<ParticipantWaitingInfoDto>.Success("", res));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateExam([FromBody]CreateExamDto data)
        {
            var res = await _mediator.Send(new CreateExamCommand
            {
                Data = data,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            });
            return Ok(Result<CreateResponseDto>.Success("Tạo bài kiểm tra thành công.", res));
        }

        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> Submit([FromBody] SubmitExamDto data)
        {
            await _mediator.Send(new SubmitAnswerCommand
            {
                ParticipantId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Data = data
            });
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
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
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
