using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zlearn.V2.Domain.CatalogContext.Quizzes;
using Zlearn.V2.Domain.CatalogContext.Tags;

namespace Zlearn.V2.Infas.Data.Configurations
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.ToTable("Quizzes");
            builder.HasKey(q => q.Id);
            builder.Property(q => q.Name).IsRequired().HasMaxLength(255);
            builder.HasIndex(q => q.Slug).IsUnique();

            builder.HasOne(q => q.Category)
                .WithMany()
                .HasForeignKey(q => q.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(q => q.Tags)
                .WithMany(t => t.Quizzes)
                .UsingEntity<Dictionary<string, object>>(
                    "QuizTag",
                    j => j.HasOne<Tag>().WithMany().HasForeignKey("TagsId"),
                    j => j.HasOne<Quiz>().WithMany().HasForeignKey("QuizzesId"),
                    j =>
                    {
                        j.HasIndex("QuizzesId", "TagsId").IsUnique();
                    }
                );
        }
    }
}
