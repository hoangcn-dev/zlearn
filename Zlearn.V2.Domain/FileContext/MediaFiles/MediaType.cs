using System;
using System.Text.Json.Serialization;

namespace Zlearn.V2.Domain.FileContext.MediaFiles
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MediaType
    {
        Image,
        Audio,
        Video
    }

    public static class MediaTypeExtensions
    {
        public static MediaType ToMediaType(this string extension)
        {
            return extension.ToUpperInvariant() switch
            {
                FileRules.Extension.IMAGE_PNG_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_JPG_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_JPEG_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_GIF_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_SVG_EXTENSION => MediaType.Image,
                FileRules.Extension.AUDIO_MP3_EXTENSION => MediaType.Audio,
                FileRules.Extension.AUDIO_WAV_EXTENSION => MediaType.Audio,
                FileRules.Extension.AUDIO_OGG_EXTENSION => MediaType.Audio,
                FileRules.Extension.VIDEO_MP4_EXTENSION => MediaType.Video,
                _ => throw new ArgumentException("Unsupported file extension.")
            };
        }
    }
}
