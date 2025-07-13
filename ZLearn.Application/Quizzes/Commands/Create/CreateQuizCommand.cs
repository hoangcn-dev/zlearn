using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Commands.Create
{
    public class CreateQuizCommand : IRequest<CreateResponseDto>
    {
        public CreateQuizDto Data { get; set; }
    }
}
