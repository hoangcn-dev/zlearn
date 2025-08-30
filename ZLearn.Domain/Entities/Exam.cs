using ZLearn.Domain.Common;
using ZLearn.Domain.Enums;

namespace ZLearn.Domain.Entities
{
    public class Exam : AuditableEntity
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string? Note { get; set; }
        public string? JoinPass { get; set; }
        public bool LockAccess { get; set; }
        public bool ShowAnswerAndKey { get; set; }
        public bool MixQuestions { get; set; }
        public bool MixAnswers { get; set; }
        public bool RequireJoinWithCode { get; set; }
        public bool RequireJoinWithName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public ExamStatus Status { get; set; }
        public Quiz Quiz { get; set; }
        public string QuizId { get; set; }
        public int MaxParticipants { get; set; }
        public List<ExamParticipant> Participants { get; set; }
    }

    public class ExamRules
    {
        public const int NAME_MAX_LENGTH = 150;
        public const int JOINPASS_MAX_LENGTH = 20;
    }
}
