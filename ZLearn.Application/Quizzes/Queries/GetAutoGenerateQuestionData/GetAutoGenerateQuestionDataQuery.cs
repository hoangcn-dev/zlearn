using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Queries.GetAutoGenerateQuestionData
{
    public class GetAutoGenerateQuestionDataQuery : IRequest<List<CreateQuestionDto>>
    {
        public AutoGenerateQuestionRequestDto Data { get; set; }
    }
}
