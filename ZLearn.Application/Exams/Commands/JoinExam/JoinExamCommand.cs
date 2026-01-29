using System.Security.Claims;
using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Commands.JoinExam
{
    public class JoinExamCommand : IRequest<ParticipantWaitingInfoDto>
    {
        public ClaimsPrincipal User { get; set; }
        public JoinExamRequestDto Data { get; set; }
    }
}
