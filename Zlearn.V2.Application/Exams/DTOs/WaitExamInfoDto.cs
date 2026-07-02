namespace Zlearn.V2.Application.Exams.DTOs
{
    public class WaitExamInfoDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string JoinUrl { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string Note { get; set; }
        public bool RequireJoinWithCode { get; set; }
        public bool RequireJoinWithName { get; set; }
        public bool RequirePassword { get; set; }
    }
}



