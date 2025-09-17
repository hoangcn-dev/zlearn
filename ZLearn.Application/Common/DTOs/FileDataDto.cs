namespace ZLearn.Application.Common.DTOs
{
    public class FileDataDto
    {
        public string FileName { get; set; }
        public Stream StreamData { get; set; }
        public string MIMEType { get; set; }
    }

    public class MIMETypes
    {
        public const string XLSX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string PDF = "application/pdf";
        public const string WORD = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    }
}
