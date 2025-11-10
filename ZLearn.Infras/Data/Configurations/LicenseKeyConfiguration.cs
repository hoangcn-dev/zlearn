using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Configurations
{
    public class LicenseKeyConfiguration : IEntityTypeConfiguration<LicenseKey>
    {
        public void Configure(EntityTypeBuilder<LicenseKey> builder)
        {
            builder.ToTable("LicenseKeys");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Level).HasConversion<string>();
            builder.Property(x => x.Type).HasConversion<string>();
            builder.Property(x => x.Status).HasConversion<string>();
        }
    }
}
