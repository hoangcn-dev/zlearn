using System;
using Zlearn.V2.Domain.Common;
using Zlearn.V2.Domain.FileContext.MediaFiles.Events;

namespace Zlearn.V2.Domain.FileContext.MediaFiles
{
    public class MediaFile : AuditableEntity
    {
        public string SourceUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public MediaType Type { get; set; }
        public string Extension { get; set; } = string.Empty;
        public long FileByteSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public double? SecDuration { get; set; }
        public bool IsUsing { get; set; }

        public MediaFile()
        {
        }

        public MediaFile(string sourceUrl, string fileName, MediaType type, string extension, long fileByteSize,
            int? width = null, int? height = null, double? secDuration = null, bool isUsing = false)
        {
            Id = IdGenerator.Generate("FIL");
            SourceUrl = sourceUrl;
            FileName = fileName;
            Type = type;
            Extension = extension;
            FileByteSize = fileByteSize;
            Width = width;
            Height = height;
            SecDuration = secDuration;
            IsUsing = isUsing;

            RaiseEvent(new MediaFileCreatedEvent
            {
                Id = Id,
                SourceUrl = sourceUrl,
                FileName = fileName,
                Type = type,
                Extension = extension,
                FileByteSize = fileByteSize,
                Width = width,
                Height = height,
                SecDuration = secDuration,
                IsUsing = isUsing
            });
        }

        public void Delete()
        {
            RaiseEvent(new MediaFileDeletedEvent(Id, SourceUrl));
        }

        public double GetKbSize() => Math.Round((double)FileByteSize / 1024, 2);
        public double GetMbSize() => Math.Round((double)FileByteSize / (1024 * 1024), 2);
        public double GetGbSize() => Math.Round((double)FileByteSize / (1024 * 1024 * 1024), 2);
    }
}
