using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ZLearn.Application.Exams;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Enums;
using ZLearn.Infras.External.SignalR;

namespace ZLearn.Infras.Realtime.ExamTracking
{
    public class ExamTrackingService : IExamTrackingService
    {
        private readonly IHubContext<ExamHub> _hubContext;

        public ExamTrackingService(IHubContext<ExamHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task AddParticipant(string examId, ParticipantStatusDto participant)
        {
            await _hubContext.Clients.Group(ExamHub.GetExamOwnerGroupName(examId))
                .SendAsync("ParticipantJoined", participant);
        }

        public async Task UpdateParticipantStatus(string examId, string userId, ParticipantStatus status)
        {
            await _hubContext.Clients.Group(ExamHub.GetExamOwnerGroupName(examId))
                .SendAsync("ParticipantStatusUpdated", userId, status.ToString());
        }
    }
}
