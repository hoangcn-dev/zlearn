using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZLearn.Application.Temp.Validations;

namespace ZLearn.Infra.Data.Entities
{
    [Table("Answers")]
    public class AnswerEntity : BaseEntity
    {
        public int Key { get; set; }
        public string? StringContent { get; set; }

        [MaxLength(PropertyLimits.QuestionLimit.URL_LENGTH)]
        public string? ImageUrl { get; set; }

        [Required]
        [ForeignKey(nameof(Question))]
        public string QuestionId { get; set; }   
        public QuestionEntity Question { get; set; }
    }
}
