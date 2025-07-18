using System.Linq.Expressions;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes
{
    public interface IQuestionRepo : IBaseRepo<Question>
    {
        Task<CorrectAnswerKeyDto> GetCorrectAnswerKeyAsync(string questionId);
        Task<Question?> GetQuestionWithAnswers(Expression<Func<Question, bool>> filter);
    }
}
