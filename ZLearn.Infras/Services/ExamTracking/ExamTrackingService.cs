using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Reflection;
using ZLearn.Application.Exams;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Enums;
using ZLearn.Infras.External.SignalR;

namespace ZLearn.Infras.Services.ExamTracking
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

        public async Task UpdateExamStatus(string examId, ExamStatus status)
        {
            await _hubContext.Clients.Group(ExamHub.GetExamParticipantGroupName(examId))
                .SendAsync("ExamStatusChange", status.ToString());
            await _hubContext.Clients.Group(ExamHub.GetExamOwnerGroupName(examId))
                .SendAsync("ExamStatusChange", status.ToString());
        }

        public async Task UpdateParticipantStatus(string examId, string userId, ParticipantStatus status)
        {
            await _hubContext.Clients.Group(ExamHub.GetExamOwnerGroupName(examId))
                .SendAsync("ParticipantStatusUpdated", userId, status.ToString());
        }

        public async Task UpdateRemainingTime(string examId, long remainingMilisec, string methodName)
        {
            await _hubContext.Clients.Group(ExamHub.GetExamParticipantGroupName(examId))
                .SendAsync(methodName, remainingMilisec);
            await _hubContext.Clients.Group(ExamHub.GetExamOwnerGroupName(examId))
                .SendAsync(methodName, remainingMilisec);
        }

        public async Task UpdateParticipantProgress(string examId, string userId, int completedCount)
        {
            await _hubContext.Clients.Group(ExamHub.GetExamOwnerGroupName(examId))
                .SendAsync("ParticipantSelectedAnswer", userId, completedCount);
        }
    }
}
