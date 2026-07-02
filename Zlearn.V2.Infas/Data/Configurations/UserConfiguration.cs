using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zlearn.V2.Domain.FileContext.MediaFiles;
using Zlearn.V2.Infas.Identity;

namespace Zlearn.V2.Infas.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<AppIdentityUser>
    {
        public void Configure(EntityTypeBuilder<AppIdentityUser> builder)
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
