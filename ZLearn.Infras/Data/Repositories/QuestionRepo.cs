using System.Linq.Expressions;
using ZLearn.API.Exceptions;
using ZLearn.Application.Quizzes;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Repositories
{
    public class QuestionRepo : BaseRepo<Question>, IQuestionRepo
    {
        public QuestionRepo(AppDbContext context) : base(context)
        {
        }

        public async Task<CorrectAnswerKeyDto> GetCorrectAnswerKeyAsync(string questionId)
        {
            var question = await _context.Set<Question>()
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId)
                ?? throw new NotFoundException(nameof(Question), questionId);
            question.AttemptCount++;
            Update(question);
            await _context.SaveChangesAsync();
            return new CorrectAnswerKeyDto
            {
                QuestionId = question.Id,
                CorrectKeys = question.Answers.Where(a => a.IsCorrect).Select(a => a.Key).ToList(),
                Explanation = question.Explanation ?? "Chưa có giải thích",
            };
        }

        public async Task<Question?> GetQuestionWithAnswers(Expression<Func<Question, bool>> filter)
        {
            var q = await _context.Set<Question>()
                .Include(q => q.Quiz)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(filter);
            return q;
        }
    }
}
