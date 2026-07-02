using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Quizzes;
using Zlearn.V2.Domain.CatalogContext.Quizzes;
using Zlearn.V2.Domain.CatalogContext.Tags;
using Zlearn.V2.Infas.Data;
using Zlearn.V2.Domain.ExamContext.Exams;

namespace Zlearn.V2.Infas.Data.Repositories
{
    public class QuizWriteRepo : WriteRepo<Quiz>, IQuizWriteRepo
    {
        public QuizWriteRepo(AppDbContext context) : base(context)
        {
        }

        public async Task<Quiz?> GetFullQuizContent(string quizId)
        {
            return await _context.Set<Quiz>()
                .Include(q => q.Category)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(q => q.Tags)
                .FirstOrDefaultAsync(q => q.Id == quizId.ToUpper());
        }

        public async Task SetQuestionsTagAsync(Quiz quiz, List<string> tags)
        {
            // Remove all old tags
            quiz.Tags.Clear();

            if (tags == null || tags.Count == 0)
                return;

            // Add existing tags and create new ones if they don't exist
            var existingTags = await _context.Set<Tag>()
                .Where(t => tags.Contains(t.Name))
                .ToListAsync();
            quiz.Tags.AddRange(existingTags);
            foreach (var tagName in tags.Except(existingTags.Select(t => t.Name)))
            {
                var newTag = new Tag(IdGenerator.Generate("TAG"), tagName);
                quiz.Tags.Add(newTag);
            }
        }

        public Task<List<string>> GetAllTagsAsync()
        {
            return _context.Set<Tag>()
                .Select(t => t.Name)
                .ToListAsync();
        }

        public async Task<bool> HasOngoingExamsAsync(string quizId)
        {
            return await _context.Exams.AnyAsync(e => 
                e.QuizId == quizId && 
                (e.Status == ExamStatus.WaitStart || e.Status == ExamStatus.InProgress));
        }
    }
}
