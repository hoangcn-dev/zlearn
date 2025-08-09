using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Quizzes.Commands.Delete
{
    public class DeleteQuizCommand : IRequest<DeleteResponseDto>
    {
        public List<string> Ids { get; set; }
        public string OwnerId { get; set; }
    }
}
