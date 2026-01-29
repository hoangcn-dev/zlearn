using ZLearn.Application.Common.Identity;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Quizzes;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Repositories
{
    public class QuizRepo : BaseRepo<Quiz>, IQuizRepo
    {

        public QuizRepo(AppDbContext context) : base(context)
        {
        }

        public Task<List<string>> GetAllTagsAsync()
        {
            return _context.Set<Tag>()
                .Select(t => t.Name)
                .ToListAsync();
        }

        public async Task<Quiz?> GetFullQuizContent(string quizId)
        {
            return await _context.Set<Quiz>()
                .Include(q => q.Category)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(q => q.Tags)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task SetQuestionsTagAsync(Quiz quiz, List<string> tags)
        {
            // Remove all old tags
            quiz.Tags.Clear();

            // Add existing tags and create new ones if they don't exist
            var existingTags = await _context.Set<Tag>()
                .Where(t => tags.Contains(t.Name))
                .ToListAsync();
            quiz.Tags.AddRange(existingTags);
            foreach (var tagName in tags.Except(existingTags.Select(t => t.Name)))
            {
                var newTag = new Tag { Id = IdGenerator.Generate("TAG"), Name = tagName };
                quiz.Tags.Add(newTag);
            }
        }
    }
}
