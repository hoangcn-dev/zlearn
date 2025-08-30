
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Configurations
{
    public class ExamParticipantConfiguration : IEntityTypeConfiguration<ExamParticipant>
    {
        public void Configure(EntityTypeBuilder<ExamParticipant> builder)
        {
            builder.Property(ep => ep.ParticipantName).HasMaxLength(50);
            builder.Property(ep => ep.Status).HasConversion<string>().HasMaxLength(50);
            builder.HasOne(ep => ep.Exam)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
