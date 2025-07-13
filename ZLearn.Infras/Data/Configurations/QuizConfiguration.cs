using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Domain.Constants;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Configurations
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.ToTable("Quizzes");

            builder.Property(q => q.Name)
                .HasMaxLength(StringLengths.QuizNameMaxLength)
                .IsRequired();

            builder
                .HasOne(q => q.Category)
                .WithMany(t => t.Quizzes)
                .HasForeignKey(q => q.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(q => q.Tags)
                .WithMany(t => t.Quizzes);
        }
    }
}
