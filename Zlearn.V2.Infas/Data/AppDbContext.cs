using Microsoft.EntityFrameworkCore;
using Zlearn.V2.Infas.Data.Outbox;
using Zlearn.V2.Infas.Data.Configurations;
using ZLearn.Domain.Entities;

namespace Zlearn.V2.Infas.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.CatalogContext.Categories.Category> Categories => Set<Domain.CatalogContext.Categories.Category>();
        public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
        public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Cấu hình bảng Categories của V2 trỏ vào bảng "Categories" hiện tại của Postgres
            modelBuilder.Entity<Domain.CatalogContext.Categories.Category>(builder =>
            {
                builder.ToTable("Categories");
                builder.HasKey(c => c.Id);
                builder.Property(c => c.Name).IsRequired().HasMaxLength(256);
                builder.HasIndex(c => c.Slug).IsUnique();
            });

            // Cấu hình bảng OutboxEvents của V2
            modelBuilder.Entity<OutboxEvent>(builder =>
            {
                builder.ToTable("OutboxEvents");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Type).IsRequired().HasMaxLength(256);
                builder.Property(x => x.Content).IsRequired();
                builder.Property(x => x.OccurredOn).IsRequired();
                builder.HasIndex(x => x.ProcessedOn);
            });

            // Cấu hình bảng MediaFiles sử dụng MediaFileConfiguration
            modelBuilder.ApplyConfiguration(new MediaFileConfiguration());
        }
    }
}
