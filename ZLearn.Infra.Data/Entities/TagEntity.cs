using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZLearn.Application.Temp.Validations;

namespace ZLearn.Infra.Data.Entities
{
    [Table("Tags")]
    public class TagEntity : BaseEntity
    {
        [Required]
        [MaxLength(PropertyLimits.TagLimit.NAME_MAX_LENGTH)]
        public string Name { get; set; }

        public List<QuizTagEntity> QuizTags { get; set; } = new List<QuizTagEntity>();
    }
}
