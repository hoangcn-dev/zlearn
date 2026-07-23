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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
