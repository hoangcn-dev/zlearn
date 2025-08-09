namespace ZLearn.Application.Quizzes.DTOs
{
    public class QuizListItemDto
    {
        public string Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }
        public int DownloadCount { get; set; }
    }
}
