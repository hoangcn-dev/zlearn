using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zlearn.V2.Infas.Data.Outbox;
using Zlearn.V2.Infas.Data.Configurations;
using Zlearn.V2.Infas.Identity;
using Zlearn.V2.Domain.CatalogContext.Categories;
using Zlearn.V2.Domain.CatalogContext.Quizzes;
using Zlearn.V2.Domain.CatalogContext.Questions;
using Zlearn.V2.Domain.CatalogContext.Answers;
using Zlearn.V2.Domain.CatalogContext.Tags;
using Zlearn.V2.Domain.FileContext.MediaFiles;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Infas.Data
{
    public class AppDbContext : IdentityDbContext<AppIdentityUser, AppIdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Quiz> Quizzes => Set<Quiz>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Answer> Answers => Set<Answer>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
        public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
        public DbSet<Exam> Exams => Set<Exam>();
        public DbSet<ExamParticipant> ExamParticipants => Set<ExamParticipant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppIdentityUser>().ToTable("Users");
            modelBuilder.Entity<AppIdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            
            // Cấu hình bảng Categories của V2 trỏ vào bảng "Categories" hiện tại của Postgres
            modelBuilder.Entity<Category>(builder =>
            {
                builder.ToTable("Categories");
                builder.HasKey(c => c.Id);
                builder.Property(c => c.Name).IsRequired().HasMaxLength(256);
                builder.HasIndex(c => c.Slug).IsUnique();
            });

            // Cấu hình bảng Quizzes của V2 trỏ vào bảng "Quizzes" hiện tại của Postgres
            modelBuilder.Entity<Quiz>(builder =>
            {
                builder.ToTable("Quizzes");
                builder.HasKey(q => q.Id);
                builder.Property(q => q.Name).IsRequired().HasMaxLength(255);
                builder.HasIndex(q => q.Slug).IsUnique();
                builder.HasOne(q => q.Category)
                    .WithMany()
                    .HasForeignKey(q => q.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                builder.HasMany(q => q.Tags)
                    .WithMany(t => t.Quizzes)
                    .UsingEntity<Dictionary<string, object>>(
                        "QuizTag",
                        j => j.HasOne<Tag>().WithMany().HasForeignKey("TagsId"),
                        j => j.HasOne<Quiz>().WithMany().HasForeignKey("QuizzesId")
                    );
            });

            // Cấu hình bảng Questions của V2
            modelBuilder.Entity<Question>(builder =>
            {
                builder.ToTable("Questions");
                builder.HasKey(q => q.Id);
                builder.Property(q => q.Slug).IsRequired();
                builder.HasIndex(q => q.Slug).IsUnique();
                builder.HasOne(q => q.Quiz)
                    .WithMany(qz => qz.Questions)
                    .HasForeignKey(q => q.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng Answers của V2
            modelBuilder.Entity<Answer>(builder =>
            {
                builder.ToTable("Answers");
                builder.HasKey(a => a.Id);
                builder.HasOne(a => a.Question)
                    .WithMany(q => q.Answers)
                    .HasForeignKey(a => a.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng Tags của V2
            modelBuilder.Entity<Tag>(builder =>
            {
                builder.ToTable("Tags");
                builder.HasKey(t => t.Id);
                builder.Property(t => t.Name).IsRequired().HasMaxLength(20);
            });

            // Cấu hình bảng OutboxEvents của V2
            modelBuilder.Entity<OutboxEvent>(builder =>
            {
                builder.ToTable("OutboxEvents");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Type).IsRequired().HasMaxLength(256);
                builder.Property(x => x.Content).IsRequired();
                builder.Property(x => x.OccurredOn).IsRequired();
                builder.Property(x => x.AggregateId).IsRequired().HasMaxLength(150);
                builder.HasIndex(x => x.ProcessedOn);
                builder.HasIndex(x => x.AggregateId);
            });

            // Cấu hình bảng MediaFiles sử dụng MediaFileConfiguration
            modelBuilder.ApplyConfiguration(new MediaFileConfiguration());
            modelBuilder.ApplyConfiguration(new ExamConfiguration());
            modelBuilder.ApplyConfiguration(new ExamParticipantConfiguration());
        }
    }
}
