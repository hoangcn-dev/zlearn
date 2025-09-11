using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using ZLearn.Application.Exams;
using ZLearn.Domain.Enums;

namespace ZLearn.Infras.External.SignalR
{
    public class ExamHub : Hub
    {
        private readonly IExamRepo _examRepo;
        private readonly ConcurrentDictionary<string, string> _participantMap;
        private readonly IExamTrackingService _examTrackingService;

        public const string HUB_URL = "/hubs/exam";

        public ExamHub(
            ConcurrentDictionary<string, string> userMap, IExamRepo examRepo, IExamTrackingService examTrackingService)
        {
            _participantMap = userMap;
            _examRepo = examRepo;
            _examTrackingService = examTrackingService;
        }

        public async Task AnswerSelected(int qCount)
        {
            var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_participantMap.TryGetValue(userId, out var examId))
            {
                await Clients.Group(GetExamOwnerGroupName(examId)).SendAsync("ParticipantSelectedAnswer", userId, qCount);
            }
        }

        public async Task UpdateProgress(string examId)
        {
            var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
            if (await _examRepo.IsExamCreator(examId, userId))
            {
                await Clients.Group(GetExamParticipantGroupName(examId)).SendAsync("RequireUpdateProgress");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var examId = Context.GetHttpContext()?.Request.Query["examId"];
            if (Context.User is null || !Context.User.Identity!.IsAuthenticated || string.IsNullOrEmpty(examId))
            {
                Context.Abort();
            }
            var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);

            // Handle exam participant
            if (await _examRepo.IsExamParticipant(examId!, userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetExamParticipantGroupName(examId!));
                _participantMap.AddOrUpdate(userId, examId!, (key, oldValue) => examId!);
                await Clients.Caller.SendAsync("Connected", "Connected!");
                await base.OnConnectedAsync();
                return;
            }

            // Handle exam owner
            if (await _examRepo.IsExamCreator(examId!, userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetExamOwnerGroupName(examId!));
                await Clients.Caller.SendAsync("Connected", "Connected!");
                await base.OnConnectedAsync();
                return;
            }

            // Invalid user
            Context.Abort();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_participantMap.TryRemove(userId, out var examId))
            {
                var status = await _examRepo.SetParticipantStatus(userId, examId, ParticipantStatus.ConnectionLost);
                await _examTrackingService.UpdateParticipantStatus(examId, userId, status);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetExamParticipantGroupName(examId!));
            }
            else
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetExamOwnerGroupName(examId!));
            }    
            await base.OnDisconnectedAsync(exception);
        }

        public static string GetExamOwnerGroupName(string examId) => $"{examId}_owner";
        public static string GetExamParticipantGroupName(string examId) => $"{examId}_participant";
    }
}
