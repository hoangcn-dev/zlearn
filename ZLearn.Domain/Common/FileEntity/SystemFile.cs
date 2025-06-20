using ZLearn.Domain.Common.FileEntity;
using ZLearn.Domain.Enums;

namespace ZLearn.Domain.Common.FileModel
{
    public class SystemFile
    {
        public SystemFile(string name, byte[] data, string mime, FileType type)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("File data cannot be empty.");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("SystemFile name cannot be empty.");
            if (type == FileType.Image && data.Length / (1024.0 * 1024.0) > FileRules.MaxSize.MAX_IMAGE_FILE_SIZE_MB)
                throw new ArgumentException($"Image file size exceeds the maximum allowed limit of {FileRules.MaxSize.MAX_IMAGE_FILE_SIZE_MB} MB");
            if (type == FileType.Audio && data.Length / (1024.0 * 1024.0) > FileRules.MaxSize.MAX_AUDIO_FILE_SIZE_MB)
                throw new ArgumentException($"Audio file size exceeds the maximum allowed limit of {FileRules.MaxSize.MAX_AUDIO_FILE_SIZE_MB} MB");
            if (type == FileType.Video && data.Length / (1024.0 * 1024.0) > FileRules.MaxSize.MAX_VIDEO_FILE_SIZE_MB)
                throw new ArgumentException($"Video file size exceeds the maximum allowed limit of {FileRules.MaxSize.MAX_VIDEO_FILE_SIZE_MB} MB");

            Extension = mime switch
            {
                FileRules.MimeType.IMAGE_PNG => FileRules.Extension.IMAGE_PNG_EXTENSION,
                FileRules.MimeType.IMAGE_JPEG => FileRules.Extension.IMAGE_JPEG_EXTENSION,
                FileRules.MimeType.IMAGE_GIF => FileRules.Extension.IMAGE_GIF_EXTENSION,
                FileRules.MimeType.IMAGE_SVG => FileRules.Extension.IMAGE_SVG_EXTENSION,
                FileRules.MimeType.AUDIO_MP3 => FileRules.Extension.AUDIO_MP3_EXTENSION,
                FileRules.MimeType.AUDIO_WAV => FileRules.Extension.AUDIO_WAV_EXTENSION,
                FileRules.MimeType.AUDIO_OGG => FileRules.Extension.AUDIO_OGG_EXTENSION,
                FileRules.MimeType.VIDEO_MP4 => FileRules.Extension.VIDEO_MP4_EXTENSION,
                _ => throw new ArgumentException("Unknown mime type.")
            };

            if (type == FileType.Audio)
            {
                if (mime != FileRules.MimeType.AUDIO_MP3 &&
                    mime != FileRules.MimeType.AUDIO_WAV &&
                    mime != FileRules.MimeType.AUDIO_OGG)
                    throw new ArgumentException("Invalid audio file mime.");
            }
            if (type == FileType.Video)
            {
                if (mime != FileRules.MimeType.VIDEO_MP4)
                    throw new ArgumentException("Invalid video file mime.");
            }
            if (type == FileType.Video)
            {
                if (mime == FileRules.MimeType.VIDEO_MP4)
                    throw new ArgumentException("Invalid video file mime.");
            }

            Name = name;
            Data = data;
            Mime = mime;
            Type = type;
        }

        public SystemFile(string name, string url, string mime, FileType type)
        {
            Extension = mime switch
            {
                FileRules.MimeType.IMAGE_PNG => FileRules.Extension.IMAGE_PNG_EXTENSION,
                FileRules.MimeType.IMAGE_JPEG => FileRules.Extension.IMAGE_JPEG_EXTENSION,
                FileRules.MimeType.IMAGE_GIF => FileRules.Extension.IMAGE_GIF_EXTENSION,
                FileRules.MimeType.IMAGE_SVG => FileRules.Extension.IMAGE_SVG_EXTENSION,
                FileRules.MimeType.AUDIO_MP3 => FileRules.Extension.AUDIO_MP3_EXTENSION,
                FileRules.MimeType.AUDIO_WAV => FileRules.Extension.AUDIO_WAV_EXTENSION,
                FileRules.MimeType.AUDIO_OGG => FileRules.Extension.AUDIO_OGG_EXTENSION,
                FileRules.MimeType.VIDEO_MP4 => FileRules.Extension.VIDEO_MP4_EXTENSION,
                _ => throw new ArgumentException("Unknown mime type.")
            };

            if (type == FileType.Audio)
            {
                if (mime != FileRules.MimeType.AUDIO_MP3 &&
                    mime != FileRules.MimeType.AUDIO_WAV &&
                    mime != FileRules.MimeType.AUDIO_OGG)
                    throw new ArgumentException("Invalid audio file mime.");
            }
            if (type == FileType.Video)
            {
                if (mime != FileRules.MimeType.VIDEO_MP4)
                    throw new ArgumentException("Invalid video file mime.");
            }
            if (type == FileType.Video)
            {
                if (mime == FileRules.MimeType.VIDEO_MP4)
                    throw new ArgumentException("Invalid video file mime.");
            }

            Name = name;
            Url = url;
            Mime = mime;
            Type = type;
        }

        public string Name { get; private set; }
        public string Extension { get; private set; }
        public byte[]? Data { get; private set; }
        public string Mime { get; private set; }
        public string? Url { get; private set; }
        public FileType Type { get; private set; }

        public double GetSize(SizeType typeOfSize)
        {
            return typeOfSize switch
            {
                SizeType.Byte => Data.Length,
                SizeType.Kb => Data.Length / 1024.0,
                SizeType.Mb => Data.Length / (1024.0 * 1024.0),
                SizeType.Gb => Data.Length / (1024.0 * 1024.0 * 1024.0),
                _ => throw new ArgumentException("Invalid size type.")
            };
        }

        //public FileType GetType()
        //{
        //    return Mime switch
        //    {
        //        FileRules.MimeType.IMAGE_PNG => FileType.Image,
        //        FileRules.MimeType.IMAGE_JPEG => FileType.Image,
        //        FileRules.MimeType.IMAGE_GIF => FileType.Image,
        //        FileRules.MimeType.IMAGE_SVG => FileType.Image,
        //        FileRules.MimeType.AUDIO_MP3 => FileType.Audio,
        //        FileRules.MimeType.AUDIO_WAV => FileType.Audio,
        //        FileRules.MimeType.AUDIO_OGG => FileType.Audio,
        //        FileRules.MimeType.VIDEO_MP4 => FileType.Video,
        //        _ => throw new ArgumentException("Unknown type.")
        //    };
        //}
    }
}
