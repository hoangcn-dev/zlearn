namespace ZLearn.Application.Quizzes.DTOs
{
    public class ExportDocumentDto
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}
