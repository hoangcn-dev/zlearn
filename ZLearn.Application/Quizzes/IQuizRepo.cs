using ZLearn.Application.Common.Interfaces;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes
{
    public interface IQuizRepo : IBaseRepo<Quiz>
    {
        Task<Quiz?> GetFullQuizContent(string quizId);
        Task SetQuestionsTagAsync(Quiz quiz, List<string> tags);
    }
}
