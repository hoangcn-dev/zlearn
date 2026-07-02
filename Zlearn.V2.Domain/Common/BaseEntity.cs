using System.ComponentModel.DataAnnotations;

namespace Zlearn.V2.Domain.Common
{
    public class BaseEntity
    {
        [MaxLength(16)]
        public string Id { get; set; }
    }
}
