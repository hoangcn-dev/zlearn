using ZLearn.Domain.Common;
using ZLearn.Domain.Constants;
using ZLearn.Domain.Enums;

namespace ZLearn.Domain.Entities
{
    public class MediaFile : AuditableEntity
    {
        public string SourceUrl { get; set; }
        public string FileName { get; set; }
        public MediaType Type { get; set; }
        public string Extension { get; set; }
        public long FileByteSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public double? SecDuration { get; set; }
        public bool IsUsing { get; set; }


        public double GetKbSize() => Math.Round((double)FileByteSize / 1024, 2);
        public double GetMbSize() => Math.Round((double)FileByteSize / (1024 * 1024), 2);
        public double GetGbSize() => Math.Round((double)FileByteSize / (1024 * 1024 * 1024), 2);
    }
}
