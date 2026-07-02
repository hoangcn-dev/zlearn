namespace Zlearn.V2.Domain.ExamContext.Participants
{
    public enum ParticipantStatus
    {
        WaitingForExamStart,
        InProgress,
        ConnectionLost,
        Completed,
        TimeOut,
        NotAllowed,
    }
}
