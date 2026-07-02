using System.Collections.Generic;
using System.Threading.Tasks;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Domain.CatalogContext.Quizzes;

namespace Zlearn.V2.Application.Quizzes
{
    public interface IQuizWriteRepo : IWriteRepo<Quiz>
    {
        Task<Quiz?> GetFullQuizContent(string quizId);
        Task SetQuestionsTagAsync(Quiz quiz, List<string> tags);
        Task<List<string>> GetAllTagsAsync();
        Task<bool> HasOngoingExamsAsync(string quizId);
    }
}

