using Microsoft.EntityFrameworkCore;
using ZLearn.Infra.Data.Entities;

namespace ZLearn.Infra.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<QuizEntity> Quizzes { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<TagEntity> Tags { get; set; }
        public DbSet<QuizTagEntity> QuizTags { get; set; }
        public DbSet<QuestionEntity> Questions { get; set; }
        public DbSet<AnswerEntity> Answers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<QuizTagEntity>(qt => 
            {
                qt.HasKey(qt => new { qt.QuizId, qt.TagId });
            });
        }
    }
}
