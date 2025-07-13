using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Queries.GetUpdateQuizContent
{
    public class GetUpdateQuizContentQuery : IRequest<UpdateQuizDto>
    {
        public string Id { get; set; }
    }
}
