using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Commands.CreateExam
{
    public class CreateExamCommand : IRequest<CreateResponseDto>
    {
        public CreateExamDto Data { get; set; }
        public string UserId { get; set; }
    }
}
