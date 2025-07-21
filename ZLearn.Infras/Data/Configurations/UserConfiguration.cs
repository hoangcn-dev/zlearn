using ZLearn.Domain.Constants;
using ZLearn.Infras.Identity;

namespace ZLearn.Infras.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(u => u.FirstName)
                .HasMaxLength(StringLengths.FirstNameMaxLength)
                .IsRequired(false);

            builder.Property(u => u.LastName)
                .HasMaxLength(StringLengths.LastNameMaxLength)
                .IsRequired(false);

            builder.Property(u => u.NickName)
                .HasMaxLength(StringLengths.NickNameMaxLength)
                .IsRequired();
        }
    }
}
