namespace ZLearn.Application.Quizzes.DTOs
{
    public class CreateAnswerDto
    {
        public int Key { get; set; }
        public string? StringContent { get; set; }
        public List<string> ImageIds { get; set; }
    }
}
