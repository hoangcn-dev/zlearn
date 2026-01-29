namespace ZLearn.Application.Categories.DTOs
{
    public class CateDetailDto : CateListItemDto
    {
        public string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
