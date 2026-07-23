using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zlearn.V2.Domain.ExamContext.Participants;
using Zlearn.V2.Infas.Identity;

namespace Zlearn.V2.Infas.Data.Configurations
{
    public class ExamParticipantConfiguration : IEntityTypeConfiguration<ExamParticipant>
    {
        public void Configure(EntityTypeBuilder<ExamParticipant> builder)
        {
            builder.ToTable("ExamParticipant");
            builder.Property(ep => ep.ParticipantName).HasMaxLength(50);
            builder.Property(ep => ep.Status).HasConversion<string>().HasMaxLength(50);
            builder.HasOne(ep => ep.Exam)
                .WithMany(e => e.Participants)
                .HasForeignKey(ep => ep.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<AppIdentityUser>()
                .WithMany()
                .HasForeignKey(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ep => new { ep.ExamId, ep.UserId }).IsUnique();
        }
    }
}
