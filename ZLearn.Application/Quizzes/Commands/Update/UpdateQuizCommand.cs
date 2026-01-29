using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Commands.Update
{
    public class UpdateQuizCommand : IRequest<UpdateResponseDto>
    {
        public UpdateQuizDto Data { get; set; }
        public string OwnerId { get; set; }
    }
}
