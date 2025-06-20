using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZLearn.Application.Temp.Validations;

namespace ZLearn.Infra.Data.Entities
{
    [Table("Quizzes")]
    public class QuizEntity : BaseEntity
    {
        [Required]
        [MaxLength(PropertyLimits.QuizLimit.NAME_MAX_LENGTH)]
        public string Name { get; set; }

        [Required]
        [ForeignKey(nameof(Category))]
        public string CategoryId { get; set; }
        public CategoryEntity Category { get; set; }

        public List<QuizTagEntity> QuizTags { get; set; } = new List<QuizTagEntity>();
        public List<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
    }
}
