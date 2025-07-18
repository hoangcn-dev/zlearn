using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Queries.GetQuizDetail
{
    public class GetQuizDetailQuery : IRequest<QuizDetailDto>
    {
        public string Id { get; set; }
    }
}
