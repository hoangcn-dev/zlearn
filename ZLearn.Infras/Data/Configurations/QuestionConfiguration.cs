using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZLearn.Domain.Constants;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Configurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable("Questions");
            builder
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
