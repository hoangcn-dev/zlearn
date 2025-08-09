using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class QuizSearchDto : PagingRequestDto
    {
        public string? Name { get; set; }
        public string? CategoryId { get; set; }
        public string? Tag { get; set; }
    }
}
