using ZLearn.Domain.Constants;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Configurations
{
    public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
    {
        public void Configure(EntityTypeBuilder<MediaFile> builder)
        {
            builder.ToTable("MediaFiles");
            builder.Property(mf => mf.SourceUrl)
                .IsRequired()
                .HasMaxLength(StringLengths.UrlMaxLength);
            builder.Property(mf => mf.FileName)
                .IsRequired()
                .HasMaxLength(StringLengths.FileNameMaxLength);
            builder.Property(mf => mf.Type)
                .IsRequired()
                .HasConversion<string>().HasColumnType("varchar(50)");
            builder.HasIndex(mf => mf.Id).IsUnique();
        }
    }
}
