using System;
using Zlearn.V2.Domain.Common;
using Zlearn.V2.Domain.ExamContext.Exams;

namespace Zlearn.V2.Domain.ExamContext.Participants
{
    public class ExamParticipant : BaseEntity
    {
        public string? ParticipantCode { get; private set; }
        public string ParticipantName { get; private set; } = string.Empty;
        public string UserId { get; private set; } = string.Empty;
        public Exam Exam { get; private set; } = null!;
        public string ExamId { get; private set; } = string.Empty;
        public DateTimeOffset? FirstCheckIn { get; private set; }
        public DateTimeOffset? LastCheckOut { get; private set; }
        public ParticipantStatus Status { get; private set; }
        public int Correct { get; private set; }
        public int Completed { get; private set; }
        public bool IsBanned { get; private set; }
        public double Score { get; private set; }
        public string SelectedAnswers { get; private set; } = "[]";

        public ExamParticipant() { }

        public ExamParticipant(
            string id,
            string examId,
            string userId,
            string participantName,
            string? participantCode)
        {
            Id = id;
            ExamId = examId;
            UserId = userId;
            ParticipantName = participantName;
            ParticipantCode = participantCode;
            Status = ParticipantStatus.WaitingForExamStart;
            IsBanned = false;
            SelectedAnswers = "[]";
        }

        public void CheckIn(DateTimeOffset checkInTime)
        {
            if (FirstCheckIn == null)
            {
                FirstCheckIn = checkInTime;
            }
            Status = ParticipantStatus.InProgress;
        }

        public void Submit(
            int completed,
            int correct,
            double score,
            string selectedAnswers,
            DateTimeOffset checkOutTime,
            ParticipantStatus submitStatus)
        {
            Completed = completed;
            Correct = correct;
            Score = score;
            SelectedAnswers = selectedAnswers;
            LastCheckOut = checkOutTime;
            Status = submitStatus;
        }

        public void TimeoutWithReset(DateTimeOffset checkOutTime)
        {
            LastCheckOut = checkOutTime;
            Status = ParticipantStatus.TimeOut;
            Score = 0;
            Correct = 0;
            Completed = 0;
            SelectedAnswers = "[]";
        }

        public void Timeout(DateTimeOffset checkOutTime)
        {
            LastCheckOut = checkOutTime;
            Status = ParticipantStatus.TimeOut;
        }

        public void Ban()
        {
            IsBanned = true;
            Status = ParticipantStatus.NotAllowed;
        }

        public void Unban()
        {
            IsBanned = false;
            Status = ParticipantStatus.ConnectionLost;
        }

        public void ChangeStatus(ParticipantStatus status)
        {
            Status = status;
        }
    }
}
