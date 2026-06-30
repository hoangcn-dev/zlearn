using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZLearn.Infras.Data.Outbox;

namespace ZLearn.Infras.Data.Configurations
{
    public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
    {
        public void Configure(EntityTypeBuilder<OutboxEvent> builder)
        {
            builder.ToTable("OutboxEvents");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Content).IsRequired();
            builder.Property(x => x.OccurredOn).IsRequired();
            
            builder.HasIndex(x => x.ProcessedOn);
        }
    }
}
