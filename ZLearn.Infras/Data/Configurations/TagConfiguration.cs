using ZLearn.Domain.Constants;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.Property(t => t.Name)
                .HasMaxLength(StringLengths.TagNameMaxLength)
                .IsRequired();
        }
    }
}
