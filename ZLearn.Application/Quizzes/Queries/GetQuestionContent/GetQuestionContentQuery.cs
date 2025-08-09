using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Queries.GetQuestionContent
{
    public class GetQuestionContentQuery : IRequest<QuestionContentDto>
    {
        public string? Id { get; set; }
        public string? Slug { get; set; }
        public int? Order { get; set; }
        public string? QuizId { get; set; }
    }
}
