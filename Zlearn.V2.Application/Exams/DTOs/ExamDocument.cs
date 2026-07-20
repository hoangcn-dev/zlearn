using System;

namespace Zlearn.V2.Application.Exams.DTOs
{
    public class ExamDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? JoinPass { get; set; }
        public bool LockAccess { get; set; }
        public bool ShowAnswerAndKey { get; set; }
        public bool MixQuestions { get; set; }
        public bool MixAnswers { get; set; }
        public bool RequireJoinWithCode { get; set; }
        public bool RequireJoinWithName { get; set; }
        public bool AllowLateSubmit { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string QuizId { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset SyncedAt { get; set; }
    }
}



