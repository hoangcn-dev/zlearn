using Zlearn.V2.Domain.CatalogContext.Quizzes;
using Zlearn.V2.Domain.Common;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Domain.ExamContext.Exams
{
    public class Exam : AuditableEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Alias { get; private set; } = string.Empty;
        public string? Note { get; private set; }
        public string? JoinPass { get; private set; }
        public bool LockAccess { get; private set; }
        public bool ShowAnswerAndKey { get; private set; }
        public bool MixQuestions { get; private set; }
        public bool MixAnswers { get; private set; }
        public bool RequireJoinWithCode { get; private set; }
        public bool RequireJoinWithName { get; private set; }
        public bool AllowLateSubmit { get; private set; }
        public DateTimeOffset StartTime { get; private set; }
        public string? StartJobId { get; private set; }
        public string? EndJobId { get; private set; }
        public DateTimeOffset? EndTime { get; private set; }
        public ExamStatus Status { get; private set; }
        public Quiz Quiz { get; private set; } = null!;
        public string QuizId { get; private set; } = string.Empty;
        public int MaxParticipants { get; private set; }
        public List<ExamParticipant> Participants { get; private set; } = new List<ExamParticipant>();

        public Exam() { }

        public Exam(
            string name,
            string alias,
            string quizId,
            string? joinPass,
            DateTimeOffset startTime,
            DateTimeOffset? endTime,
            ExamStatus status,
            bool showAnswerAndKey,
            int maxParticipants,
            bool mixAnswers,
            bool mixQuestions,
            bool requireJoinWithCode,
            bool requireJoinWithName,
            bool allowLateSubmit,
            string? note,
            string? id = null)
        {
            Id = string.IsNullOrEmpty(id) ? IdGenerator.Generate("EXA") : id;
            Name = name;
            Alias = alias;
            QuizId = quizId;
            JoinPass = joinPass;
            StartTime = startTime;
            EndTime = endTime;
            Status = status;
            LockAccess = false;
            ShowAnswerAndKey = showAnswerAndKey;
            MaxParticipants = maxParticipants;
            MixAnswers = mixAnswers;
            MixQuestions = mixQuestions;
            RequireJoinWithCode = requireJoinWithCode;
            RequireJoinWithName = requireJoinWithName;
            AllowLateSubmit = allowLateSubmit;
            Note = note;

            RaiseEvent(new Events.ExamCreatedEvent(
                Id,
                Name,
                Alias,
                QuizId,
                StartTime,
                EndTime,
                Status.ToString(),
                Note,
                JoinPass,
                LockAccess,
                ShowAnswerAndKey,
                MixQuestions,
                MixAnswers,
                RequireJoinWithCode,
                RequireJoinWithName,
                AllowLateSubmit,
                MaxParticipants
            ));
        }

        public ExamParticipant AddParticipant(
            string userId,
            string participantName,
            string? participantCode,
            string? joinPassword,
            string? participantId = null)
        {
            if (Status == ExamStatus.Ended)
                throw new InvalidOperationException("Bài kiểm tra đã kết thúc.");

            if (Participants.Count >= MaxParticipants)
                throw new InvalidOperationException("Bài kiểm tra đã đủ số lượng người tham gia.");

            if (RequireJoinWithCode && string.IsNullOrEmpty(participantCode))
                throw new InvalidOperationException("Vui lòng nhập mã tham gia.");

            if (RequireJoinWithName && string.IsNullOrEmpty(participantName))
                throw new InvalidOperationException("Vui lòng nhập tên tham gia.");

            if (!string.IsNullOrEmpty(JoinPass) && JoinPass != joinPassword)
                throw new InvalidOperationException("Mật khẩu tham gia không đúng, vui lòng thử lại");

            // Check if user already joined
            if (Participants.Any(p => p.UserId == userId))
                throw new InvalidOperationException("Bạn đã tham gia bài kiểm tra");

            // Check if participant code already used
            if (RequireJoinWithCode && Participants.Any(p => p.ParticipantCode == participantCode))
                throw new InvalidOperationException("Mã tham gia đã được sử dụng");

            var participant = new ExamParticipant(Id, userId, participantName, participantCode, participantId);
            Participants.Add(participant);
            return participant;
        }

        public void ChangeStatus(ExamStatus status)
        {
            Status = status;
        }

        public void Start(DateTimeOffset startTime)
        {
            Status = ExamStatus.InProgress;
            StartTime = startTime;
            RaiseEvent(new Events.ExamStartedEvent(Id, startTime));
        }

        public void SetLockAccess(bool lockAccess)
        {
            LockAccess = lockAccess;
        }

        public void UpdateJobIds(string? startJobId, string? endJobId)
        {
            StartJobId = startJobId;
            EndJobId = endJobId;
        }

        public void End(DateTimeOffset endedAt)
        {
            Status = ExamStatus.Ended;
            EndTime = endedAt;
            if (!AllowLateSubmit)
            {
                foreach (var p in Participants)
                {
                    if (p.Status != ParticipantStatus.Completed)
                    {
                        p.TimeoutWithReset(endedAt);
                    }
                }
            }
            RaiseEvent(new Events.ExamEndedEvent(Id, endedAt));
        }

        public void Delete()
        {
            RaiseEvent(new Events.ExamDeletedEvent(Id));
        }
    }

    public class ExamRules
    {
        public const int NAME_MAX_LENGTH = 150;
        public const int JOINPASS_MAX_LENGTH = 20;
    }
}
