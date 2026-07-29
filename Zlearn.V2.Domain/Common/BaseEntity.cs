using System.ComponentModel.DataAnnotations;

namespace Zlearn.V2.Domain.Common
{
    public class BaseEntity : AggregateRoot
    {
        [MaxLength(16)]
        public string Id { get; set; }
    }
}
