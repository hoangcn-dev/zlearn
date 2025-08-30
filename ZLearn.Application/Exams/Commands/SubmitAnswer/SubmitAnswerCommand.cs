using System.Security.Claims;
using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Commands.SubmitAnswer
{
    public class SubmitAnswerCommand : IRequest<string>
    {
        public string ParticipantId { get; set; }
        public SubmitExamDto Data { get; set; }
    }
}
