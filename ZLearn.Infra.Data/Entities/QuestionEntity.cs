using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZLearn.Application.Temp.Validations;

namespace ZLearn.Infra.Data.Entities
{
    [Table("Questions")]
    public class QuestionEntity : BaseEntity
    {
        public string? StringContent { get; set; }

        [MaxLength(PropertyLimits.QuestionLimit.URL_LENGTH)]
        public string? ImageUrl { get; set; }

        [MaxLength(PropertyLimits.QuestionLimit.URL_LENGTH)]
        public string? AudioUrl { get; set; }

        public int CorrectKey { get; set; }
        public int Order { get; set; }
        public List<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();

        [Required]
        [ForeignKey(nameof(Quiz))]
        public string QuizId { get; set; }
        public QuizEntity Quiz { get; set; }
    }
}
