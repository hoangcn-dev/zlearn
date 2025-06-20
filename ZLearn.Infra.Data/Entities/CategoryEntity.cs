using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZLearn.Application.Temp.Validations;

namespace ZLearn.Infra.Data.Entities
{
    [Table("Categories")]
    public class CategoryEntity : BaseEntity
    {
        [Required]
        [MaxLength(PropertyLimits.CategoryLimit.NAME_MAX_LENGTH)]
        public string Name { get; set; }

        public List<QuizEntity> Quizzes { get; set; } = new List<QuizEntity>();
    }
}
