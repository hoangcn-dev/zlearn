using ZLearn.Domain.Constants;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Files
{
    public class FileHelper
    {
        public static MediaType GetMediaType(string url)
        {
            var mediaType = Path.GetExtension(url) switch
            {
                FileRules.Extension.IMAGE_PNG_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_JPEG_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_JPG_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_GIF_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_SVG_EXTENSION => MediaType.Image,
                FileRules.Extension.AUDIO_MP3_EXTENSION => MediaType.Audio,
                FileRules.Extension.AUDIO_WAV_EXTENSION => MediaType.Audio,
                FileRules.Extension.AUDIO_OGG_EXTENSION => MediaType.Audio,
                FileRules.Extension.VIDEO_MP4_EXTENSION => MediaType.Video,
                _ => throw new ArgumentException("Unsupported file extension."),
            };

            return mediaType;
        }
    }
}
