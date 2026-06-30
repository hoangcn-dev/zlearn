namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class CreateAnswerDto
    {
        public int Key { get; set; }
        public string? StringContent { get; set; }
        public List<string> MediaFileUrls { get; set; } = new();
        public bool IsCorrect { get; set; }
    }
}
