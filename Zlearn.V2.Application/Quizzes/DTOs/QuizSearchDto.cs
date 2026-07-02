using Zlearn.V2.Application.Common.DTOs;

namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class QuizSearchDto : PagingRequestDto
    {
        public string? Name { get; set; }
        public string? CategoryId { get; set; }
        public string? Tag { get; set; }
    }
}

