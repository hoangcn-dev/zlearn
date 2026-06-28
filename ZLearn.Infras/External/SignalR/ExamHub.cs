using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using ZLearn.Application.Exams;
using ZLearn.Domain.Enums;
using ZLearn.Infras.External.Redis;

namespace ZLearn.Infras.External.SignalR
{
    public class ExamHub : Hub
    {
        private readonly IExamRepo _examRepo;
        private readonly IRedisService _redisService;
        private readonly IExamTrackingService _examTrackingService;

        public const string HUB_URL = "/hubs/exam";

        public ExamHub(
            IRedisService redisService, 
            IExamRepo examRepo, 
            IExamTrackingService examTrackingService)
        {
            _redisService = redisService;
            _examRepo = examRepo;
            _examTrackingService = examTrackingService;
        }

        public async Task AnswerSelected(int qCount)
        {
            var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                // Fallback: try to read from cookie session token if claims is not present
                var token = Context.GetHttpContext()?.Request.Cookies["exam_session_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    var session = await _redisService.GetObject<ExamSessionDto>(RedisKeys.EXAM_SESSION, token);
                    if (session is not null)
                    {
                        userId = session.u;
                    }
                }
            }

            if (userId is not null)
            {
                var examId = await _redisService.Get(RedisKeys.EXAM_PARTICIPANT_MAP, userId);
                if (examId is not null)
                {
                    await Clients.Group(GetExamOwnerGroupName(examId)).SendAsync("ParticipantSelectedAnswer", userId, qCount);
                }
            }
        }

        public async Task UpdateProgress(string examId)
        {
            var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null && await _examRepo.IsExamCreator(examId, userId))
            {
                await Clients.Group(GetExamParticipantGroupName(examId)).SendAsync("RequireUpdateProgress");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var token = httpContext?.Request.Cookies["exam_session_token"];

            // 1. Handle exam participant via cookie & Redis
            if (!string.IsNullOrEmpty(token))
            {
                var session = await _redisService.GetObject<ExamSessionDto>(RedisKeys.EXAM_SESSION, token);
                if (session is not null)
                {
                    var userId = session.u;
                    var examId = session.e;

                    if (await _examRepo.Any(e => e.Id == examId && e.Status == ExamStatus.Ended))
                    {
                        Context.Abort();
                        return;
                    }

                    await Groups.AddToGroupAsync(Context.ConnectionId, GetExamParticipantGroupName(examId));
                    
                    // Store connection mapping in Redis (TTL: 12 hours)
                    await _redisService.Set(RedisKeys.EXAM_PARTICIPANT_MAP, userId, examId, TimeSpan.FromHours(12));
                    
                    // Cancel grace period if the user reconnected
                    await _redisService.Delete(RedisKeys.EXAM_SESSION_DISCONNECT, userId);

                    await Clients.Caller.SendAsync("Connected", "Connected!");
                    await base.OnConnectedAsync();
                    return;
                }
            }

            // 2. Handle exam owner via claims & query param
            if (Context.User is not null && Context.User.Identity!.IsAuthenticated)
            {
                var examId = httpContext?.Request.Query["examId"].ToString();
                var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(examId) && userId is not null && await _examRepo.IsExamCreator(examId, userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, GetExamOwnerGroupName(examId));
                    await Clients.Caller.SendAsync("Connected", "Connected!");
                    await base.OnConnectedAsync();
                    return;
                }
            }

            // Invalid user
            Context.Abort();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                // Fallback: try to read from cookie session token if claims is not present
                var token = Context.GetHttpContext()?.Request.Cookies["exam_session_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    var session = await _redisService.GetObject<ExamSessionDto>(RedisKeys.EXAM_SESSION, token);
                    if (session is not null)
                    {
                        userId = session.u;
                    }
                }
            }

            if (userId is not null)
            {
                var examId = await _redisService.Get(RedisKeys.EXAM_PARTICIPANT_MAP, userId);
                if (examId is not null)
                {
                    // Delete mapping from Redis
                    await _redisService.Delete(RedisKeys.EXAM_PARTICIPANT_MAP, userId);

                    // Record disconnect in Redis (TTL: 30 seconds)
                    var disconnectTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    await _redisService.Set(RedisKeys.EXAM_SESSION_DISCONNECT, userId, $"{examId}:{disconnectTime}", TimeSpan.FromSeconds(30));

                    // Perform background status check after 15 seconds grace period
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(15000);
                        var disconnectVal = await _redisService.Get(RedisKeys.EXAM_SESSION_DISCONNECT, userId);
                        if (disconnectVal is not null)
                        {
                            var parts = disconnectVal.Split(':');
                            if (parts.Length > 0)
                            {
                                var targetExamId = parts[0];
                                var status = await _examRepo.SetParticipantStatus(userId, targetExamId, ParticipantStatus.ConnectionLost);
                                await _examTrackingService.UpdateParticipantStatus(targetExamId, userId, status);
                            }
                            await _redisService.Delete(RedisKeys.EXAM_SESSION_DISCONNECT, userId);
                        }
                    });
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static string GetExamOwnerGroupName(string examId) => $"{examId}_owner";
        public static string GetExamParticipantGroupName(string examId) => $"{examId}_participant";
    }
}
