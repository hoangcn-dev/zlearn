using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zlearn.V2.Domain.FileContext.MediaFiles;

namespace Zlearn.V2.Infas.Data.Configurations
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
            builder.Property(mf => mf.IsUsing).HasDefaultValue(false);
            builder.HasIndex(mf => mf.Id).IsUnique();
            builder.HasIndex(mf => mf.SourceUrl).IsUnique();
        }
    }
}
