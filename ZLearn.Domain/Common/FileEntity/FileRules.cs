namespace ZLearn.Domain.Common.FileEntity
{
    public class FileRules
    {
        public class Extension
        {
            public const string IMAGE_PNG_EXTENSION = ".png";
            public const string IMAGE_JPEG_EXTENSION = ".jpg";
            public const string IMAGE_GIF_EXTENSION = ".gif";
            public const string IMAGE_SVG_EXTENSION = ".svg";
            public const string AUDIO_MP3_EXTENSION = ".mp3";
            public const string AUDIO_WAV_EXTENSION = ".wav";
            public const string AUDIO_OGG_EXTENSION = ".ogg";
            public const string PDF_EXTENSION = ".pdf";
            public const string DOCX_EXTENSION = ".docx";
            public const string VIDEO_MP4_EXTENSION = ".mp4";
        }

        public class MimeType
        {
            public const string IMAGE_PNG = "image/png";
            public const string IMAGE_JPEG = "image/jpeg";
            public const string IMAGE_GIF = "image/gif";
            public const string IMAGE_SVG = "image/svg+xml";
            public const string AUDIO_MP3 = "audio/mpeg";
            public const string AUDIO_WAV = "audio/wav";
            public const string AUDIO_OGG = "audio/ogg";
            public const string PDF = "application/pdf";
            public const string DOCX = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            public const string VIDEO_MP4 = "video/mp4";
        }

        public class MaxSize
        {
            public const int MAX_IMAGE_FILE_SIZE_MB = 5;
            public const int MAX_AUDIO_FILE_SIZE_MB = 20;
            public const int MAX_VIDEO_FILE_SIZE_MB = 50;
        }
    }
}
