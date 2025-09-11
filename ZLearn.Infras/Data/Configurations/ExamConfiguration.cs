using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Configurations
{
    public class ExamConfiguration : IEntityTypeConfiguration<Exam>
    {
        public void Configure(EntityTypeBuilder<Exam> builder)
        {
            builder.Property(e => e.Name).HasMaxLength(ExamRules.NAME_MAX_LENGTH);
            builder.Property(e => e.StartJobId).HasMaxLength(50);
            builder.Property(e => e.EndJobId).HasMaxLength(50);
            builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            builder.Property(e => e.JoinPass).HasMaxLength(ExamRules.JOINPASS_MAX_LENGTH);
            builder.HasOne(e => e.Quiz)
                .WithMany(q => q.Exams)
                .HasForeignKey(e => e.QuizId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasIndex(e => e.Alias).IsUnique();
        }
    }
}
