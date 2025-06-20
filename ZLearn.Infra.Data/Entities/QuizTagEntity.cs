using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZLearn.Infra.Data.Entities
{
    [Table("QuizTags")]
    public class QuizTagEntity
    {
        [Required]
        [ForeignKey(nameof(Quiz))]
        public string QuizId { get; set; }
        public QuizEntity Quiz { get; set; }

        [Required]
        [ForeignKey(nameof(Tag))]
        public string TagId { get; set; }
        public TagEntity Tag { get; set; }
    }
}
