using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Configurations
{
    public class AccessHistoryConfiguration : IEntityTypeConfiguration<AccessHistory>
    {
        public void Configure(EntityTypeBuilder<AccessHistory> builder)
        {
            builder.ToTable("AccessHistories");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Day).IsRequired();
            builder.Property(a => a.AccessCount).IsRequired();
        }
    }
}
