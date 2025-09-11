using System.Security.Claims;
using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Commands.ManageParticipant
{
    public class ManageParticipantCommand : IRequest<string>
    {
        public ManageParticipantDto Data { get; set; }
        public string ExamId { get; set; }
        public ClaimsPrincipal UserClaims { get; set; }
    }
}
