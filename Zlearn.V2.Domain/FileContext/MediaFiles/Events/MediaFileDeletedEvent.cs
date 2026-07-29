using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.FileContext.MediaFiles.Events
{
    public record MediaFileDeletedEvent : DeletedEvent
    {
        public string SourceUrl { get; }

        public MediaFileDeletedEvent(string fileId, string sourceUrl) : base(fileId)
        {
            SourceUrl = sourceUrl;
        }
    }
}
