using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.FileContext.MediaFiles.Events
{
    public record MediaFileCreatedEvent : DomainEvent
    {
        public override string AggregateId => Id;
        public string Id { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public MediaType Type { get; set; }
        public string Extension { get; set; } = string.Empty;
        public long FileByteSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public double? SecDuration { get; set; }
        public bool IsUsing { get; set; }
    }
}
